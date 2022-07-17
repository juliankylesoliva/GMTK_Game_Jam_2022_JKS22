using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaterMovement : MonoBehaviour
{
    [SerializeField] float idleAmplitude = 1f;
    [SerializeField] float idleRadiansPerSecond = 3.141592f;
    [SerializeField, Range(0f, 0.5f)] float radsPerSecDeviation = 0.25f;
    [SerializeField] int changeSpriteAfterEveryXFrames = 2;
    [SerializeField] Sprite[] spriteList;

    SpriteRenderer spriteRenderer;

    private Vector3 startingPosition;
    private float currentRadians = 0f;
    private int changeTimer = 0;
    private bool isAlternateSprite = false;

    void Awake()
    {
        spriteRenderer = this.gameObject.GetComponent<SpriteRenderer>();
    }

    void Start()
    {
        changeTimer = changeSpriteAfterEveryXFrames;
        startingPosition = this.transform.position;
    }

    void Update()
    {
        currentRadians += (idleRadiansPerSecond * Time.deltaTime * Random.Range(1f - radsPerSecDeviation, 1f + radsPerSecDeviation));
        this.transform.position = (startingPosition + (Vector3.up * ((idleAmplitude * Mathf.Sin(currentRadians)) + idleAmplitude)));
        if (changeTimer == 0)
        {
            isAlternateSprite = !isAlternateSprite;
            changeTimer = changeSpriteAfterEveryXFrames;
        }
        else
        {
            changeTimer--;
        }

        spriteRenderer.sprite = (isAlternateSprite ? spriteList[0] : spriteList[1]);
    }
}
