// Scripts/VoiceActions/VoiceCommandHandler.cs
using System;
using UnityEngine;
using System.Linq;


public class VoiceCommandHandler : MonoBehaviour
{
    public void HandleVoiceCommand(string command)
    {
        Debug.Log($"Received command: {command}");
        string lowerCommand = command.ToLower();

        string[] actions = { "move", "rotate", "scale", "color" };
        string[] colors = { "blue", "red", "green", "yellow", "black", "white" };
        string[] types = { "book", "cube", "chair", "table", "plant" };

        string action = actions.FirstOrDefault(a => lowerCommand.Contains(a));
        string color = colors.FirstOrDefault(c => lowerCommand.Contains(c));
        string type = types.FirstOrDefault(t => lowerCommand.Contains(t));

        if (action == null || color == null || type == null)
        {
            Debug.LogWarning($"Command parsing failed. Action: {action}, Color: {color}, Type: {type}");
            return;
        }

        GameObject[] targets = GameObject.FindGameObjectsWithTag("Interactable");
        foreach (var obj in targets)
        {
            var meta = obj.GetComponent<VoiceControllable>();
            if (meta != null &&
                meta.objectType.ToLower() == type &&
                meta.color.ToLower() == color)
            {
                ApplyAction(obj, action);
            }
        }
    }


    // public void HandleVoiceCommand(string command)
    // {
    //     Debug.Log($"🗣️ Received command: {command}");
    //     string[] words = command.ToLower().Split(' ');

    //     if (words.Length < 3)
    //     {
    //         Debug.LogWarning("❗Command must follow format: <action> <color> <type>");
    //         return;
    //     }

    //     string action = words[0];  // e.g., scale, move, rotate, color
    //     string color = words[1];   // e.g., blue
    //     string type = words[2];    // e.g., book

    //     GameObject[] targets = GameObject.FindGameObjectsWithTag("Interactable");
    //     bool found = false;

    //     foreach (var obj in targets)
    //     {
    //         var meta = obj.GetComponent<VoiceControllable>();
    //         if (meta != null &&
    //             meta.objectType.ToLower().Contains(type) &&
    //             meta.color.ToLower().Contains(color))
    //         {
    //             found = true;
    //             ApplyAction(obj, action);
    //         }
    //     }

    //     if (!found)
    //     {
    //         Debug.LogWarning($"❓No object matched type: {type}, color: {color}");
    //     }
    // }

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
}
