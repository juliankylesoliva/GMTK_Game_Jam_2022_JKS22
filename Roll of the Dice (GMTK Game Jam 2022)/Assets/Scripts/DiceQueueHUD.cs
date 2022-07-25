using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DiceQueueHUD : MonoBehaviour
{
    [SerializeField] SpriteRenderer[] spritePositions;
    [SerializeField] Sprite[] spriteList;

    public void UpdateQueueDisplay(DieObj[] diceList)
    {
        for (int i = 0; i < 5; ++i)
        {
            ActionDieObj currentDie = (ActionDieObj)diceList[i];
            if (currentDie != null)
            {
                spritePositions[i].color = Color.white;
                SideType side = currentDie.GetCurrentSideType();
                switch (side)
                {
                    case SideType.STRIKE:
                        spritePositions[i].sprite = spriteList[0];
                        break;
                    case SideType.GUARD:
                        spritePositions[i].sprite = spriteList[1];
                        break;
                    case SideType.SUPPORT:
                        spritePositions[i].sprite = spriteList[2];
                        break;
                }
            }
            else
            {
                spritePositions[i].color = Color.clear;
                spritePositions[i].sprite = spriteList[3];
            }
        }
    }
}
