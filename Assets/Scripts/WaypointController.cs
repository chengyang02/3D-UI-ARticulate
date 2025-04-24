using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaypointController : MonoBehaviour
{
    public Camera minimapCamera;
    public RectTransform miniMapTextureRect;
    public RectTransform arrowPrefab;
    public RectTransform indicatorPrefab; 
    private RectTransform arrowInstance;
    private RectTransform indicatorInstance;

    void Start()
    {
        // instantiate a pin on the mini-map when a waypoint is marked 
        indicatorInstance = Instantiate(indicatorPrefab, miniMapTextureRect.parent); 
        indicatorInstance.SetAsLastSibling();
    }

    void Update()
    {
        Vector3 viewportPos = minimapCamera.WorldToViewportPoint(transform.position);

        bool isInside = viewportPos.z > 0 && viewportPos.x > 0 && viewportPos.x < 1 && viewportPos.y > 0 && viewportPos.y < 1;
        indicatorInstance.gameObject.SetActive(!isInside);
        Debug.Log("Isinside: " + isInside);

        if (!isInside)
        {
            // Direction from minimap center to waypoint
            Vector3 dir = transform.position - minimapCamera.transform.position;
            dir.y = 0;
            dir.Normalize();

            // Center of minimap texture in local space
            Vector2 center = miniMapTextureRect.anchoredPosition;
            float radius = Mathf.Min(miniMapTextureRect.sizeDelta.x, miniMapTextureRect.sizeDelta.y) / 2f;

            // pull back slightly from edge
            Vector2 offset = new Vector2(dir.x, dir.z) * (radius - 20f); 
            indicatorInstance.anchoredPosition = center + offset;

            float angle = Mathf.Atan2(dir.x, dir.z) * Mathf.Rad2Deg;
            indicatorInstance.localRotation = Quaternion.Euler(0, 0, -angle);
        }
    }

    void OnDestroy()
    {
        Destroy(indicatorInstance.gameObject);
    }
}
