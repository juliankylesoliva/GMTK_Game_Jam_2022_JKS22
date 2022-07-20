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

            yield return new WaitForSeconds(0.1f);

            gameMaster.DieSelectButtonClicked(chosenDie);
            gameMaster.ShowSelectedDie(chosenDie);

            while (!Input.GetMouseButtonDown(0))
            {
                yield return null;
            }
            gameMaster.HideSelectedDie();

            yield return new WaitForSeconds(0.1f);
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

    public void DoActionPhase(SideType[] opposingActions = null)
    {
        StartCoroutine(SelectActions(opposingActions));
    }

    private IEnumerator SelectActions(SideType[] opposingActions)
    {
        yield return new WaitForSeconds(0.5f);
        gameMaster.RollButtonClick();
        yield return new WaitForSeconds(1.25f);

        if (opposingActions != null)
        {
            yield return StartCoroutine(GetCounterActionOrder(opposingActions));
        }
        else
        {
            yield return StartCoroutine(GetActionOrder());
        }
        

        gameMaster.PhaseChangeButtonClick();
    }

    private IEnumerator GetActionOrder()
    {
        int[] priority = GetPriorityArray();

        PlayerField myField = gameMaster.GetPlayerField(playerCode);
        DieObj[] diceRolls = myField.DiceRolls;

        List<KeyValuePair<SideType, int>> priorityOrder = new List<KeyValuePair<SideType, int>>();
        priorityOrder.Add(new KeyValuePair<SideType, int>(SideType.STRIKE, priority[0])); 
        priorityOrder.Add(new KeyValuePair<SideType, int>(SideType.GUARD, priority[1]));
        priorityOrder.Add(new KeyValuePair<SideType, int>(SideType.SUPPORT, priority[2]));
        
        for (int i = 0; i < 3; ++i)
        {
            int biggestIndex = -1;
            for (int j = 0; j < (3 - i); ++j)
            {
                KeyValuePair<SideType, int> currentPair = priorityOrder[i];
                if (biggestIndex == -1 || (currentPair.Value > priorityOrder[biggestIndex].Value))
                {
                    biggestIndex = j;
                }
            }
            KeyValuePair<SideType, int> biggestValuedPair = priorityOrder[biggestIndex];
            priorityOrder.RemoveAt(biggestIndex);
            priorityOrder.Add(biggestValuedPair);
        }

        while (gameMaster.GetRollsLeft() >= 0)
        {
            for (int i = 0; i < 5; ++i)
            {
                DieObj d = diceRolls[i];
                if (d == null)
                {
                    myField.TakeDieFromActionOrderField(i);
                    yield return new WaitForSeconds(0.15f);
                }
            }

            foreach (KeyValuePair<SideType, int> kvp in priorityOrder)
            {
                bool isStillDiceLeft = true;
                int numDiceToGet = kvp.Value;
                while ((numDiceToGet > 0 || gameMaster.GetRollsLeft() == 0) && isStillDiceLeft)
                {
                    isStillDiceLeft = false;

                    ActionDieObj selectedDie = null;
                    float highestProbability = -1f;

                    foreach (DieObj d in diceRolls)
                    {
                        if (d != null)
                        {
                            ActionDieObj currentActDie = (ActionDieObj)d;
                            float currentProbability = currentActDie.GetProbabiltyOfSide(kvp.Key);
                            if ((gameMaster.GetRollsLeft() == 0 && currentActDie.GetCurrentSideType() == kvp.Key) || (currentActDie.GetCurrentSideType() == kvp.Key && currentProbability > highestProbability))
                            {
                                selectedDie = currentActDie;
                                highestProbability = currentProbability;
                                isStillDiceLeft = true;
                            }
                        }
                    }

                    if (selectedDie != null)
                    {
                        yield return new WaitForSeconds(0.15f);
                        numDiceToGet--;
                        myField.SendDieToActionOrderField(selectedDie.DieID + 1);
                    }
                }
            }

            if ((gameMaster.GetRollsLeft() > 0 && myField.DoesCurrentRollsFieldHaveDice()) || myField.DoesCurrentRollsFieldHaveDice())
            {
                gameMaster.RollButtonClick();
                yield return new WaitForSeconds(1.25f);
            }
            else
            {
                break;
            }
        }
        yield return null;
    }

    private IEnumerator GetCounterActionOrder(SideType[] opposingActions)
    {
        PlayerField myField = gameMaster.GetPlayerField(playerCode);
        DieObj[] diceRolls = myField.DiceRolls;

        while (gameMaster.GetRollsLeft() >= 0)
        {
            for (int i = 0; i < 5; ++i)
            {
                DieObj d = diceRolls[i];
                if (d == null)
                {
                    myField.TakeDieFromActionOrderField(i);
                    yield return new WaitForSeconds(0.15f);
                }
            }

            List<KeyValuePair<SideType, int>> priorityOrder = new List<KeyValuePair<SideType, int>>();
            foreach (SideType s in opposingActions)
            {
                priorityOrder.Clear();
                switch (s)
                {
                    case SideType.STRIKE:
                        priorityOrder.Add(new KeyValuePair<SideType, int>(SideType.GUARD, 1));
                        if (gameMaster.GetRollsLeft() == 0)
                        {
                            priorityOrder.Add(new KeyValuePair<SideType, int>(SideType.STRIKE, 1));
                            priorityOrder.Add(new KeyValuePair<SideType, int>(SideType.SUPPORT, 1));
                        }
                        break;
                    case SideType.GUARD:
                        priorityOrder.Add(new KeyValuePair<SideType, int>(SideType.SUPPORT, 1));
                        if (gameMaster.GetRollsLeft() == 0)
                        {
                            priorityOrder.Add(new KeyValuePair<SideType, int>(SideType.GUARD, 1));
                            priorityOrder.Add(new KeyValuePair<SideType, int>(SideType.STRIKE, 1));
                        }
                        break;
                    case SideType.SUPPORT:
                        priorityOrder.Add(new KeyValuePair<SideType, int>(SideType.STRIKE, 1));
                        if (gameMaster.GetRollsLeft() == 0)
                        {
                            priorityOrder.Add(new KeyValuePair<SideType, int>(SideType.SUPPORT, 1));
                            priorityOrder.Add(new KeyValuePair<SideType, int>(SideType.GUARD, 1));
                        }
                        break;
                }

                foreach (KeyValuePair<SideType, int> kvp in priorityOrder)
                {
                    bool isMatchupFound = false;
                    bool isStillDiceLeft = true;
                    int numDiceToGet = kvp.Value;
                    while ((numDiceToGet > 0 || gameMaster.GetRollsLeft() == 0) && isStillDiceLeft)
                    {
                        isStillDiceLeft = false;

                        ActionDieObj selectedDie = null;
                        float highestProbability = -1f;

                        foreach (DieObj d in diceRolls)
                        {
                            if (d != null)
                            {
                                ActionDieObj currentActDie = (ActionDieObj)d;
                                float currentProbability = currentActDie.GetProbabiltyOfSide(kvp.Key);
                                if ((gameMaster.GetRollsLeft() == 0 && currentActDie.GetCurrentSideType() == kvp.Key) || (currentActDie.GetCurrentSideType() == kvp.Key && currentProbability > highestProbability))
                                {
                                    selectedDie = currentActDie;
                                    highestProbability = currentProbability;
                                    isStillDiceLeft = true;
                                    isMatchupFound = true;
                                }
                            }
                        }

                        if (selectedDie != null)
                        {
                            yield return new WaitForSeconds(0.15f);
                            numDiceToGet--;
                            myField.SendDieToActionOrderField(selectedDie.DieID + 1);
                            break;
                        }
                    }
                    if (isMatchupFound) { break; }
                }
            }

            if ((gameMaster.GetRollsLeft() > 0 && myField.DoesCurrentRollsFieldHaveDice()) || myField.DoesCurrentRollsFieldHaveDice())
            {
                gameMaster.RollButtonClick();
                yield return new WaitForSeconds(1.25f);
            }
            else
            {
                break;
            }
        }
        yield return null;
    }

    private int[] GetPriorityArray()
    {
        int strikePriorityLevel = 0;
        int guardPriorityLevel = 0;
        int supportPriorityLevel = 0;

        int lpDiff = gameMaster.GetLPDifference(PlayerCode.P2);
        if (lpDiff != 0 && lpDiff >= lifePointDifferenceThreshold) // AI has more LP remaining than the player.
        {
            strikePriorityLevel += 2;
            guardPriorityLevel += 1;
        }
        else if (lpDiff != 0 && lpDiff <= -lifePointDifferenceThreshold) // AI has fewer LP remaining than the player.
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

    public void DoNumberPhase(bool isFirst)
    {
        StartCoroutine(SelectNumbers(isFirst));
    }

    private IEnumerator SelectNumbers(bool isFirst)
    {
        yield return new WaitForSeconds(0.5f);
        gameMaster.RollButtonClick();
        yield return new WaitForSeconds(1.25f);

        yield return StartCoroutine(GetNumberOrder(isFirst));

        //gameMaster.PhaseChangeButtonClick();
    }

    private IEnumerator GetNumberOrder(bool isFirst)
    {
        PlayerField myField = gameMaster.GetPlayerField(playerCode);
        DieObj[] diceRolls = myField.DiceRolls;

        int[] dieValues = new int[5];
        int currentValue = 0;
        for (int i = 0; i < 5; ++i)
        {
            DieObj d = diceRolls[i];
            if (d != null)
            {
                dieValues[i] = d.GetCurrentSideNumber();
                currentValue += d.GetCurrentSideNumber();
            }
        }

        int[] bonusValues = new int[5];
        SetChecker.CheckGivenSet(dieValues, ref bonusValues);
        for (int i = 0; i < 5; ++i)
        {
            currentValue += bonusValues[i];
        }

        bool[] rerollArray = SetChecker.GetBestReroll(dieValues);

        yield return null;
    }
}