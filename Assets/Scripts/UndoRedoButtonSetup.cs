using UnityEngine;
using UnityEngine.UI;

// Add this component to the WhisperUI GameObject to connect undo/redo buttons
public class UndoRedoButtonSetup : MonoBehaviour
{
    // Manually assign in inspector if auto-find fails
    public Button undoButton;
    public Button redoButton;
    
    void Awake()
    {
        // Find buttons in children if not assigned
        if (undoButton == null)
        {
            undoButton = transform.Find("Window/UndoButton")?.GetComponent<Button>();
            
            if (undoButton == null)
            {
                Debug.LogError("UndoRedoButtonSetup: Could not find UndoButton. Please assign it manually in the Inspector.");
            }
            else
            {
                Debug.Log("UndoRedoButtonSetup: Found UndoButton automatically");
            }
        }
        
        if (redoButton == null)
        {
            redoButton = transform.Find("Window/RedoButton")?.GetComponent<Button>();
            
            if (redoButton == null)
            {
                Debug.LogError("UndoRedoButtonSetup: Could not find RedoButton. Please assign it manually in the Inspector.");
            }
            else
            {
                Debug.Log("UndoRedoButtonSetup: Found RedoButton automatically");
            }
        }
        
        // Find the UndoRedoManager component
        UndoRedoManager manager = GetComponent<UndoRedoManager>();
        if (manager == null)
        {
            // If no UndoRedoManager exists, add one
            manager = gameObject.AddComponent<UndoRedoManager>();
            Debug.Log("UndoRedoButtonSetup: Added UndoRedoManager component");
        }
        
        // Connect buttons to manager
        if (undoButton != null && manager != null)
        {
            manager.undoButton = undoButton;
            Debug.Log("UndoRedoButtonSetup: Connected UndoButton to manager");
            
            // Make sure the button has a click handler
            undoButton.onClick.RemoveAllListeners();
            undoButton.onClick.AddListener(manager.Undo);
            Debug.Log("UndoRedoButtonSetup: Added Undo click listener");
        }
        
        if (redoButton != null && manager != null)
        {
            manager.redoButton = redoButton;
            Debug.Log("UndoRedoButtonSetup: Connected RedoButton to manager");
            
            // Make sure the button has a click handler
            redoButton.onClick.RemoveAllListeners();
            redoButton.onClick.AddListener(manager.Redo);
            Debug.Log("UndoRedoButtonSetup: Added Redo click listener");
        }
    }
} 