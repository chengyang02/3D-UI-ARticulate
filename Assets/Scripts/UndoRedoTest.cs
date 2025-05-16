using UnityEngine;
using UnityEngine.UI;

// Add this component to a GameObject to test undo/redo functionality
public class UndoRedoTest : MonoBehaviour
{
    [SerializeField] private Button createButton;
    [SerializeField] private Button colorButton;
    [SerializeField] private Button moveButton;
    [SerializeField] private Button undoButton;
    [SerializeField] private Button redoButton;
    
    void Start()
    {
        if (createButton) createButton.onClick.AddListener(SimulateCreateObject);
        if (colorButton) colorButton.onClick.AddListener(SimulateColorChange);
        if (moveButton) moveButton.onClick.AddListener(SimulateMoveObject);
        if (undoButton) undoButton.onClick.AddListener(SimulateUndo);
        if (redoButton) redoButton.onClick.AddListener(SimulateRedo);
        
        Debug.Log("UndoRedoTest: Ready for testing. Click buttons to simulate actions.");
    }
    
    // Simulates creating a cube
    public void SimulateCreateObject()
    {
        if (ActionExecutioner.Instance != null)
        {
            Debug.Log("UndoRedoTest: Simulating object creation");
            ActionExecutioner.Instance.Execute("creation\nobject_type: cube\ncolor: red");
        }
        else
        {
            Debug.LogError("UndoRedoTest: ActionExecutioner.Instance is null!");
        }
    }
    
    // Simulates changing the color of an object
    public void SimulateColorChange()
    {
        if (ActionExecutioner.Instance != null)
        {
            Debug.Log("UndoRedoTest: Simulating color change");
            ActionExecutioner.Instance.Execute("color\nobject_type: cube\ncolor: blue");
        }
        else
        {
            Debug.LogError("UndoRedoTest: ActionExecutioner.Instance is null!");
        }
    }
    
    // Simulates moving an object
    public void SimulateMoveObject()
    {
        if (ActionExecutioner.Instance != null)
        {
            Debug.Log("UndoRedoTest: Simulating object movement");
            ActionExecutioner.Instance.Execute("translation\nobject_type: cube\ndirection: up\ndistance: 1");
        }
        else
        {
            Debug.LogError("UndoRedoTest: ActionExecutioner.Instance is null!");
        }
    }
    
    // Simulates undo
    public void SimulateUndo()
    {
        if (ActionExecutioner.Instance != null)
        {
            Debug.Log("UndoRedoTest: Simulating undo");
            ActionExecutioner.Instance.Undo();
        }
        else
        {
            Debug.LogError("UndoRedoTest: ActionExecutioner.Instance is null!");
        }
    }
    
    // Simulates redo
    public void SimulateRedo()
    {
        if (ActionExecutioner.Instance != null)
        {
            Debug.Log("UndoRedoTest: Simulating redo");
            ActionExecutioner.Instance.Redo();
        }
        else
        {
            Debug.LogError("UndoRedoTest: ActionExecutioner.Instance is null!");
        }
    }
} 