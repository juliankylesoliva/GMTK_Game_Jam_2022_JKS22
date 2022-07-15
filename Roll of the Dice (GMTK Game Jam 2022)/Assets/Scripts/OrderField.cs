using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OrderField : CurrentRollsField
{
    public void PlaceDieInOrderField(DieObj dieObj)
    {
        for (int i = 0; i < 5; ++i)
        {
            if (diceObjArray[i] == null)
            {
                dieObj.transform.parent = dicePositions[i];
                dieObj.transform.localPosition = Vector3.zero;
                diceObjArray[i] = dieObj;
                return;
            }
        }
    }

    public DieObj GetDieFromOrderField(int id)
    {
        DieObj result = null;

        for (int i = 0; i < 5; ++i)
        {
            DieObj a = diceObjArray[i];
            if (a != null && a.DieID == id)
            {
                result = a;
                diceObjArray[i] = null;
                result.transform.parent = null;
                break;
            }
        }

        ShiftArray();

        return result;
    }

    private void ShiftArray()
    {
        for (int i = 0; i < 4; ++i)
        {
            if (diceObjArray[i] == null)
            {
                for (int j = i + 1; j < 5; ++j)
                {
                    if (diceObjArray[j] != null)
                    {
                        DieObj tempDie = RemoveDieFromPosition(j + 1);
                        PlaceDieInOrderField(tempDie);
                        break;
                    }
                }
            }
        }
    }
}
