using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DiePreview : MonoBehaviour
{
    [SerializeField] Sprite[] spriteList;
    [SerializeField] SpriteRenderer[] previewSprites;

    public void SetSides(SideType[] sideList)
    {
        for (int i = 0; i < sideList.Length; ++i)
        {
            SideType side = sideList[i];
            switch (side)
            {
                case SideType.STRIKE:
                    previewSprites[i].sprite = spriteList[0];
                    break;
                case SideType.GUARD:
                    previewSprites[i].sprite = spriteList[1];
                    break;
                case SideType.SUPPORT:
                    previewSprites[i].sprite = spriteList[2];
                    break;
                default:
                    previewSprites[i].sprite = spriteList[3];
                    break;
            }
        }
    }
}
