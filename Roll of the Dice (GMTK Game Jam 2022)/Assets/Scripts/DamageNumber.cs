using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class DamageNumber : MonoBehaviour
{
    [SerializeField] float initialVerticalVelocity = 4f;
    [SerializeField] float initialHorizontalVelocityMagnitude = 3f;

    new private Rigidbody2D rigidbody2D;
    private TextMeshPro damageText;

    void Awake()
    {
        rigidbody2D = this.gameObject.GetComponent<Rigidbody2D>();
        damageText = this.gameObject.GetComponent<TextMeshPro>();
    }

    void Start()
    {
        rigidbody2D.velocity = (Vector2.up * initialVerticalVelocity) + ((Random.Range(0, 2) == 0 ? -1f : 1f) * initialHorizontalVelocityMagnitude * Vector2.right);
    }

    void Update()
    {
        rigidbody2D.gravityScale += 0.125f;
        if (this.transform.position.y < -50f)
        {
            GameObject.Destroy(this.gameObject);
        }
    }

    public void Setup(int damage, Color color)
    {
        damageText.text = $"{damage}";
        damageText.color = color;
    }
}
