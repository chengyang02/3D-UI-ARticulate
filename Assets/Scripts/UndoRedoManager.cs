using UnityEngine;
using UnityEngine.UI;

public class UndoRedoManager : MonoBehaviour
{
    public Button undoButton;
    public Button redoButton;
    
    void Start()
    {
        Debug.Log("UndoRedoManager: Start called");
        
        // Register button click events
        if (undoButton != null)
        {
            Debug.Log("UndoRedoManager: Undo button found, adding listener");
            undoButton.onClick.RemoveAllListeners(); // Clear any existing listeners
            undoButton.onClick.AddListener(Undo);
        }
        else
        {
            Debug.LogError("UndoRedoManager: Undo button reference is null! Buttons need to be assigned in the Inspector or programmatically.");
        }
        
        if (redoButton != null)
        {
            Debug.Log("UndoRedoManager: Redo button found, adding listener");
            redoButton.onClick.RemoveAllListeners(); // Clear any existing listeners
            redoButton.onClick.AddListener(Redo);
        }
        else
        {
            Debug.LogError("UndoRedoManager: Redo button reference is null! Buttons need to be assigned in the Inspector or programmatically.");
        }
        
        // Update button states initially
        UpdateButtonStates();
    }
    
    void Update()
    {
        // Update button interactability based on ActionExecutioner state
        UpdateButtonStates();
        
        // Keyboard shortcuts for undo/redo
        if (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl))
        {
            if (Input.GetKeyDown(KeyCode.Z))
            {
                Undo();
            }
            else if (Input.GetKeyDown(KeyCode.Y))
            {
                Redo();
            }
        }
    }
    
    public void Undo()
    {
        if (ActionExecutioner.Instance != null && ActionExecutioner.Instance.CanUndo())
        {
            Debug.Log("UndoRedoManager: Executing Undo operation");
            ActionExecutioner.Instance.Undo();
            UpdateButtonStates();
        }
        else
        {
            Debug.LogWarning("UndoRedoManager: Cannot Undo - ActionExecutioner is null or has no actions to undo");
        }
    }
    
    public void Redo()
    {
        if (ActionExecutioner.Instance != null && ActionExecutioner.Instance.CanRedo())
        {
            Debug.Log("UndoRedoManager: Executing Redo operation");
            ActionExecutioner.Instance.Redo();
            UpdateButtonStates();
        }
        else
        {
            Debug.LogWarning("UndoRedoManager: Cannot Redo - ActionExecutioner is null or has no actions to redo");
        }
    }
    
    private void UpdateButtonStates()
    {
        if (ActionExecutioner.Instance != null)
        {
            if (undoButton != null)
            {
                bool canUndo = ActionExecutioner.Instance.CanUndo();
                undoButton.interactable = canUndo;
                if (Debug.isDebugBuild && undoButton.interactable != canUndo)
                {
                    Debug.Log($"Undo button interactable set to: {canUndo}");
                }
            }
            
            if (redoButton != null)
            {
                bool canRedo = ActionExecutioner.Instance.CanRedo();
                redoButton.interactable = canRedo;
                if (Debug.isDebugBuild && redoButton.interactable != canRedo)
                {
                    Debug.Log($"Redo button interactable set to: {canRedo}");
                }
            }
        }
        else
        {
            // If ActionExecutioner is not available, disable both buttons
            if (undoButton != null) undoButton.interactable = false;
            if (redoButton != null) redoButton.interactable = false;
        }
    }
} 