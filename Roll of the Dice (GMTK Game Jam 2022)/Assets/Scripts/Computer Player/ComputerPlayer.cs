using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ComputerPlayer : MonoBehaviour
{
    [SerializeField] PlayerCode playerCode = PlayerCode.P2;
    public PlayerCode PlayerID { get { return playerCode; } }

    [SerializeField] DiceDeck_SO overrideDeck = null; // For debug purposes
    [SerializeField] ActDie_SO[] initialDicePool;

    [Header("ACTION PRIORITY PARAMETERS")]
    [SerializeField, Range(0, 99)] int lifePointDifferenceThreshold = 20;
    [SerializeField, Range(1, 99)] int dangerThreshold = 30;

    TheGameMaster gameMaster;

    void Awake()
    {
        gameMaster = this.gameObject.GetComponent<TheGameMaster>();
    }

    public void ChooseDice()
    {
        StartCoroutine(DiceChooser());
    }

    private IEnumerator DiceChooser()
    {
        List<ActDie_SO> remainingDicePool = CreateInitialDicePool();

        for (int i = 0; i < 5; ++i)
        {
            ActDie_SO chosenDie;

            if (overrideDeck == null)
            {
                int chosenIndex = Random.Range(0, remainingDicePool.Count);
                chosenDie = remainingDicePool[chosenIndex];
                EliminateDiceGivenChoice(chosenDie, ref remainingDicePool);
            }
            else
            {
                chosenDie = overrideDeck.GetActionDie(i + 1);
            }

            yield return new WaitForSeconds(0.5f);

            gameMaster.DieSelectButtonClicked(chosenDie);
            gameMaster.ShowSelectedDie(chosenDie);

            float timer = 1f;
            while (!Input.GetMouseButtonDown(0) && timer > 0)
            {
                yield return null;
                timer -= Time.deltaTime;
            }
            gameMaster.HideSelectedDie();

            yield return new WaitForSeconds(0.5f);
        }

        gameMaster.ConfirmDiceButtonClicked();

        yield return null;
    }

    private List<ActDie_SO> CreateInitialDicePool()
    {
        List<ActDie_SO> result = new List<ActDie_SO>();
        foreach (ActDie_SO d in initialDicePool)
        {
            result.Add(d);
        }
        return result;
    }

    private void EliminateDiceGivenChoice(ActDie_SO chosenDie, ref List<ActDie_SO> remainingDicePool)
    {
        if (chosenDie.DieCategory == DiceType.SPECIALIZED)
        {
            for (int i = 0; i < remainingDicePool.Count;)
            {
                ActDie_SO d = remainingDicePool[i];

                if (d.DieCategory == DiceType.SPECIALIZED)
                {
                    remainingDicePool.RemoveAt(i);
                    i = 0;
                }
                else
                {
                    i++;
                }
            }
        }
        else if (chosenDie.DieCategory == DiceType.MIXED)
        {
            for (int i = 0; i < remainingDicePool.Count;)
            {
                ActDie_SO d = remainingDicePool[i];

                if (d.DieCategory == DiceType.MIXED && (chosenDie.Primary == d.Primary || chosenDie.Secondary == d.Secondary))
                {
                    remainingDicePool.RemoveAt(i);
                    i = 0;
                }
                else
                {
                    i++;
                }
            }
        }
        else {/* Nothing */}
    }

    public void DoActionPhase(bool isFirst, SideType[] opposingActions = null)
    {
        StartCoroutine(SelectActions(isFirst, opposingActions));
    }

    private IEnumerator SelectActions(bool isFirst, SideType[] opposingActions = null)
    {
        List<ActionDieObj> actionDiceOrder = new List<ActionDieObj>();

        gameMaster.RollButtonClick();
        yield return new WaitForSeconds(1.25f);

        if (isFirst)
        {
            GetDiceOrder(ref actionDiceOrder);
        }
        else
        {
            GetDiceOrder(ref actionDiceOrder, opposingActions);
        }

        yield return null;
    }

    private void GetDiceOrder(ref List<ActionDieObj> resultOrder)
    {
        int[] priority = GetPriorityArray();
        PlayerField myField = gameMaster.GetPlayerField(playerCode);
        DieObj[] diceRolls = myField.DiceRolls;

        foreach (DieObj d in diceRolls)
        {
            if (d != null)
            {
                resultOrder.Add((ActionDieObj)d);
            }
        }

        // Sort the list by removing at an index and adding to the end (reduce the max search length every iteration)
    }

    private void GetDiceOrder(ref List<ActionDieObj> resultOrder, SideType[] opposingArray)
    {

    }

    private int[] GetPriorityArray()
    {
        int strikePriorityLevel = 0;
        int guardPriorityLevel = 0;
        int supportPriorityLevel = 0;

        int lpDiff = gameMaster.GetLPDifference(PlayerCode.P2);
        if (lpDiff >= lifePointDifferenceThreshold) // AI has more LP remaining than the player.
        {
            strikePriorityLevel += 2;
            guardPriorityLevel += 1;
        }
        else if (lpDiff <= -lifePointDifferenceThreshold) // AI has fewer LP remaining than the player.
        {
            guardPriorityLevel += 2;
            supportPriorityLevel += 1;
        }
        else // AI has roughly the same amount of LP as the player.
        {
            strikePriorityLevel += 1;
            guardPriorityLevel += 1;
            supportPriorityLevel += 1;
        }

        int lpRemaining = gameMaster.GetLPRemaining(PlayerCode.P2);
        if (lpRemaining <= dangerThreshold)
        {
            guardPriorityLevel += 1;
            supportPriorityLevel += 1;
        }
        else if (lpRemaining > dangerThreshold && lpRemaining < TheGameMaster.MAX_LIFE_POINTS)
        {
            strikePriorityLevel += 1;
            guardPriorityLevel += 1;
        }
        else
        {
            strikePriorityLevel += 2;
            supportPriorityLevel = 0;
        }

        int enemyLPRemaining = gameMaster.GetLPRemaining(PlayerCode.P1);
        if (enemyLPRemaining <= dangerThreshold)
        {
            strikePriorityLevel += 2;
        }
        else
        {
            strikePriorityLevel += 1;
        }

        int sum = strikePriorityLevel + guardPriorityLevel + supportPriorityLevel;

        if (sum > 5)
        {
            for (int i = 0; sum == 5; ++i)
            {
                switch (i % 3)
                {
                    case 0:
                        supportPriorityLevel -= (supportPriorityLevel > 1 ? 1 : 0);
                        break;
                    case 1:
                        guardPriorityLevel -= (guardPriorityLevel > 1 ? 1 : 0);
                        break;
                    case 2:
                        strikePriorityLevel -= (strikePriorityLevel > 1 ? 1 : 0);
                        break;
                }
                sum = strikePriorityLevel + guardPriorityLevel + supportPriorityLevel;
            }
        }

        return new int[] { strikePriorityLevel, guardPriorityLevel, supportPriorityLevel };
    }
}
