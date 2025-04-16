using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Clock : MonoBehaviour
{
    public Transform hourHandTransform;
    public Transform minuteHandTransform;
    public Transform secHandTransform;
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        System.DateTime now = System.DateTime.Now;

        float hour = now.Hour % 12 + now.Minute / 60f;
        float minute = now.Minute + now.Second / 60f;
        float second = now.Second;
        float previousSecond = -1;

        hourHandTransform.localRotation = Quaternion.Euler(hour * 30f, 0, 0); 
        minuteHandTransform.localRotation = Quaternion.Euler(minute * 6f,0, 0); 
        secHandTransform.localRotation = Quaternion.Euler(second * 6f,0, 0);

        if (second != previousSecond)
        {
            // Debug.Log($"Current Time: {now.Hour:00}:{now.Minute:00}:{now.Second:00}");
            previousSecond = second;
        }
    }
}


