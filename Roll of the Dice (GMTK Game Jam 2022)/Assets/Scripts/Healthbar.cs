using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Healthbar : MonoBehaviour
{
    private enum ShipAnimationState { IDLE, DAMAGE, SINKING }

    [SerializeField] float idleAmplitude = 1f;
    [SerializeField] float idleRadiansPerSecond = 3.141592f;
    [SerializeField, Range(0f, 0.5f)] float radsPerSecDeviation = 0.25f;

    [SerializeField] float damageShakeAmplitude = 1f;
    [SerializeField] int shakeAfterEveryXFrames = 2;

    [SerializeField] float sinkingSpeed = 0.5f;
    [SerializeField] float sinkingYLimit = -50f;

    [SerializeField] Transform meterFill;

    private Vector3 startingPosition;
    private ShipAnimationState currentState = ShipAnimationState.IDLE;
    private float animTimer = 0f;
    private float currentRadians = 0f;
    private bool shakeRight = false;
    private int shakeTimer = 0;

    void Start()
    {
        startingPosition = this.transform.position;
        FloatShip();
    }

    void Update()
    {
        switch (currentState)
        {
            case ShipAnimationState.IDLE:
                currentRadians += (idleRadiansPerSecond * Time.deltaTime * Random.Range(1f - radsPerSecDeviation, 1f + radsPerSecDeviation));
                this.transform.position = (startingPosition + (Vector3.up * ((idleAmplitude * Mathf.Sin(currentRadians)) - idleAmplitude)));
                break;
            case ShipAnimationState.DAMAGE:
                this.transform.position = (new Vector3(startingPosition.x, this.transform.position.y, startingPosition.z) + ((shakeRight ? Vector3.right : -Vector3.right) * damageShakeAmplitude * (animTimer / 1f)));
                if (shakeTimer == 0)
                {
                    shakeRight = !shakeRight;
                    shakeTimer = shakeAfterEveryXFrames;
                }
                else
                {
                    shakeTimer--;
                }
                break;
            case ShipAnimationState.SINKING:
                this.transform.position = (new Vector3(startingPosition.x, this.transform.position.y, startingPosition.z)  + ((shakeRight ? Vector3.right : -Vector3.right) * damageShakeAmplitude));
                if (this.transform.position.y > sinkingYLimit)
                {
                    this.transform.position -= (Vector3.up * sinkingSpeed * Time.deltaTime);
                }

                if (shakeTimer == 0)
                {
                    shakeRight = !shakeRight;
                    shakeTimer = shakeAfterEveryXFrames;
                }
                else
                {
                    shakeTimer--;
                }
                break;
        }

        animTimer -= Time.deltaTime;

        if (animTimer <= 0)
        {
            switch (currentState)
            {
                case ShipAnimationState.IDLE:
                    break;
                case ShipAnimationState.DAMAGE:
                    FloatShip();
                    break;
                case ShipAnimationState.SINKING:
                    break;
            }
        }
    }

    public void SetFillScale(float ratio)
    {
        meterFill.localScale = new Vector3(1f, ratio, 1f);
    }

    private void FloatShip()
    {
        currentState = ShipAnimationState.IDLE;
        animTimer = 0;
    }

    public void DamageShip()
    {
        currentState = ShipAnimationState.DAMAGE;
        shakeRight = true;
        shakeTimer = shakeAfterEveryXFrames;
        animTimer = 1f;
    }

    public void SinkShip()
    {
        currentState = ShipAnimationState.SINKING;
        shakeRight = true;
        shakeTimer = shakeAfterEveryXFrames;
        animTimer = 0;
    }
}
