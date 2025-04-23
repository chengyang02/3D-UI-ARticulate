using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class SelectorManager : MonoBehaviour
{
    [Header("References")]
    public Transform controllerTransform; 
    public GameObject selectionSpherePrefab;
    public XRRayInteractor rayInteractor;

    [Header("Settings")]
    public float offsetFromController = 0.5f;

    private GameObject selectionSphere;
    private HashSet<GameObject> currentTargets = new HashSet<GameObject>();
    public static SelectorManager Instance; 

    void Start()
    {
        if (Instance == null) {
            Instance = this; 
        } else {
            Destroy(Instance); 
        }
    }

    void Update()
    {
        // Debug.Log("current selected number: " + currentTargets.Count);
    }

    public void ToggleSelector()
    {
        if (selectionSphere == null)
        {
            selectionSphere = Instantiate(selectionSpherePrefab);

            // Set the object as child of the controller
            selectionSphere.transform.SetParent(controllerTransform, true); 
            Vector3 offset = new Vector3(0, 0, offsetFromController); 
            selectionSphere.transform.localPosition = offset;

            // disable ray
            rayInteractor.enabled = false; 
        } else {
            Destroy(selectionSphere); 
            selectionSphere = null; 

            // renable ray
            rayInteractor.enabled = true; 
        }
    }

    public void ConfirmGroupSelection()
    {
        if (selectionSphere == null)
            return;

        float radius = selectionSphere.transform.localScale.x / 2f;
        Vector3 center = selectionSphere.transform.position;

        Collider[] hits = Physics.OverlapSphere(center, radius);

        foreach (Collider hit in hits)
        {
            GameObject hitObject = hit.gameObject;
            ObjectController objectController = hitObject.GetComponent<ObjectController>();
            if (objectController != null) 
            {
                objectController.ToggleHighlight(); 
                if (currentTargets.Contains(hitObject)) {
                    RemoveFromSelection(hitObject);
                } else {
                    AddToSelection(hitObject); 
                }
            }
        }
    }

    public void AddToSelection(GameObject obj)
    {
        currentTargets.Add(obj);
    }

    public void RemoveFromSelection(GameObject obj)
    {
        currentTargets.Remove(obj);
    }
}
