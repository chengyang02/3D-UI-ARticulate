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
    public GameObject parentInteractable; 
    private bool groupUpdateNeeded = false; 


    [Header("Settings")]
    public float offsetFromController = 0.5f;

    private GameObject selectionSphere;
    private HashSet<GameObject> currentTargets = new HashSet<GameObject>();
    public static SelectorManager Instance; 

    void Awake()
    {
        if (Instance == null) {
            Instance = this; 
        } else {
            Destroy(Instance); 
        }
    }

    // void LateUpdate()
    // {
    //     if (!groupUpdateNeeded) return;

    //     groupUpdateNeeded = false;

    //     // var vic = FindObjectOfType<Voice2Action.VoiceIntentController>();
    //     // Debug.Log(vic);
    //     // vic?.RefreshControllers();

    //     if (currentTargets.Count == 0) return;

    //     // Unparent and destroy old tag groups
    //     foreach (Transform child in parentInteractable.transform)
    //     {
    //         foreach (Transform obj in child)
    //         {
    //             obj.SetParent(null, true);
    //         }
    //         Destroy(child.gameObject);
    //     }

    //     Dictionary<string, GameObject> tagGroups = new Dictionary<string, GameObject>();

    //     foreach (GameObject obj in currentTargets)
    //     {
    //         string tag = obj.tag;

    //         if (!tagGroups.ContainsKey(tag))
    //         {
    //             GameObject group = new GameObject(tag);
    //             group.transform.SetParent(parentInteractable.transform);
    //             tagGroups[tag] = group;
    //         }

    //         var interactable = obj.GetComponent<XRGrabInteractable>();
    //         bool wasEnabled = false;

    //         if (interactable != null && interactable.enabled)
    //         {
    //             wasEnabled = true;
    //             interactable.enabled = false; // disable before reparenting
    //         }

    //         obj.transform.SetParent(tagGroups[tag].transform, true);
    //         Debug.Log($"[GroupFix] {obj.name} parented to {tagGroups[tag].name}");

    //         if (wasEnabled)
    //             interactable.enabled = true; // re-enable after parenting
    //     }
    // }


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
        groupUpdateNeeded = true;
    }

    public void RemoveFromSelection(GameObject obj)
    {
        currentTargets.Remove(obj);
        groupUpdateNeeded = true;
    }
}
