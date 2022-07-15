using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DieObj : MonoBehaviour
{
    [SerializeField] protected Sprite[] sideSprites;

    protected SpriteRenderer spriteRenderer;

    protected int currentSideIndex = -1;

    protected int dieID = -1;
    public int DieID
    {
        get { return dieID; }
    }

    void Awake()
    {
        spriteRenderer = this.gameObject.GetComponent<SpriteRenderer>();
    }

    void Update()
    {
        if (currentSideIndex >= 0)
        {
            spriteRenderer.sprite = sideSprites[currentSideIndex];
        }
        else
        {
            spriteRenderer.sprite = sideSprites[6];
        }
    }

    public void Setup(int id)
    {
        dieID = id;
    }

    public void Roll()
    {
        currentSideIndex = Random.Range(0, 6);
    }

    public int GetCurrentSideNumber()
    {
        return currentSideIndex + 1;
    }
}
