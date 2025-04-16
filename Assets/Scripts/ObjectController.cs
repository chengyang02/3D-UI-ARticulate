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


    // Start is called before the first frame update
    void Start()
    {
        selectorManager = SelectorManager.Instance;
        outlinable = GetComponent<Outlinable>();
        xRGrabInteractable = GetComponent<XRGrabInteractable>();

        // disable outline at the beginning 
        outlinable.enabled = false; 

        // add listener for when object is selected or not 
        xRGrabInteractable.selectEntered.AddListener(OnSelectEntered);
        // xRGrabInteractable.selectExited.AddListener(OnSelectExited);
    }

    void OnSelectEntered(SelectEnterEventArgs args)
    {
        Debug.Log("entered selection");
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
        outlinable.enabled = !outlinable.enabled; 
    }

    void OnSelectExited(SelectExitEventArgs args)
    {
        outlinable.enabled = false;
    }

    void OnDestroy()
    {
        // Clean up event listeners
        xRGrabInteractable.selectEntered.RemoveListener(OnSelectEntered);
        // xRGrabInteractable.selectExited.RemoveListener(OnSelectExited);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
