using UnityEngine;
using UnityEngine.UI;

// This component automatically connects the UndoButton and RedoButton to UndoRedoManager at runtime
public class UndoRedoButtonConnector : MonoBehaviour
{
    void Start()
    {
        Debug.Log("UndoRedoButtonConnector: Attempting to connect buttons...");
        
        // Find the UndoRedoManager
        UndoRedoManager manager = FindObjectOfType<UndoRedoManager>();
        if (manager == null)
        {
            Debug.LogError("UndoRedoButtonConnector: No UndoRedoManager found in the scene!");
            return;
        }
        
        // Find buttons by name
        Button undoButton = GameObject.Find("UndoButton")?.GetComponent<Button>();
        Button redoButton = GameObject.Find("RedoButton")?.GetComponent<Button>();
        
        // If not found by GameObject.Find, try finding child buttons in the hierarchy
        if (undoButton == null || redoButton == null)
        {
            Debug.Log("UndoRedoButtonConnector: Buttons not found by name, searching in hierarchy...");
            Transform parentTransform = transform;
            
            // Try to find the window parent
            Transform windowTransform = transform.Find("Window");
            if (windowTransform != null)
            {
                parentTransform = windowTransform;
            }
            
            // Search in the hierarchy
            undoButton = undoButton ?? parentTransform.Find("UndoButton")?.GetComponent<Button>();
            redoButton = redoButton ?? parentTransform.Find("RedoButton")?.GetComponent<Button>();
        }
        
        // Connect buttons to manager
        if (undoButton != null)
        {
            manager.undoButton = undoButton;
            Debug.Log("UndoRedoButtonConnector: Undo button connected to manager");
        }
        else
        {
            Debug.LogError("UndoRedoButtonConnector: UndoButton not found!");
        }
        
        if (redoButton != null)
        {
            manager.redoButton = redoButton;
            Debug.Log("UndoRedoButtonConnector: Redo button connected to manager");
        }
        else
        {
            Debug.LogError("UndoRedoButtonConnector: RedoButton not found!");
        }
        
        // Add UndoRedoVerifier for diagnostics
        if (!GetComponent<UndoRedoVerifier>())
        {
            UndoRedoVerifier verifier = gameObject.AddComponent<UndoRedoVerifier>();
            Debug.Log("UndoRedoButtonConnector: Added UndoRedoVerifier for diagnostics");
        }
    }
} 