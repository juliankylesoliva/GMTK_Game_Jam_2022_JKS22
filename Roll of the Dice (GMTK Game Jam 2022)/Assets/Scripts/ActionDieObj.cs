using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActionDieObj : DieObj
{
    [SerializeField] DiePreview dieSidesPreview;

    private SideType[] dieSides;

    private int strength = 0;
    public int Strength
    {
        get { return strength; }
        set
        {
            if (value >= 1 && value <= 6)
            {
                strength = value;
            }
        }
    }

    private int bonus = 0;
    public int Bonus
    {
        get { return bonus; }
        set
        {
            if (value > 0)
            {
                bonus = value;
            }
        }
    }

    void Update()
    {
        if (currentSideIndex >= 0 && !isRolling)
        {
            SideType currentSideType = GetCurrentSideType();
            switch (currentSideType)
            {
                case SideType.STRIKE:
                    spriteRenderer.sprite = sideSprites[0];
                    break;
                case SideType.GUARD:
                    spriteRenderer.sprite = sideSprites[1];
                    break;
                case SideType.SUPPORT:
                    spriteRenderer.sprite = sideSprites[2];
                    break;
            }
        }
        else
        {
            if (isRolling)
            {
                if (rollThisFrame)
                {
                    spriteRenderer.sprite = sideSprites[Random.Range(0, 3)];
                }
                rollThisFrame = !rollThisFrame;
            }
            else
            {
                spriteRenderer.sprite = sideSprites[3];
            }
        }

        CheckIfMouseover();
    }

    private void CheckIfMouseover()
    {
        RaycastHit2D hit = Physics2D.Raycast(Camera.main.ScreenToWorldPoint(Input.mousePosition), Vector2.zero);
        if (hit.collider != null)
        {
            GameObject tempDie = hit.transform.gameObject;
            if (tempDie != null)
            {
                dieSidesPreview.gameObject.SetActive(GameObject.ReferenceEquals(this.gameObject, tempDie));
            }
        }
        else
        {
            dieSidesPreview.gameObject.SetActive(false);
        }
    }

    public void Setup(ActDie_SO adso, int id)
    {
        dieSides = adso.SideList;
        dieID = id;
        dieSidesPreview.SetSides(dieSides);
    }

    public SideType GetCurrentSideType()
    {
        return dieSides[currentSideIndex];
    }

    public float GetProbabiltyOfSide(SideType side)
    {
        int sum = 0;
        foreach (SideType s in dieSides)
        {
            if (s == side) { sum++; }
        }
        return ((float)sum / (float)dieSides.Length);
    }

}
