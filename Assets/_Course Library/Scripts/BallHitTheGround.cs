using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BallHitTheGround : MonoBehaviour
{
    public AudioSource audioSource;
    public AudioClip clip;
    private Rigidbody rb; // Reference to the Rigidbody component

    void Start()
    {
        rb = GetComponent<Rigidbody>(); // Get the Rigidbody component
    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.tag == "Ground")
        {
            float speed = rb.velocity.magnitude; // Get the speed of the ball
            float volume = Mathf.Clamp(speed / 10, 0.1f, 1f); // Calculate volume based on speed, adjust values as needed
            audioSource.PlayOneShot(clip, volume);
        }
    }
}

