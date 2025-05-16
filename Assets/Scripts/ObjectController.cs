using System.Collections;
using System.Collections.Generic;
using EPOOutline;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class ObjectController : MonoBehaviour
{
    private Outlinable outlinable;
    private XRGrabInteractable xRGrabInteractable;
    private SelectorManager selectorManager;

    // Initialize components in Awake instead of Start
    void Awake()
    {
        InitializeComponents();
    }

    // Start is called before the first frame update
    void Start()
    {
        // Make sure components are initialized
        if (outlinable == null || xRGrabInteractable == null) {
            InitializeComponents();
        }
    }
    
    // Helper method to initialize all required components
    private void InitializeComponents()
    {
        // Check if selector manager exists
        if (SelectorManager.Instance != null)
        {
            selectorManager = SelectorManager.Instance;
        }
        else
        {
            Debug.LogWarning($"SelectorManager.Instance is null! Object {gameObject.name} won't be selectable yet.");
            // Try to find it directly as a fallback
            selectorManager = FindObjectOfType<SelectorManager>();
        }
        
        outlinable = GetComponent<Outlinable>();
        xRGrabInteractable = GetComponent<XRGrabInteractable>();

        if (outlinable == null) {
            outlinable = gameObject.AddComponent<Outlinable>();
            Debug.Log($"Added missing Outlinable component to {gameObject.name}");
        }
        
        if (xRGrabInteractable == null) {
            xRGrabInteractable = gameObject.AddComponent<XRGrabInteractable>();
            Debug.Log($"Added missing XRGrabInteractable component to {gameObject.name}");
        }

        // disable outline at the beginning 
        if (outlinable != null)
            outlinable.enabled = false; 

        // add listener for when object is selected or not 
        if (xRGrabInteractable != null)
        {
            // Check if we already have a listener to avoid adding duplicates
            bool hasListener = false;
            
            // Unity doesn't provide a direct way to check, so we'll use a workaround
            var count = xRGrabInteractable.selectEntered.GetPersistentEventCount();
            for (int i = 0; i < count; i++)
            {
                var target = xRGrabInteractable.selectEntered.GetPersistentTarget(i);
                if (target == this)
                {
                    hasListener = true;
                    break;
                }
            }
            
            if (!hasListener)
            {
                xRGrabInteractable.selectEntered.AddListener(OnSelectEntered);
            }
        }
    }

    void Update()
    {

    }

    void OnSelectEntered(SelectEnterEventArgs args)
    {
        Debug.Log("entered selection");
        
        // Make sure components are initialized
        if (outlinable == null || selectorManager == null)
        {
            InitializeComponents();
            
            // If still null after initialization, log and return to avoid NullReferenceException
            if (outlinable == null || selectorManager == null)
            {
                Debug.LogError($"Missing components on {gameObject.name}: outlinable={outlinable!=null}, selectorManager={selectorManager!=null}");
                return;
            }
        }
        
        ToggleHighlight();
        
        if (outlinable.enabled) {
            // add to selected object list 
            selectorManager.AddToSelection(gameObject); 
        } else {
            // remove from selected object list 
            selectorManager.RemoveFromSelection(gameObject);
        }
    }

    public void ToggleHighlight() {
        // Make sure we have the outlinable component
        if (outlinable == null) {
            outlinable = GetComponent<Outlinable>();
            if (outlinable == null) {
                // If still null, try to add it
                outlinable = gameObject.AddComponent<Outlinable>();
            }
        }
        
        if (outlinable != null) {
            outlinable.enabled = !outlinable.enabled;
        }
    }

    void OnSelectExited(SelectExitEventArgs args)
    {
        if (outlinable != null)
        {
            outlinable.enabled = false;
        }
    }

    void OnDestroy()
    {
        // Clean up event listeners
        if (xRGrabInteractable != null)
            xRGrabInteractable.selectEntered.RemoveListener(OnSelectEntered);
        // xRGrabInteractable.selectExited.RemoveListener(OnSelectExited);
    }
}
