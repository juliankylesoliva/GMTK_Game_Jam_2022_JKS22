using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class DiceQueueHUD : MonoBehaviour
{
    [SerializeField] SpriteRenderer[] spritePositions;
    [SerializeField] TMP_Text[] textPositions;
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
                        textPositions[i].color = TheGameMaster.StrikeColor;
                        break;
                    case SideType.GUARD:
                        spritePositions[i].sprite = spriteList[1];
                        textPositions[i].color = TheGameMaster.GuardColor;
                        break;
                    case SideType.SUPPORT:
                        spritePositions[i].sprite = spriteList[2];
                        textPositions[i].color = TheGameMaster.SupportColor;
                        break;
                }

                int diePower = (currentDie.Strength + currentDie.Bonus);
                if (diePower > 0)
                {
                    textPositions[i].text = $"{diePower}";
                }
                else
                {
                    textPositions[i].text = "--";
                    textPositions[i].color = Color.white;
                }
            }
            else
            {
                spritePositions[i].color = Color.clear;
                spritePositions[i].sprite = spriteList[3];
                textPositions[i].color = Color.white;
                textPositions[i].text = "--";
            }
        }
    }
}
