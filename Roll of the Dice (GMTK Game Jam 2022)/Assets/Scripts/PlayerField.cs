using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerField : MonoBehaviour
{
    [SerializeField] DiceDeck_SO diceDeck;
    [SerializeField] CurrentRollsField currentRollsField;
    [SerializeField] OrderField actionOrderField;
    [SerializeField] OrderField numberOrderField;

    private ActionDieObj[] actionDiceRefs = new ActionDieObj[5];
    private DieObj[] numberDiceRefs = new DieObj[5];

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
                DieObj tempDie = hit.transform.gameObject.GetComponent<DieObj>();
                if (tempDie != null)
                {
                    if (currentRollsField.ContainsDiceObject(tempDie))
                    {
                        if (tempDie.gameObject.tag == "ActionDie")
                        {
                            SendDieToActionOrderField(tempDie.DieID + 1);
                        }
                        else if (tempDie.gameObject.tag == "NumberDie")
                        {
                            SendDieToNumberOrderField(tempDie.DieID + 1);
                        }
                        else {/* Nothing */}
                    }
                    else if (actionOrderField.ContainsDiceObject(tempDie))
                    {
                        TakeDieFromActionOrderField(tempDie.DieID);
                    }
                    else if (numberOrderField.ContainsDiceObject(tempDie))
                    {
                        TakeDieFromNumberOrderField(tempDie.DieID);
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
            ActionDieObj tempDie = actionDiceRefs[i];
            if (tempDie == null)
            {
                GameObject tempObj = Instantiate(TheDieFactory.GetActionDiePrefab(), this.transform.position, Quaternion.identity);
                tempDie = tempObj.GetComponent<ActionDieObj>();
                tempDie.Setup(diceDeck.GetActionDie(i + 1), i);
                actionDiceRefs[i] = tempDie;
                currentRollsField.SetDieToPosition(tempDie, i + 1);
            }

            if (currentRollsField.ContainsDiceObject(tempDie))
            {
                tempDie.Roll();
            }
        }
    }

    public void RollNumberDice()
    {
        for (int i = 0; i < 5; ++i)
        {
            DieObj tempDie = numberDiceRefs[i];
            if (tempDie == null)
            {
                GameObject tempObj = Instantiate(TheDieFactory.GetNumberDiePrefab(), this.transform.position, Quaternion.identity);
                tempDie = tempObj.GetComponent<DieObj>();
                tempDie.Setup(i);
                numberDiceRefs[i] = tempDie;
                currentRollsField.SetDieToPosition(tempDie, i + 1);
            }

            if (currentRollsField.ContainsDiceObject(tempDie))
            {
                tempDie.Roll();
            }
        }
    }

    public void SendDieToActionOrderField(int posNum)
    {
        if (posNum < 1 || posNum > 5) { return; }
        DieObj tempDie = currentRollsField.RemoveDieFromPosition(posNum);
        actionOrderField.PlaceDieInOrderField(tempDie);
    }

    public void TakeDieFromActionOrderField(int id)
    {
        if (id < 0 || id > 4 || !currentRollsField.IsPositionEmpty(id + 1)) { return; }
        DieObj tempDie = actionOrderField.GetDieFromOrderField(id);
        if (tempDie != null && currentRollsField.IsPositionEmpty(id + 1))
        {
            currentRollsField.SetDieToPosition(tempDie, tempDie.DieID + 1);
        }
    }

    public void SendDieToNumberOrderField(int posNum)
    {
        if (posNum < 1 || posNum > 5) { return; }
        DieObj tempDie = currentRollsField.RemoveDieFromPosition(posNum);
        numberOrderField.PlaceDieInOrderField(tempDie);
    }

    public void TakeDieFromNumberOrderField(int id)
    {
        if (id < 0 || id > 4 || !currentRollsField.IsPositionEmpty(id + 1)) { return; }
        if (!currentRollsField.IsPositionEmpty(id + 1)) { return; }
        DieObj tempDie = numberOrderField.GetDieFromOrderField(id);
        if (tempDie != null)
        {
            currentRollsField.SetDieToPosition(tempDie, tempDie.DieID + 1);
        }
    }
}
