using System.Collections.Generic;
using UnityEngine;
using EPOOutline;

public class SelectionState : MonoBehaviour
{
    public static SelectionState Instance;
    public List<GameObject> SelectedObjects = new();

    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(this);
    }

    public void SelectObjects(List<GameObject> objects)
    {
        // ⛔ Clear everything first
        ClearSelection();

        Debug.Log($"[SelectionState] Selecting {objects.Count} object(s).");

        foreach (var obj in objects)
        {
            SelectedObjects.Add(obj);

            var outline = obj.GetComponent<Outlinable>();
            if (outline != null)
            {
                outline.enabled = true;
                Debug.Log($"[SelectionState] Enabled outline on {obj.name}");
            }
        }
    }

    public void ClearSelection()
    {
        Debug.Log("[SelectionState] Clearing selection...");

        // 🧼 Force disable all outlines just in case
        var allObjects = GameObject.FindGameObjectsWithTag("Interactable");
        foreach (var obj in allObjects)
        {
            var outline = obj.GetComponent<Outlinable>();
            if (outline != null && outline.enabled)
            {
                outline.enabled = false;
                Debug.Log($"[SelectionState] Disabled outline on {obj.name}");
            }
        }

        SelectedObjects.Clear();
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
