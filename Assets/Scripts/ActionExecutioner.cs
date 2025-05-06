using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ActionExecutioner : MonoBehaviour
{
    public class ActionCommand
    {
        public string ActionType;
        public Dictionary<string, string> Arguments = new Dictionary<string, string>();
    }
    public static ActionExecutioner Instance = null; 
    private Dictionary<string, Color> namedColors = new Dictionary<string, Color>()
    {
        { "red", Color.red },
        { "green", Color.green },
        { "blue", Color.blue },
        { "yellow", Color.yellow },
        { "white", Color.white },
        { "black", Color.black },
        { "gray", Color.gray },
        { "cyan", Color.cyan },
        { "magenta", Color.magenta }
    };

    // Start is called before the first frame update
    void Start()
    {
        if (Instance == null) {
            Instance = this; 
        } else {
            Destroy(Instance);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Execute(string response) {
        ActionCommand command = ParseLLMResponse(response); 
        if (command.ActionType == "selection") {
            ExecuteSelection(command); 
        } else {

        }
    }

    public void ExecuteSelection(ActionCommand actionCommand) {
        string objectType = actionCommand.Arguments["object_type"];

        // Step 1: Gather all matching objects by tag
        List<GameObject> candidates = new List<GameObject>();
        List<GameObject> list_temp = new List<GameObject>();
        foreach (var obj in SelectorManager.Instance.currentTargets)
        {
            if (obj.CompareTag(objectType))
            {
                candidates.Add(obj);
            }
        }

        // Step 2: Apply optional filters
        if (actionCommand.Arguments.TryGetValue("color", out string colorFilter))
        {
            if (TryGetColorFromName(colorFilter, out Color expectedColor))
            {
                const float colorTolerance = 0.1f; // Allow slight variation

                candidates = candidates.FindAll(obj =>
                {
                    Renderer renderer = obj.GetComponent<Renderer>();
                    if (renderer == null) return false;

                    Color actualColor = renderer.material.color;
                    return ColorsApproximatelyEqual(actualColor, expectedColor, colorTolerance);
                });
            }
        }

        if (actionCommand.Arguments.TryGetValue("location", out string location))
        {
            candidates = ApplySpatialFilter(candidates, location);

            // if someone already included a specific location to select from, normally they want one specific object unless quantity is set explicitly 
            if (!actionCommand.Arguments.ContainsKey("quantity")) {
                actionCommand.Arguments["quantity"] = "1";
            }
        }

        if (actionCommand.Arguments.TryGetValue("quantity", out string quantityStr) && int.TryParse(quantityStr, out int quantity))
        {
            candidates = candidates.Take(quantity).ToList();
        }

        // Step 4: Deselect anything no longer in candidates
        foreach (var obj in SelectorManager.Instance.currentTargets)
        {
            if (!candidates.Contains(obj))
            {
                obj.GetComponent<ObjectController>()?.ToggleHighlight();
                list_temp.Add(obj);
            }
        }

        foreach (var obj in list_temp) {
            SelectorManager.Instance.RemoveFromSelection(obj);
        }
    }

    private List<GameObject> ApplySpatialFilter(List<GameObject> objects, string filter)
    {
        Transform player = Camera.main.transform; 
        string lower = filter.ToLower();

        if (lower.Contains("left"))
        {
            return objects.OrderByDescending(obj =>
                Vector3.Dot((obj.transform.position - player.position).normalized, -player.right)).ToList();
        }
        else if (lower.Contains("right"))
        {
            return objects.OrderByDescending(obj =>
                Vector3.Dot((obj.transform.position - player.position).normalized, player.right)).ToList();
        }
        else if (lower.Contains("closest") || lower.Contains("nearest"))
        {
            return objects.OrderBy(obj =>
                Vector3.Distance(obj.transform.position, player.position)).ToList();
        }
        else if (lower.Contains("farthest") || lower.Contains("furthest"))
        {
            return objects.OrderByDescending(obj =>
                Vector3.Distance(obj.transform.position, player.position)).ToList();
        }
        else if (lower.Contains("highest") || lower.Contains("top"))
        {
            return objects.OrderByDescending(obj =>
                obj.transform.position.y).ToList();
        }
        else if (lower.Contains("lowest") || lower.Contains("bottom"))
        {
            return objects.OrderBy(obj =>
                obj.transform.position.y).ToList();
        }
        return objects;
    }

    private bool TryGetColorFromName(string name, out Color color)
    {
        name = name.ToLower();
        return namedColors.TryGetValue(name, out color);
    }

    private bool ColorsApproximatelyEqual(Color a, Color b, float tolerance)
    {
        return Mathf.Abs(a.r - b.r) < tolerance &&
            Mathf.Abs(a.g - b.g) < tolerance &&
            Mathf.Abs(a.b - b.b) < tolerance;
    }

    public ActionCommand ParseLLMResponse(string response)
    {
        var command = new ActionCommand();
        var lines = response.Split('\n');
        int num = 0; 
        foreach (var line in lines)
        {
            num++; 
            if (string.IsNullOrWhiteSpace(line)) continue;
            if (num == 1 && !line.Contains("action_type")) {
                if (line.Contains("select")) {
                    command.ActionType = "selection"; 
                } else if (line.Contains("rotation")) {
                    command.ActionType = "rotation";
                } else if (line.Contains("translation")) {
                    command.ActionType = "translation";
                } else if (line.Contains("scale")) {
                    command.ActionType = "scale";
                }
                continue; 
            }
            var parts = line.Split(new[] { ':' }, 2);
            if (parts.Length != 2) continue;

            string key = parts[0].Trim().ToLower();
            string value = parts[1].Trim().ToLower();

            if (key == "action_type")
                command.ActionType = value;
            else
                command.Arguments[key] = value;
        }
        return command;
    }
}
