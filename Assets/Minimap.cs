using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Minimap : MonoBehaviour
{
    [SerializeField] private Transform player;

    // Update is called once per frame
    void Update()
    {
        Vector3 newPosition = player.position;
        newPosition.y = transform.position.y;
        transform.position = newPosition;

        // Rotate the minimap camera to match the player's Y rotation
        // Quaternion newRotation = Quaternion.Euler(90f, player.eulerAngles.y, 0f); // Top-down view
        // transform.rotation = newRotation;
    }
}
