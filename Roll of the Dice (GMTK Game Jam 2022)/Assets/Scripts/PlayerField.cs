using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerField : MonoBehaviour
{
    [SerializeField] DiceDeck_SO diceDeck;
    [SerializeField] CurrentRollsField currentRollsFieldObject;
    [SerializeField] ActionOrderField actionOrderFieldObject;

    public void RollActionDice()
    {
        currentRollsFieldObject.ClearField();
        for (int i = 0; i < 5; ++i)
        {
            GameObject tempObj = Instantiate(TheDieFactory.GetActionDiePrefab(), this.transform.position, Quaternion.identity);
            ActionDie_Obj tempDie = tempObj.GetComponent<ActionDie_Obj>();
            tempDie.Setup(diceDeck.GetActionDie(i + 1), i);
            currentRollsFieldObject.SetDieToPosition(tempDie, i + 1);
            tempDie.Roll();
        }
    }

    public void SendDieToActionOrderField(int posNum)
    {
        if (posNum < 1 || posNum > 5) { return; }
        ActionDie_Obj tempDie = currentRollsFieldObject.RemoveDieFromPosition(posNum);
        actionOrderFieldObject.PlaceDieInOrderField(tempDie);
    }

    public void TakeDieFromActionOrderField(int id)
    {
        if (id < 0 || id > 4) { return; }
        ActionDie_Obj tempDie = actionOrderFieldObject.GetDieFromOrderField(id);
        if (tempDie != null)
        {
            currentRollsFieldObject.SetDieToPosition(tempDie, tempDie.DieID + 1);
        }
    }
}
