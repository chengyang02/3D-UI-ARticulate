using System;
using UnityEngine;
using System.Linq;
using System.Collections.Generic;


public class VoiceCommandHandler : MonoBehaviour
{
    [Serializable]
    private class VoiceAction
    {
        public string action_type;
        public Target target;
        public string axis;
        public string angle;
        public string direction;
        public string distance;
        public string scale_factor;
        public string color;
    }

    [Serializable]
    private class Target
    {
        public string color;
        public string object_type;
    }

    public void HandleVoiceCommand(string command)
    {
        Debug.Log($"🗣️ Received command: {command}");
        string lowerCommand = command.ToLower();

        // Handle selection commands first
        if (lowerCommand.Contains("select") || lowerCommand.Contains("only"))
        {
            HandleSelectionCommand(lowerCommand);
            return;
        }

        // Try JSON parsing if LLM generated structured input
        if (lowerCommand.Trim().StartsWith("{"))
        {
            try
            {
                VoiceAction action = JsonUtility.FromJson<VoiceAction>(command);
                if (action != null && action.target != null)
                {
                    ExecuteStructuredAction(action);
                    return;
                }
            }
            catch (Exception e)
            {
                Debug.LogWarning($"⚠️ JSON parsing failed: {e.Message}");
            }
        }

        // Fallback: original simple format e.g. "rotate blue book"
        string[] actions = { "move", "rotate", "scale", "color" };
        string[] colors = { "blue", "brown","red", "green", "yellow", "black", "white" };
        string[] types = { "book","notebook","hat","cube", "chair", "table", "plant", "camera", "speaker","cube",
        "sphere" };

        string actionType = actions.FirstOrDefault(a => lowerCommand.Contains(a));
        string color = colors.FirstOrDefault(c => lowerCommand.Contains(c));
        string type = types.FirstOrDefault(t => lowerCommand.Contains(t));

        if (actionType == null || type == null)
        {
            Debug.LogWarning($"⚠️ Command parsing failed. Action: {actionType}, Type: {type}");
            return;
        }


        GameObject[] targets = GameObject.FindGameObjectsWithTag("Interactable");
        foreach (var obj in targets)
        {
            var meta = obj.GetComponent<VoiceControllable>();
            if (meta != null &&
                meta.objectType.ToLower() == type &&
                (string.IsNullOrEmpty(color) || meta.color.ToLower() == color))
            {
                ApplyAction(obj, actionType);
            }
        }
    }

    private void ExecuteStructuredAction(VoiceAction action)
    {
        string type = action.target.object_type.ToLower();
        string color = action.target.color?.ToLower(); // optional

        if (action.action_type.ToLower() == "selection")
        {
            List<GameObject> selected = new();
            GameObject[] all = GameObject.FindGameObjectsWithTag("Interactable");

            foreach (var obj in all)
            {
                var meta = obj.GetComponent<VoiceControllable>();
                if (meta != null && meta.objectType.ToLower() == type &&
                    (string.IsNullOrEmpty(color) || meta.color.ToLower() == color))
                {
                    selected.Add(obj);
                }
            }

            Debug.Log($"✅ Selecting {selected.Count} objects: " + string.Join(", ", selected.Select(o => o.name)));
            SelectionState.Instance.SelectObjects(selected);
            return;
        }

        // Otherwise: handle normal actions like move, rotate, scale
        GameObject[] targets = GameObject.FindGameObjectsWithTag("Interactable");
        foreach (var obj in targets)
        {
            var meta = obj.GetComponent<VoiceControllable>();
            if (meta != null &&
                meta.objectType.ToLower() == type &&
                (string.IsNullOrEmpty(color) || meta.color.ToLower() == color))
            {
                ApplyStructuredAction(obj, action);
            }
        }
    }

    private void HandleSelectionCommand(string command)
    {
        string[] colors = { "blue", "red", "green", "yellow", "black", "white" };
        string[] types = { "book", "cube", "chair", "table", "plant", "camera", "speaker", "hat" };

        string color = colors.FirstOrDefault(c => command.Contains(c));
        string type = types.FirstOrDefault(t => command.Contains(t));

        if (color == null || type == null)
        {
            Debug.LogWarning($"⚠️ Selection command parsing failed. Color: {color}, Type: {type}");
            return;
        }

        // Get the SelectorManager instance
        var selectorManager = SelectorManager.Instance;
        if (selectorManager == null)
        {
            Debug.LogError("❌ SelectorManager instance not found!");
            return;
        }

        // Find all interactable objects
        GameObject[] allObjects = GameObject.FindGameObjectsWithTag("Interactable");
        
        // First, deselect everything
        foreach (var obj in allObjects)
        {
            var interactable = obj.GetComponent<UnityEngine.XR.Interaction.Toolkit.XRGrabInteractable>();
            if (interactable != null)
            {
                interactable.enabled = false;
            }
        }

        // Then, select only the matching object
        foreach (var obj in allObjects)
        {
            var meta = obj.GetComponent<VoiceControllable>();
            if (meta != null &&
                meta.objectType.ToLower() == type &&
                meta.color.ToLower() == color)
            {
                var interactable = obj.GetComponent<UnityEngine.XR.Interaction.Toolkit.XRGrabInteractable>();
                if (interactable != null)
                {
                    interactable.enabled = true;
                    Debug.Log($"✅ Selected {obj.name}");
                }
            }
        }
    }

    void ApplyAction(GameObject obj, string action)
    {
        switch (action)
        {
            case "scale":
                obj.transform.localScale *= 1.2f;
                Debug.Log($"✅ Scaled {obj.name}");
                break;
            case "move":
                obj.transform.position += Vector3.up * 0.5f;
                Debug.Log($"✅ Moved {obj.name} upward");
                break;
            case "rotate":
                obj.transform.Rotate(Vector3.up, 45f);
                Debug.Log($"✅ Rotated {obj.name} around Y-axis");
                break;
            case "color":
                var renderer = obj.GetComponent<Renderer>();
                if (renderer != null)
                {
                    renderer.material.color = Color.green;
                    Debug.Log($"✅ Changed color of {obj.name} to green");
                }
                else
                {
                    Debug.LogWarning($"⚠️ No Renderer found on {obj.name}");
                }
                break;
            default:
                Debug.LogWarning($"🚫 Unknown action: {action}");
                break;
        }
    }

    void ApplyStructuredAction(GameObject obj, VoiceAction action)
    {
        switch (action.action_type.ToLower())
        {
            case "rotation":
                float angle = ParseFloat(action.angle, 45f);
                Vector3 axis = ParseAxis(action.axis);
                obj.transform.Rotate(axis, angle);
                Debug.Log($"✅ Rotated {obj.name} {angle}° around {axis}");
                break;

            case "translation":
            case "move":
                float dist = ParseFloat(action.distance, 0.5f);
                Vector3 dir = obj.transform.TransformDirection(ParseDirection(action.direction));
                Rigidbody rb = obj.GetComponent<Rigidbody>();
                if (rb != null)
                {
                    Vector3 move = dir * dist;
                    move.y = 0f; // ✅ Don't affect gravity
                    rb.MovePosition(rb.position + move);
                }
                else
                {
                    obj.transform.position += dir * dist;
                }
                Debug.Log($"✅ Moved {obj.name} {dist}m toward {dir}");
                break;

            case "scale":
                float factor = ParseFloat(action.scale_factor, 1.2f);
                obj.transform.localScale *= factor;
                Debug.Log($"✅ Scaled {obj.name} by {factor}");
                break;

            case "color":
                var renderer = obj.GetComponent<Renderer>();
                if (renderer != null)
                {
                    string clr = action.color ?? action.target.color;
                    renderer.material.color = ColorFromString(clr);
                    Debug.Log($"✅ Colored {obj.name} as {clr}");
                }
                break;

            default:
                Debug.LogWarning($"🚫 Unknown action: {action.action_type}");
                break;
        }
    }



    float ParseFloat(string input, float fallback)
    {
        if (string.IsNullOrWhiteSpace(input)) return fallback;
        input = input.ToLower().Replace("meters", "").Replace("degrees", "").Trim();
        return float.TryParse(input, out float value) ? value : fallback;
    }

    Vector3 ParseAxis(string axis)
    {
        if (string.IsNullOrWhiteSpace(axis)) return Vector3.up;

        axis = axis.ToLower();

        Vector3 result = Vector3.zero;

        if (axis.Contains("x")) result += Vector3.right;
        if (axis.Contains("y")) result += Vector3.up;
        if (axis.Contains("z")) result += Vector3.forward;

        // Normalize so it rotates evenly if it's multiple axes
        return result == Vector3.zero ? Vector3.up : result.normalized;
    }


    Vector3 ParseDirection(string dir)
    {
        return dir?.ToLower() switch
        {
            "up" => Vector3.up,
            "down" => Vector3.down,
            "left" => Vector3.left,
            "right" => Vector3.right,
            "forward" => Vector3.forward,
            "backward" => Vector3.back,
            _ => Vector3.zero
        };
    }

    Color ColorFromString(string color)
    {
        return color.ToLower() switch
        {
            "red" => Color.red,
            "blue" => Color.blue,
            "green" => Color.green,
            "yellow" => Color.yellow,
            "black" => Color.black,
            "white" => Color.white,
            _ => Color.gray
        };
    }
}

