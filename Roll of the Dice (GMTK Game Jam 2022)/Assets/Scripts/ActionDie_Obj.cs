using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActionDie_Obj : MonoBehaviour
{
    [SerializeField] Sprite[] actionSideSprites;

    private SpriteRenderer spriteRenderer;

    private SideType[] dieSides;

    private int currentSideIndex = -1;

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

    private int multiplier = 1;
    public int Multiplier
    {
        get { return multiplier; }
        set
        {
            if (value > 0)
            {
                multiplier = value;
            }
        }
    }

    private int dieID = -1;
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
            SideType currentSideType = GetCurrentSideType();
            switch (currentSideType)
            {
                case SideType.STRIKE:
                    spriteRenderer.sprite = actionSideSprites[0];
                    break;
                case SideType.GUARD:
                    spriteRenderer.sprite = actionSideSprites[1];
                    break;
                case SideType.SUPPORT:
                    spriteRenderer.sprite = actionSideSprites[2];
                    break;
            }
        }
        else
        {
            spriteRenderer.sprite = actionSideSprites[3];
        }
    }

    public void Setup(ActDie_SO adso, int id)
    {
        dieSides = adso.SideList;
        dieID = id;
    }

    public void Roll()
    {
        currentSideIndex = Random.Range(0, 6);
    }

    public SideType GetCurrentSideType()
    {
        return dieSides[currentSideIndex];
    }

    public float[] GetTypeProbabilities()
    {
        int numStrike = 0;
        int numGuard = 0;
        int numSupport = 0;

        foreach (SideType side in dieSides)
        {
            switch (side)
            {
                case SideType.STRIKE:
                    numStrike++;
                    break;
                case SideType.GUARD:
                    numGuard++;
                    break;
                case SideType.SUPPORT:
                    numSupport++;
                    break;
            }
        }

        return new float[] { (float)numStrike / 6f, (float)numGuard / 6f, (float)numSupport / 6f };
    }
}
