using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerField : MonoBehaviour
{
    [SerializeField] DiceDeck_SO diceDeck;
    [SerializeField] CurrentRollsField currentRollsFieldObject;
    [SerializeField] ActionOrderField actionOrderFieldObject;

    private ActionDie_Obj[] actionDiceRefs = new ActionDie_Obj[5];

    void Update()
    {
        CheckIfDiceClickedOn();
    }

    private void CheckIfDiceClickedOn()
    {
        if (Input.GetMouseButtonDown(0))
        {
            RaycastHit2D hit = Physics2D.Raycast(Camera.main.ScreenToWorldPoint(Input.mousePosition), Vector2.zero);
            if (hit.collider != null)
            {
                ActionDie_Obj tempDie = hit.transform.gameObject.GetComponent<ActionDie_Obj>();
                if (tempDie != null)
                {
                    if (currentRollsFieldObject.ContainsDiceObject(tempDie))
                    {
                        SendDieToActionOrderField(tempDie.DieID + 1);
                    }
                    else if (actionOrderFieldObject.ContainsDiceObject(tempDie))
                    {
                        TakeDieFromActionOrderField(tempDie.DieID);
                    }
                    else {/* Nothing */}
                }
            }
        }
    }

    public void RollActionDice()
    {
        for (int i = 0; i < 5; ++i)
        {
            ActionDie_Obj tempDie = actionDiceRefs[i];
            if (tempDie == null)
            {
                GameObject tempObj = Instantiate(TheDieFactory.GetActionDiePrefab(), this.transform.position, Quaternion.identity);
                tempObj.tag = "Player";
                tempDie = tempObj.GetComponent<ActionDie_Obj>();
                tempDie.Setup(diceDeck.GetActionDie(i + 1), i);
                actionDiceRefs[i] = tempDie;
                currentRollsFieldObject.SetDieToPosition(tempDie, i + 1);
            }

            if (currentRollsFieldObject.ContainsDiceObject(tempDie))
            {
                tempDie.Roll();
            }
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
