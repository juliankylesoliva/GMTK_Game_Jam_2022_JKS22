using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class AbilityBase : MonoBehaviour
{
    [SerializeField] GameObject counterPrefab;
    [SerializeField] Transform[] counterLocations;

    [SerializeField] protected string abilityName;
    protected string AbilityName { get { return AbilityName; } }

    [SerializeField] bool is1Available = false;
    [SerializeField] bool is2Available = false;
    [SerializeField] bool is3Available = false;
    [SerializeField] bool is4Available = false;
    [SerializeField] bool is5Available = false;
    [SerializeField] bool is6Available = false;

    private bool[] numbersAvailable = new bool[6];
    private AbilityCounter[] counterRefs = new AbilityCounter[6];

    protected PlayerField assignedField = null;
    public PlayerField AssignedField { set { assignedField = value; } }

    protected TheGameMaster gameMasterRef = null;
    public TheGameMaster GameMasterRef { set { gameMasterRef = value; } }

    void Start()
    {
        Setup();
    }

    protected abstract IEnumerator AbilityProcedure(int dieNum);

    protected abstract bool CounterClickCondition();

    public IEnumerator ActivateAbility()
    {
        AbilityCounter selectedCounter = FindFirstSelectedCounter();
        if (selectedCounter == null) { yield break; }

        selectedCounter.SetState(AbilityCounterState.Used);
        gameMasterRef.MakeAnnouncement($"{(assignedField.PlayerCode == PlayerCode.P1 ? "Player 1" : "Player 2")} activated {abilityName} ({selectedCounter.CurrentNumber})!");
        yield return StartCoroutine(gameMasterRef.WaitForInput());
        yield return StartCoroutine(AbilityProcedure(selectedCounter.CurrentNumber));
    }

    private void Setup()
    {
        numbersAvailable[0] = is1Available;
        numbersAvailable[1] = is2Available;
        numbersAvailable[2] = is3Available;
        numbersAvailable[3] = is4Available;
        numbersAvailable[4] = is5Available;
        numbersAvailable[5] = is6Available;

        for (int i = 0; i < 6; ++i)
        {
            if (numbersAvailable[i])
            {
                InsertCounterInNextOpenSlot(i + 1);
            }
        }
    }

    private void InsertCounterInNextOpenSlot(int num)
    {
        for (int i = 0; i < 6; ++i)
        {
            if (counterLocations[i].transform.childCount <= 0)
            {
                GameObject tempObj = Instantiate(counterPrefab, counterLocations[i]);
                AbilityCounter tempCounter = tempObj.GetComponent<AbilityCounter>();
                tempCounter.SetNumber(num);
                tempCounter.SetClickCondition(CounterClickCondition);
                counterRefs[i] = tempCounter;
                break;
            }
        }
    }

    private AbilityCounter FindFirstSelectedCounter()
    {
        foreach (AbilityCounter counter in counterRefs)
        {
            if (counter != null && counter.CurrentState == AbilityCounterState.Selected)
            {
                return counter;
            }
        }
        return null;
    }

    public void ResetSelectedCounters()
    {
        foreach (AbilityCounter counter in counterRefs)
        {
            if (counter != null && counter.CurrentState == AbilityCounterState.Selected)
            {
                counter.SetState(AbilityCounterState.Unselected);
            }
        }
    }

    public bool ContainsAbilityCounter(AbilityCounter counterObj)
    {
        foreach (AbilityCounter a in counterRefs) {
            if (a != null && GameObject.ReferenceEquals(counterObj, a))
            {
                return true;
            }
        }
        return false;
    }
}
