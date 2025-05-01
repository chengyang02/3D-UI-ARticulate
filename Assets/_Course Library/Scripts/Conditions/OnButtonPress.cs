using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

/// <summary>
/// Checks for button input on an input action
/// </summary>
public class OnButtonPress : MonoBehaviour
{
    [Tooltip("Actions to check")]
    public InputAction action = null;

    // When the button is pressed
    public UnityEvent OnPress = new UnityEvent();

    // When the button is released
    public UnityEvent OnRelease = new UnityEvent();

    // While the button is held
    public UnityEvent OnHold = new UnityEvent();
    private bool isHeld = false; 

    private void Awake()
    {
        action.started += Pressed;
        action.canceled += Released;
    }

    void Update() {
        if (isHeld) {
            OnHold.Invoke(); 
        }
    }

    private void OnDestroy()
    {
        action.started -= Pressed;
        action.canceled -= Released;
    }

    private void OnEnable()
    {
        action.Enable();
    }

    private void OnDisable()
    {
        action.Disable();
    }

    private void Pressed(InputAction.CallbackContext context)
    {
        OnPress.Invoke();
        isHeld = true; 
    }

    private void Released(InputAction.CallbackContext context)
    {
        OnRelease.Invoke();
        isHeld = false;
    }
}
