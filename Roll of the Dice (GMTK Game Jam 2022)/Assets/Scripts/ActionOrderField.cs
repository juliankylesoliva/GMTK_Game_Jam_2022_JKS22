using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActionOrderField : CurrentRollsField
{
    public void PlaceDieInOrderField(ActionDie_Obj dieObj)
    {
        for (int i = 0; i < 5; ++i)
        {
            if (diceObjArray[i] != null)
            {
                dieObj.transform.parent = dicePositions[i];
                dieObj.transform.localPosition = Vector3.zero;
                diceObjArray[i] = dieObj;
                return;
            }
        }
    }

    public ActionDie_Obj GetDieFromOrderField(int id)
    {
        ActionDie_Obj result = null;

        for (int i = 0; i < 5; ++i)
        {
            ActionDie_Obj a = diceObjArray[i];
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
                        ActionDie_Obj tempDie = RemoveDieFromPosition(j + 1);
                        PlaceDieInOrderField(tempDie);
                        break;
                    }
                }
            }
        }
    }
}
