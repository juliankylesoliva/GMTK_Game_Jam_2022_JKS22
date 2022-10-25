using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum PlayerCode { P1, P2 }

public class PlayerField : MonoBehaviour
{
    [SerializeField] PlayerCode playerCode;

    [SerializeField] DiceDeck_SO diceDeck;

    [SerializeField] CurrentRollsField currentRollsField;
    public DieObj[] DiceRolls { get { return currentRollsField.GetDiceObjectArray(); } }
    public int[] NumberRolls { get { return currentRollsField.GetDiceValueArray(); } }
    public SideType[] ActionRolls { get { return currentRollsField.GetDiceActionArray(); } }

    [SerializeField] OrderField actionOrderField;
    public DieObj[] ActionOrder { get { return actionOrderField.GetDiceObjectArray(); } }
    public SideType[] ActionOrderTypes { get { return actionOrderField.GetDiceActionArray(); } }

    [SerializeField] OrderField numberOrderField;
    public DieObj[] NumberOrder { get { return numberOrderField.GetDiceObjectArray(); } }
    public int[] NumberOrderValues { get { return numberOrderField.GetDiceValueArray(); } }

    [SerializeField] AbilityBase chosenAbility;
    public AbilityBase ChosenAbility { get { return chosenAbility; } set { chosenAbility = value; } }

    AudioSource audioSource;

    private ActionDieObj[] actionDiceRefs = new ActionDieObj[5];
    private DieObj[] numberDiceRefs = new DieObj[5];

    private Vector3 originalPosition;

    private bool isComputerControlled = false;
    public bool IsCPU
    {
        get { return isComputerControlled; }
        set { isComputerControlled = value; }
    }

    void Awake()
    {
        audioSource = this.gameObject.GetComponent<AudioSource>();
        originalPosition = this.transform.position;
    }

    void Update()
    {
        if (!isComputerControlled)
        {
            CheckIfDiceClickedOn();
            CheckIfAbilityCounterClickedOn();
        }
    }

    public void SetDiceDeck(ActDie_SO[] diceList)
    {
        diceDeck.ChangeDice(diceList);
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

            if (!tempDie.IsRolling && currentRollsField.ContainsDiceObject(tempDie))
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

            if (!tempDie.IsRolling && currentRollsField.ContainsDiceObject(tempDie))
            {
                tempDie.Roll();
            }
        }
    }

    public SideType[] GetActionOrderArray()
    {
        return actionOrderField.GetDiceActionArray();
    }

    public int[] GetNumberOrderArray()
    {
        return numberOrderField.GetDiceValueArray();
    }

    public void AssignActionStrengths()
    {
        DieObj[] numberDiceOrder = numberOrderField.GetDiceObjectArray();
        DieObj[] actionDiceOrder = actionOrderField.GetDiceObjectArray();
        for (int i = 0; i < 5; ++i)
        {
            ActionDieObj action = (ActionDieObj)actionDiceOrder[i];
            DieObj number = numberDiceOrder[i];

            if (action != null && number != null)
            {
                action.Strength = number.GetCurrentSideNumber();
            }
        }
    }

    public void AssignSetBonuses(int[] bonusArray)
    {
        DieObj[] actionDiceOrder = actionOrderField.GetDiceObjectArray();
        for (int i = 0; i < 5; ++i)
        {
            ActionDieObj action = (ActionDieObj)actionDiceOrder[i];

            if (action != null && bonusArray != null)
            {
                action.Bonus = bonusArray[i];
            }
        }
    }

    private void CheckIfDiceClickedOn()
    {
        if (IsItMyTurnYet() && Input.GetMouseButtonDown(0))
        {
            RaycastHit2D hit = Physics2D.Raycast(Camera.main.ScreenToWorldPoint(Input.mousePosition), Vector2.zero);
            if (hit.collider != null)
            {
                DieObj tempDie = hit.transform.gameObject.GetComponent<DieObj>();
                if (tempDie != null)
                {
                    if (tempDie.IsRolling) { return; }

                    if (currentRollsField.ContainsDiceObject(tempDie))
                    {
                        if (tempDie.gameObject.tag == "ActionDie" && TheGameMaster.GetCurrentPhase() == GamePhase.ACTION)
                        {
                            SendDieToActionOrderField(tempDie.DieID + 1);
                        }
                        else if (tempDie.gameObject.tag == "NumberDie" && TheGameMaster.GetCurrentPhase() == GamePhase.NUMBER)
                        {
                            SendDieToNumberOrderField(tempDie.DieID + 1);
                        }
                        else {/* Nothing */}
                    }
                    else if (actionOrderField.ContainsDiceObject(tempDie) && TheGameMaster.GetCurrentPhase() == GamePhase.ACTION)
                    {
                        TakeDieFromActionOrderField(tempDie.DieID);
                    }
                    else if (numberOrderField.ContainsDiceObject(tempDie) && TheGameMaster.GetCurrentPhase() == GamePhase.NUMBER)
                    {
                        TakeDieFromNumberOrderField(tempDie.DieID);
                    }
                    else {/* Nothing */}
                }
            }
        }
    }

    private void CheckIfAbilityCounterClickedOn()
    {
        if (IsItMyTurnYet() && TheGameMaster.GetCurrentPhase() == GamePhase.NUMBER && Input.GetMouseButtonDown(0))
        {
            RaycastHit2D hit = Physics2D.Raycast(Camera.main.ScreenToWorldPoint(Input.mousePosition), Vector2.zero);
            if (hit.collider != null)
            {
                AbilityCounter tempCounter = hit.transform.gameObject.GetComponent<AbilityCounter>();
                if (tempCounter != null && chosenAbility.ContainsAbilityCounter(tempCounter))
                {
                    if (tempCounter.CurrentState != AbilityCounterState.Selected) { chosenAbility.ResetSelectedCounters(); }
                    tempCounter.DoCounterClick();
                }
            }
        }
    }

    public bool IsCurrentRollsFieldEmpty()
    {
        return currentRollsField.IsEmpty();
    }

    public bool DoesCurrentRollsFieldHaveDice()
    {
        return currentRollsField.ContainsDice();
    }

    public int GetNumDiceInRollsField()
    {
        return currentRollsField.GetNumDiceInField();
    }

    public void SendDieToActionOrderField(int posNum)
    {
        if (posNum < 1 || posNum > 5) { return; }
        PlaySound("diceKeep", 0.85f);
        DieObj tempDie = currentRollsField.RemoveDieFromPosition(posNum);
        actionOrderField.PlaceDieInOrderField(tempDie);
    }

    public void TakeDieFromActionOrderField(int id)
    {
        if (id < 0 || id > 4 || !currentRollsField.IsPositionEmpty(id + 1)) { return; }
        DieObj tempDie = actionOrderField.GetDieFromOrderField(id);
        if (tempDie != null && currentRollsField.IsPositionEmpty(id + 1))
        {
            PlaySound("diceTake", 0.85f);
            currentRollsField.SetDieToPosition(tempDie, tempDie.DieID + 1);
        }
    }

    public ActionDieObj TakeNextActionDie()
    {
        return (ActionDieObj)actionOrderField.GetNextDie();
    }

    public bool IsActionOrderFieldFull()
    {
        return actionOrderField.IsFull();
    }

    public void SendDieToNumberOrderField(int posNum)
    {
        if (posNum < 1 || posNum > 5) { return; }
        PlaySound("diceKeep", 0.85f);
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
            PlaySound("diceTake", 0.85f);
            currentRollsField.SetDieToPosition(tempDie, tempDie.DieID + 1);
        }
    }

    public DieObj TakeNextNumberDie()
    {
        return numberOrderField.GetNextDie();
    }

    public bool IsNumberOrderFieldFull()
    {
        return numberOrderField.IsFull();
    }

    private bool IsItMyTurnYet()
    {
        return playerCode == TheGameMaster.GetCurrentTurn();
    }

    public void HideField()
    {
        this.transform.position = (originalPosition + (Vector3.up * -50f));
    }

    public void ShowField()
    {
        this.transform.position = originalPosition;
    }

    public void PlaySound(string clipName, float volume)
    {
        AudioClip clipToPlay = SoundLibrary.GetAudioClip(clipName);
        if (clipToPlay != null)
        {
            audioSource.clip = clipToPlay;
            if (volume > 1f)
            {
                audioSource.volume = 1f;
            }
            else if (volume < 0f)
            {
                audioSource.volume = 0f;
            }
            else
            {
                audioSource.volume = volume;
            }
            audioSource.Play();
        }
    }
}
