using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class WaypointMarker : MonoBehaviour
{
    public XRRayInteractor rayInteractor; 
    public GameObject waypointPrefab;     
    public string waypointTag = "Waypoint";

    public Camera minimapCamera;
    public RectTransform miniMapTextureRect;
    public RectTransform indicatorUIPrefab;

    
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void MarkWaypoint()
    {
        if (rayInteractor.TryGetCurrent3DRaycastHit(out RaycastHit hit))
        {
            GameObject hitObject = hit.collider.gameObject;

            // If we hit an existing waypoint, destroy it
            if (hitObject.CompareTag(waypointTag))
            {
                Destroy(hitObject);
                Debug.Log("Removed existing waypoint.");
            }
            else
            {
                // Place a new waypoint at the hit point
                Vector3 spawnPosition = hit.point;
                GameObject waypoint = Instantiate(waypointPrefab, spawnPosition, Quaternion.identity);

                // populate its component
                WaypointController waypointController = waypoint.GetComponent<WaypointController>();
                waypointController.minimapCamera = minimapCamera;
                waypointController.miniMapTextureRect = miniMapTextureRect;
                waypointController.indicatorPrefab = indicatorUIPrefab;

                Debug.Log("Placed new waypoint at: " + spawnPosition);
            }
        }
    }
}
