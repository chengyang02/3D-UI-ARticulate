using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using Whisper.Samples;

public class VoiceCommandManager : MonoBehaviour
{
    public static VoiceCommandManager Instance { get; private set; }
    public Text transcript;
    private string lastTranscript;
    private int commandVersion = 0; // 每次新指令+1

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

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        lastTranscript = transcript.text;
    }

    void Update()
    {
        if (transcript.text != lastTranscript && !StreamingSampleMic.Instance.isRecording)
        {
            ProcessVoiceCommand();
            lastTranscript = transcript.text;
        }
    }

    public async void ProcessVoiceCommand()
    {
        commandVersion++;
        int thisCommand = commandVersion;
        string currentTranscript = transcript.text;

        Debug.Log($"🗣️ Processing command: {currentTranscript} (version {thisCommand})");
        
        // First try to parse as JSON if it's from LLM
        if (currentTranscript.Trim().StartsWith("{"))
        {
            try
            {
                VoiceAction action = JsonUtility.FromJson<VoiceAction>(currentTranscript);
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

        // If not JSON, try to classify with LLM
        string response = await ActionClassifier.Instance.ClassifyText(currentTranscript);
        Debug.Log($"🤖 LLM response: {response}");

        // 检查是否是最新指令
        if (thisCommand != commandVersion)
        {
            Debug.Log("⚠️ This command is outdated, skipping execution.");
            return;
        }

        // Parse and execute the classified response
        ExecuteStructuredAction(JsonUtility.FromJson<VoiceAction>(response));
    }

    private void ExecuteStructuredAction(VoiceAction action)
    {
        string type = action.target.object_type.ToLower();
        string color = action.target.color?.ToLower();

        switch (action.action_type.ToLower())
        {
            case "selection":
                HandleSelection(type, color);
                break;
            case "rotation":
                HandleRotation(type, color, action.axis, action.angle);
                break;
            case "translation":
            case "move":
                HandleTranslation(type, color, action.direction, action.distance);
                break;
            case "scale":
                HandleScale(type, color, action.scale_factor);
                break;
            case "color":
                HandleColorChange(type, color, action.color);
                break;
            default:
                Debug.LogWarning($"🚫 Unknown action type: {action.action_type}");
                break;
        }
    }

    private void HandleSelection(string type, string color)
    {
        List<GameObject> selected = new();
        GameObject[] all = GameObject.FindGameObjectsWithTag("Interactable");

        foreach (var obj in all)
        {
            var meta = obj.GetComponent<VoiceControllable>();
            if (meta != null && 
                meta.objectType.ToLower() == type &&
                (string.IsNullOrEmpty(color) || meta.color.ToLower() == color))
            {
                selected.Add(obj);
            }
        }

        Debug.Log($"✅ Selecting {selected.Count} objects: " + string.Join(", ", selected.Select(o => o.name)));
        SelectionState.Instance.SelectObjects(selected);
    }

    private void HandleRotation(string type, string color, string axis, string angle)
    {
        float angleValue = ParseFloat(angle, 45f);
        Vector3 rotationAxis = ParseAxis(axis);

        GameObject[] targets = GameObject.FindGameObjectsWithTag("Interactable");
        foreach (var obj in targets)
        {
            var meta = obj.GetComponent<VoiceControllable>();
            if (meta != null &&
                meta.objectType.ToLower() == type &&
                (string.IsNullOrEmpty(color) || meta.color.ToLower() == color))
            {
                obj.transform.Rotate(rotationAxis, angleValue);
                Debug.Log($"✅ Rotated {obj.name} {angleValue}° around {rotationAxis}");
            }
        }
    }

    private void HandleTranslation(string type, string color, string direction, string distance)
    {
        float dist = ParseFloat(distance, 0.5f);
        Vector3 dir = ParseDirection(direction);

        GameObject[] targets = GameObject.FindGameObjectsWithTag("Interactable");
        foreach (var obj in targets)
        {
            var meta = obj.GetComponent<VoiceControllable>();
            if (meta != null &&
                meta.objectType.ToLower() == type &&
                (string.IsNullOrEmpty(color) || meta.color.ToLower() == color))
            {
                Rigidbody rb = obj.GetComponent<Rigidbody>();
                if (rb != null)
                {
                    Vector3 move = dir * dist;
                    move.y = 0f; // Don't affect gravity
                    rb.MovePosition(rb.position + move);
                }
                else
                {
                    obj.transform.position += dir * dist;
                }
                Debug.Log($"✅ Moved {obj.name} {dist}m toward {dir}");
            }
        }
    }

    private void HandleScale(string type, string color, string scaleFactor)
    {
        float factor = ParseFloat(scaleFactor, 1.2f);

        GameObject[] targets = GameObject.FindGameObjectsWithTag("Interactable");
        foreach (var obj in targets)
        {
            var meta = obj.GetComponent<VoiceControllable>();
            if (meta != null &&
                meta.objectType.ToLower() == type &&
                (string.IsNullOrEmpty(color) || meta.color.ToLower() == color))
            {
                obj.transform.localScale *= factor;
                Debug.Log($"✅ Scaled {obj.name} by {factor}");
            }
        }
    }

    private void HandleColorChange(string type, string color, string newColor)
    {
        GameObject[] targets = GameObject.FindGameObjectsWithTag("Interactable");
        foreach (var obj in targets)
        {
            var meta = obj.GetComponent<VoiceControllable>();
            if (meta != null &&
                meta.objectType.ToLower() == type &&
                (string.IsNullOrEmpty(color) || meta.color.ToLower() == color))
            {
                var renderer = obj.GetComponent<Renderer>();
                if (renderer != null)
                {
                    renderer.material.color = ColorFromString(newColor);
                    Debug.Log($"✅ Changed color of {obj.name} to {newColor}");
                }
            }
        }
    }

    private float ParseFloat(string input, float fallback)
    {
        if (string.IsNullOrWhiteSpace(input)) return fallback;
        input = input.ToLower().Replace("meters", "").Replace("degrees", "").Trim();
        return float.TryParse(input, out float value) ? value : fallback;
    }

    private Vector3 ParseAxis(string axis)
    {
        if (string.IsNullOrWhiteSpace(axis)) return Vector3.up;

        axis = axis.ToLower();
        Vector3 result = Vector3.zero;

        if (axis.Contains("x")) result += Vector3.right;
        if (axis.Contains("y")) result += Vector3.up;
        if (axis.Contains("z")) result += Vector3.forward;

        return result == Vector3.zero ? Vector3.up : result.normalized;
    }

    private Vector3 ParseDirection(string dir)
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

    private Color ColorFromString(string color)
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