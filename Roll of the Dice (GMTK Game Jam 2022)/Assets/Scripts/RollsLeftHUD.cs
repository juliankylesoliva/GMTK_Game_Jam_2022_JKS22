using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RollsLeftHUD : MonoBehaviour
{
    [SerializeField] Sprite[] rerollAmountSprites;

    private SpriteRenderer spriteRenderer;

    void Awake()
    {
        spriteRenderer = this.gameObject.GetComponent<SpriteRenderer>();
    }

    public void SetRerollAmount(int amount)
    {
        spriteRenderer.sprite = rerollAmountSprites[amount];
    }
}
