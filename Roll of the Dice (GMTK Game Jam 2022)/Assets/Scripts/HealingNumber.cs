using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class HealingNumber : MonoBehaviour
{
    [SerializeField] float initialVerticalVelocity = 4f;
    [SerializeField] float horizontalMotionMagnitude = 1.5f;
    [SerializeField] float angularVelocity = 3.14f;

    new private Rigidbody2D rigidbody2D;
    private TextMeshPro healingText;

    private Vector3 initialPosition;
    private float currentRadians = 0f;

    void Awake()
    {
        rigidbody2D = this.gameObject.GetComponent<Rigidbody2D>();
        healingText = this.gameObject.GetComponent<TextMeshPro>();
        initialPosition = this.transform.position;
    }

    void Start()
    {
        rigidbody2D.velocity = (Vector2.up * initialVerticalVelocity);
    }

    void Update()
    {
        this.transform.position = new Vector3(initialPosition.x + (horizontalMotionMagnitude * Mathf.Sin(currentRadians)), this.transform.position.y, 0f);
        if (this.transform.position.y > 20f)
        {
            GameObject.Destroy(this.gameObject);
        }
        currentRadians += (angularVelocity * Time.deltaTime);
    }

    public void Setup(int healing, Color color)
    {
        healingText.text = $"{healing}";
        healingText.color = color;
    }
}
