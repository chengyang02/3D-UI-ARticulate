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
    public bool highlighted = false; 

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

    void Update()
    {
        if (gameObject.tag == "Untagged") {
            highlighted = false;
        }
        if (highlighted) {
            outlinable.enabled = true;
        } else {
            outlinable.enabled = false;
        }
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
        if (outlinable.enabled) {
            highlighted = true;
            gameObject.tag = "highlighted";
        } else {
            highlighted = false; 
            gameObject.tag = "Untagged";
        }
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
}
