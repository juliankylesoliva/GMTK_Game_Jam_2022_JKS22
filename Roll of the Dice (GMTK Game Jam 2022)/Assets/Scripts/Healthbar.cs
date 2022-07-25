using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Healthbar : MonoBehaviour
{
    private enum AnimationState { IDLE, DAMAGE }

    [SerializeField] float damageShakeAmplitude = 1f;
    [SerializeField] int shakeAfterEveryXFrames = 2;
    [SerializeField] float pixelsPerUnit = 32f;
    [SerializeField, Range(-1, 1)] int drainDirection = -1;

    [SerializeField] Transform meterFill;

    private Vector3 startingPosition;
    private Vector3 startingFillPosition;
    private Vector3 endingFillPosition;
    private AnimationState currentState = AnimationState.IDLE;
    private float animTimer = 0f;
    private bool shakeRight = false;
    private int shakeTimer = 0;

    void Start()
    {
        startingPosition = this.transform.position;
        startingFillPosition = meterFill.localPosition;
        endingFillPosition = (startingFillPosition + (Vector3.right * (float)drainDirection * (1f/pixelsPerUnit) * 100f));
        Idle();
    }

    void Update()
    {
        switch (currentState)
        {
            case AnimationState.IDLE:
                break;
            case AnimationState.DAMAGE:
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
        }

        animTimer -= Time.deltaTime;

        if (animTimer <= 0)
        {
            switch (currentState)
            {
                case AnimationState.IDLE:
                    break;
                case AnimationState.DAMAGE:
                    Idle();
                    break;
            }
        }
    }

    public void SetFillPosition(float ratio)
    {
        meterFill.localPosition = Vector3.Lerp(startingFillPosition, endingFillPosition, 1f - ratio);
    }

    private void Idle()
    {
        currentState = AnimationState.IDLE;
    }

    public void Damage()
    {
        currentState = AnimationState.DAMAGE;
        shakeRight = true;
        shakeTimer = shakeAfterEveryXFrames;
        animTimer = 1f;
    }
}
