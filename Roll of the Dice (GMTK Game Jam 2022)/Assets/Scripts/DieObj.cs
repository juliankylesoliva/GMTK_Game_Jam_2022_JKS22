using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DieObj : MonoBehaviour
{
    [SerializeField] protected Sprite[] sideSprites;

    protected SpriteRenderer spriteRenderer;

    protected int currentSideIndex = -1;
    protected bool rollThisFrame = true;

    protected bool isRolling = false;
    public bool IsRolling
    {
        get { return isRolling; }
    }

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
        if (currentSideIndex >= 0 && !isRolling)
        {
            spriteRenderer.sprite = sideSprites[currentSideIndex];
        }
        else
        {
            if (isRolling)
            {
                if (rollThisFrame)
                {
                    spriteRenderer.sprite = sideSprites[Random.Range(0, 6)];
                }
                rollThisFrame = !rollThisFrame;
            }
            else
            {
                spriteRenderer.sprite = sideSprites[6];
            }
        }
    }

    public void Setup(int id)
    {
        dieID = id;
    }

    public void Roll()
    {
        if (!isRolling)
        {
            StartCoroutine(RollCoroutine());
        }
    }

    protected IEnumerator RollCoroutine()
    {
        isRolling = true;
        yield return new WaitForSeconds(0.75f);
        currentSideIndex = Random.Range(0, 6);
        isRolling = false;
    }

    public int GetCurrentSideNumber()
    {
        return currentSideIndex + 1;
    }
}
