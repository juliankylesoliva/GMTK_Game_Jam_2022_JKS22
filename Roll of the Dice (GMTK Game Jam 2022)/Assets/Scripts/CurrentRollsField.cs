using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CurrentRollsField : MonoBehaviour
{
    [SerializeField] protected Transform[] dicePositions;
    protected DieObj[] diceObjArray = new DieObj[5];

    public void SetDieToPosition(DieObj dieObj, int posNum)
    {
        if (posNum < 1 || posNum > 5) { return; }
        dieObj.transform.parent = dicePositions[posNum - 1];
        dieObj.transform.localPosition = Vector3.zero;
        diceObjArray[posNum - 1] = dieObj;
    }

    public DieObj RemoveDieFromPosition(int posNum)
    {
        if (posNum < 1 || posNum > 5) { return null; }

        DieObj dieObj = diceObjArray[posNum - 1];
        if (dieObj == null) { return null; }

        diceObjArray[posNum - 1] = null;
        dieObj.transform.parent = null;
        return dieObj;
    }

    public bool IsPositionEmpty(int posNum)
    {
        if (posNum < 1 || posNum > 5) { return false; }

        DieObj dieObj = diceObjArray[posNum - 1];
        return dieObj == null;
    }

    public bool ContainsDiceObject(DieObj dieObj)
    {
        foreach (DieObj d in diceObjArray)
        {
            if (d != null && GameObject.ReferenceEquals(dieObj, d))
            {
                return true;
            }
        }
        return false;
    }

    public bool ContainsDice()
    {
        foreach (DieObj d in diceObjArray)
        {
            if (d != null) { return true; }
        }
        return false;
    }

    public bool IsFull()
    {
        foreach (DieObj d in diceObjArray)
        {
            if (d == null)
            {
                return false;
            }
        }
        return true;
    }

    public bool IsEmpty()
    {
        foreach (DieObj d in diceObjArray)
        {
            if (d != null)
            {
                return false;
            }
        }
        return true;
    }

    public void ClearField()
    {
        for (int i = 0; i < 5; ++i)
        {
            DieObj tempObj = diceObjArray[i];
            if (tempObj != null)
            {
                GameObject.Destroy(tempObj.gameObject);
            }
        }
        diceObjArray = new DieObj[5];
    }
}
