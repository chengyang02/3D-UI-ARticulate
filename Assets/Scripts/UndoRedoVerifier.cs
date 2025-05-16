using UnityEngine;
using UnityEngine.UI;

// This utility script helps verify that the UndoRedoManager is properly set up
public class UndoRedoVerifier : MonoBehaviour
{
    void Start()
    {
        Debug.Log("UndoRedoVerifier: Starting verification of UI setup");
        VerifySetup();
    }

    public void VerifySetup()
    {
        // Check ActionExecutioner
        if (ActionExecutioner.Instance == null)
        {
            Debug.LogError("UndoRedoVerifier: ActionExecutioner.Instance is null! Make sure it exists in the scene.");
            return;
        }

        // Find UndoRedoManager
        UndoRedoManager manager = FindObjectOfType<UndoRedoManager>();
        if (manager == null)
        {
            Debug.LogError("UndoRedoVerifier: UndoRedoManager not found in scene!");
            return;
        }

        // Verify buttons
        if (manager.undoButton == null)
        {
            Debug.LogError("UndoRedoVerifier: UndoButton reference is missing in UndoRedoManager!");
        }
        else 
        {
            Debug.Log("UndoRedoVerifier: UndoButton reference is valid");
        }

        if (manager.redoButton == null)
        {
            Debug.LogError("UndoRedoVerifier: RedoButton reference is missing in UndoRedoManager!");
        }
        else
        {
            Debug.Log("UndoRedoVerifier: RedoButton reference is valid");
        }

        // Test button interactability
        if (manager.undoButton != null)
        {
            Debug.Log($"UndoRedoVerifier: UndoButton interactable = {manager.undoButton.interactable}, CanUndo = {ActionExecutioner.Instance.CanUndo()}");
        }

        if (manager.redoButton != null)
        {
            Debug.Log($"UndoRedoVerifier: RedoButton interactable = {manager.redoButton.interactable}, CanRedo = {ActionExecutioner.Instance.CanRedo()}");
        }

        // Check button click listeners
        if (manager.undoButton != null)
        {
            int listenerCount = manager.undoButton.onClick.GetPersistentEventCount();
            Debug.Log($"UndoRedoVerifier: UndoButton has {listenerCount} persistent listeners");

            // Test action execution 
            ButtonClickSimulator.SimulateClick(manager.undoButton);
        }

        if (manager.redoButton != null)
        {
            int listenerCount = manager.redoButton.onClick.GetPersistentEventCount();
            Debug.Log($"UndoRedoVerifier: RedoButton has {listenerCount} persistent listeners");

            // Test action execution
            ButtonClickSimulator.SimulateClick(manager.redoButton);
        }

        Debug.Log("UndoRedoVerifier: Setup verification complete");
    }

    // Helper class to simulate button clicks
    public static class ButtonClickSimulator
    {
        public static void SimulateClick(Button button)
        {
            if (button != null && button.interactable)
            {
                button.onClick.Invoke();
                Debug.Log($"UndoRedoVerifier: Simulated click on {button.name}");
            }
            else if (button != null)
            {
                Debug.Log($"UndoRedoVerifier: Cannot simulate click on {button.name} - button is not interactable");
            }
        }
    }
} 