using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using FormatGames.WayPoint;

public class RangeSpawner : MonoBehaviour
{
    public GameObject targetPrefab;
    public GameObject indicatorPrefab;

    public Transform spawnCenter;
    public float spawnDistance = 5f;

    public Image fillKeyUI;

    private bool isHoldingKey;

    public IndicatorReference indicator;

    private void Start()
    {
        indicator = Manager.refs.GetIndicator(55538);
    }
    void Update()
    {
        if (indicator != null && indicator.indicator.isInRange)
        {
            if (Input.GetKeyDown(KeyCode.E))
            {
                fillKeyUI.fillAmount = 0;
                isHoldingKey = true;
            }

            if (Input.GetKeyUp(KeyCode.E))
            {
                isHoldingKey = false;
                fillKeyUI.fillAmount = 1; // Reset UI if the key is released early
            }

            if (isHoldingKey)
            {
                // Increase fill amount over time
                fillKeyUI.fillAmount += Time.deltaTime / 2;

                // When UI reaches 1, spawn object and reset
                if (fillKeyUI.fillAmount >= 1)
                {
                    isHoldingKey = false;
                    SpawnObject();
                    fillKeyUI.fillAmount = 1;
                }
            }
        }  
    }

    void SpawnObject()
    {
        Transform target = Instantiate(targetPrefab, spawnPosition(), Quaternion.identity).transform;
        RectTransform indicator = Instantiate(indicatorPrefab, Manager.refs.mainCanvas.transform).GetComponent<RectTransform>();

        Manager.refs.AddIndicator(14744, indicator, target);
    }

    public Vector3 spawnPosition()
    {
        float angle = Random.Range(0f, 360f);

        return spawnCenter.position + new Vector3(Mathf.Cos(angle * Mathf.Deg2Rad) * spawnDistance, 0f, Mathf.Sin(angle * Mathf.Deg2Rad) * spawnDistance);
    }
}
