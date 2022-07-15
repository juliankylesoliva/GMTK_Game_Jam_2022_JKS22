using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CurrentRollsField : MonoBehaviour
{
    [SerializeField] protected Transform[] dicePositions;
    protected ActionDie_Obj[] diceObjArray = new ActionDie_Obj[5];

    public void SetDieToPosition(ActionDie_Obj dieObj, int posNum)
    {
        if (posNum < 1 || posNum > 5) { return; }
        dieObj.transform.parent = dicePositions[posNum - 1];
        dieObj.transform.localPosition = Vector3.zero;
        diceObjArray[posNum - 1] = dieObj;
    }

    public ActionDie_Obj RemoveDieFromPosition(int posNum)
    {
        if (posNum < 1 || posNum > 5) { return null; }

        ActionDie_Obj dieObj = diceObjArray[posNum - 1];
        if (dieObj == null) { return null; }

        diceObjArray[posNum - 1] = null;
        dieObj.transform.parent = null;
        return dieObj;
    }

    public bool ContainsDiceObject(ActionDie_Obj dieObj)
    {
        foreach (ActionDie_Obj d in diceObjArray)
        {
            if (d != null && GameObject.ReferenceEquals(dieObj, d))
            {
                return true;
            }
        }
        return false;
    }

    public void ClearField()
    {
        for (int i = 0; i < 5; ++i)
        {
            ActionDie_Obj tempObj = diceObjArray[i];
            if (tempObj != null)
            {
                GameObject.Destroy(tempObj.gameObject);
            }
        }
        diceObjArray = new ActionDie_Obj[5];
    }
}
