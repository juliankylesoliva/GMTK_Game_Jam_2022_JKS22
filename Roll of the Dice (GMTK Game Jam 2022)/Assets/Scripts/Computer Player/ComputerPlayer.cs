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

        yield return new WaitForSeconds(1f);
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
                    yield return new WaitForSeconds(0.15f);
                    myField.TakeDieFromActionOrderField(i);
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
                    yield return new WaitForSeconds(0.15f);
                    myField.TakeDieFromActionOrderField(i);
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

    public void DoNumberPhase(SideType[] opposingActions)
    {
        StartCoroutine(SelectNumbers(opposingActions));
    }

    private IEnumerator SelectNumbers(SideType[] opposingActions)
    {
        yield return new WaitForSeconds(0.5f);
        gameMaster.RollButtonClick();
        yield return new WaitForSeconds(1.25f);

        yield return StartCoroutine(GetNumberOrder(opposingActions));

        yield return new WaitForSeconds(1f);
        gameMaster.PhaseChangeButtonClick();
    }

    private IEnumerator GetNumberOrder(SideType[] opposingActions)
    {
        PlayerField myField = gameMaster.GetPlayerField(playerCode);
        DieObj[] diceRolls = myField.DiceRolls;

        PlayerField theirField = gameMaster.GetPlayerField((playerCode == PlayerCode.P1 ? PlayerCode.P2 : PlayerCode.P1));

        while (gameMaster.GetRollsLeft() >= 0)
        {
            for (int i = 0; i < 5; ++i)
            {
                DieObj d = diceRolls[i];
                if (d == null)
                {
                    myField.TakeDieFromNumberOrderField(i);
                    yield return new WaitForSeconds(0.15f);
                }
            }

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
            List<KeyValuePair<float, bool[]>> rerollArrays = new List<KeyValuePair<float, bool[]>>();
            bool[] rerollArray = new bool[5];
            bool rerollDecision = false;
            bool isBonusSet = (SetChecker.CheckGivenSet(dieValues, ref bonusValues) != SetName.NONE);

            Debug.Log($"{dieValues[0]}, {dieValues[1]}, {dieValues[2]}, {dieValues[3]}, {dieValues[4]}");

            if (gameMaster.GetRollsLeft() > 0 && !isBonusSet)
            {
                for (int i = 0; i < 5; ++i)
                {
                    currentValue += bonusValues[i];
                }

                rerollArrays = SetChecker.GetPossibleRerolls(dieValues);

                foreach (KeyValuePair<float, bool[]> kvp in rerollArrays)
                {
                    float expectedResult = 0f;
                    rerollDecision = GetRerollDecision(dieValues, kvp.Value, currentValue, ref expectedResult);
                    if (rerollDecision)
                    {
                        rerollArray = kvp.Value;
                        break;
                    }
                }
            }

            if (rerollDecision && (rerollArray[0] || rerollArray[1] || rerollArray[2] || rerollArray[3] || rerollArray[4]))
            {
                for (int i = 0; i < 5; ++i)
                {
                    if (!rerollArray[i])
                    {
                        yield return new WaitForSeconds(0.15f);
                        myField.SendDieToNumberOrderField(diceRolls[i].DieID + 1);
                    }
                }
                gameMaster.RollButtonClick();
                yield return new WaitForSeconds(1.25f);
            }
            else
            {
                if (opposingActions == null)
                {
                    bool areBonusesMaximizedByPriority = false;
                    bool reverseOrder = false;
                    while (!areBonusesMaximizedByPriority)
                    {
                        areBonusesMaximizedByPriority = true;

                        for (int i = 0; i < 5; ++i)
                        {
                            DieObj d = diceRolls[i];
                            if (d == null)
                            {
                                yield return new WaitForSeconds(0.15f);
                                myField.TakeDieFromNumberOrderField(i);
                                
                            }
                        }

                        for (int i = 0; i < 5; ++i)
                        {
                            DieObj selectedDie = null;
                            foreach (DieObj d in diceRolls)
                            {
                                if (d != null)
                                {
                                    if (selectedDie == null || (!reverseOrder ? d.GetCurrentSideNumber() > selectedDie.GetCurrentSideNumber() : d.GetCurrentSideNumber() < selectedDie.GetCurrentSideNumber()))
                                    {
                                        selectedDie = d;
                                    }
                                }
                            }

                            if (selectedDie != null)
                            {
                                yield return new WaitForSeconds(0.15f);
                                myField.SendDieToNumberOrderField(selectedDie.DieID + 1);
                            }
                        }

                        int[] numberOrder = myField.GetNumberOrderArray();
                        int[] bonusArray = new int[5];
                        SetChecker.CheckGivenSet(numberOrder, ref bonusArray);
                        for (int i = 1; i < 5; ++i)
                        {
                            int currentPair = (numberOrder[i] + bonusArray[i]);
                            int previousPair = (numberOrder[i - 1] + bonusArray[i - 1]);
                            if ((currentPair > previousPair))
                            {
                                areBonusesMaximizedByPriority = false;
                                break;
                            }
                        }

                        reverseOrder = !reverseOrder;
                    }
                }
                else
                {
                    if (isBonusSet)
                    {
                        SideType[] myActions = myField.GetActionOrderArray();
                        SideType[] theirActions = theirField.GetActionOrderArray();

                        int highestTotalOutcomeValue = -1;
                        bool selectedReverseOrder = false;

                        bool reverseOrder = false;
                        for (int i = 0; i < 3; ++i)
                        {
                            for (int j = 0; j < 5; ++j)
                            {
                                DieObj d = diceRolls[j];
                                if (d == null)
                                {
                                    yield return new WaitForSeconds(0.15f);
                                    myField.TakeDieFromNumberOrderField(j);
                                }
                            }

                            for (int j = 0; j < 5; ++j)
                            {
                                DieObj selectedDie = null;
                                foreach (DieObj d in diceRolls)
                                {
                                    if (d != null)
                                    {
                                        if (selectedDie == null || (((i < 2 && !reverseOrder) || (i == 2 && !selectedReverseOrder)) ? d.GetCurrentSideNumber() > selectedDie.GetCurrentSideNumber() : d.GetCurrentSideNumber() < selectedDie.GetCurrentSideNumber()))
                                        {
                                            selectedDie = d;
                                        }
                                    }
                                }

                                if (selectedDie != null)
                                {
                                    yield return new WaitForSeconds(0.15f);
                                    myField.SendDieToNumberOrderField(selectedDie.DieID + 1);
                                }
                            }

                            if (i < 2)
                            {
                                int[] myNumberOrder = myField.GetNumberOrderArray();
                                int[] myBonusArray = new int[5];
                                SetChecker.CheckGivenSet(myNumberOrder, ref myBonusArray);

                                int[] theirNumberOrder = theirField.GetNumberOrderArray();
                                int[] theirBonusArray = new int[5];
                                SetChecker.CheckGivenSet(theirNumberOrder, ref theirBonusArray);

                                int currentTotalOutcomeValue = 0;
                                for (int j = 0; i < 2 && j < 5; ++j)
                                {
                                    SideType myCurrentAction = myActions[j];
                                    int myCurrentPair = (myNumberOrder[j] + myBonusArray[j]);

                                    SideType theirCurrentAction = opposingActions[j];
                                    int theirCurrentPair = (theirNumberOrder[j - 1] + theirBonusArray[j - 1]);

                                    currentTotalOutcomeValue += GetActionOutcomeValue(myCurrentAction, myCurrentPair, theirCurrentAction, theirCurrentPair);
                                }

                                if (currentTotalOutcomeValue > highestTotalOutcomeValue)
                                {
                                    highestTotalOutcomeValue = currentTotalOutcomeValue;
                                    selectedReverseOrder = reverseOrder;
                                }

                                reverseOrder = !reverseOrder;
                            }
                        }
                    }
                    else
                    {
                        SideType[] myActions = myField.GetActionOrderArray();

                        int[] theirNumberOrder = theirField.GetNumberOrderArray();
                        int[] theirBonusArray = new int[5];
                        SetChecker.CheckGivenSet(theirNumberOrder, ref theirBonusArray);

                        for (int j = 0; j < 5; ++j)
                        {
                            DieObj d = diceRolls[j];
                            if (d == null)
                            {
                                myField.TakeDieFromNumberOrderField(j);
                                yield return new WaitForSeconds(0.15f);
                            }
                        }

                        for (int i = 0; i < 5; ++i)
                        {
                            SideType myCurrentAction = myActions[i];
                            SideType theirCurrentAction = opposingActions[i];

                            int theirCurrentPair = (theirNumberOrder[i] + theirBonusArray[i]);

                            DieObj selectedDie = null;
                            int highestOutcome = -999;
                            foreach (DieObj d in diceRolls)
                            {
                                int currentOutcome = 0;
                                if (d != null)
                                {
                                    currentOutcome = GetActionOutcomeValue(myCurrentAction, d.GetCurrentSideNumber(), theirCurrentAction, theirCurrentPair);

                                    if (selectedDie == null || (currentOutcome > highestOutcome) || (currentOutcome == highestOutcome && d.GetCurrentSideNumber() < selectedDie.GetCurrentSideNumber()))
                                    {
                                        highestOutcome = currentOutcome;
                                        selectedDie = d;
                                    }
                                }
                            }

                            if (selectedDie != null)
                            {
                                Debug.Log($"{selectedDie.GetCurrentSideNumber()} {theirNumberOrder[i] + theirBonusArray[i]} ---> {highestOutcome}");
                                yield return new WaitForSeconds(0.15f);
                                myField.SendDieToNumberOrderField(selectedDie.DieID + 1);
                            }
                        }
                    }
                }
                break;
            }
            yield return null;
        }
        yield return null;
    }

    private bool GetRerollDecision(int[] values, bool[] rerolls, int value, ref float expectedResult)
    {
        int[] currentDiceValues = new int[5];
        List<int[]> listOfArrays = new List<int[]>();

        int numDiceToReroll = (rerolls[0] ? 1 : 0) + (rerolls[1] ? 1 : 0) + (rerolls[2] ? 1 : 0) + (rerolls[3] ? 1 : 0) + (rerolls[4] ? 1 : 0);
        int totalPossibleRolls = 7776;
        int maxRollsNeeded = (int)Mathf.Pow(6f, numDiceToReroll);

        int resultCount = 0;
        int resultSum = 0;
        for (int x = 0; x < totalPossibleRolls && resultCount < maxRollsNeeded; ++x)
        {
            int dieVal1 = (rerolls[0] ? ((x % 6) + 1) : values[0]);
            int dieVal2 = (rerolls[1] ? (((x / 6) % 6) + 1) : values[1]);
            int dieVal3 = (rerolls[2] ? (((x / 36) % 6) + 1) : values[2]);
            int dieVal4 = (rerolls[3] ? (((x / 216) % 6) + 1) : values[3]);
            int dieVal5 = (rerolls[4] ? (((x / 1296) % 6) + 1) : values[4]);

            bool checkThisArray = true;
            foreach (int[] array in listOfArrays)
            {
                if (array[0] == dieVal1 && array[1] == dieVal2 && array[2] == dieVal3 && array[3] == dieVal4 && array[4] == dieVal5)
                {
                    checkThisArray = false;
                    break;
                }
            }

            currentDiceValues = new int[] { dieVal1, dieVal2, dieVal3, dieVal4, dieVal5 };

            if (checkThisArray)
            {
                resultSum += SetChecker.GetBestExpectedValue(currentDiceValues);
                resultCount++;
            }
        }
        float expectedValue = ((float)resultSum / (float)maxRollsNeeded);

        Debug.Log($"Expected: {expectedValue} | Current: {value}");

        expectedResult = expectedValue;
        return (expectedValue > ((float)value));
    }

    private void GetNextDiceValues(int[] currentDiceValues, bool[] rerolls, int positionToIncrement = 0)
    {
        if (positionToIncrement >= 0 && positionToIncrement <= 4)
        {
            if (rerolls[positionToIncrement])
            {
                if (currentDiceValues[positionToIncrement] == 6)
                {
                    currentDiceValues[positionToIncrement] = 1;
                    GetNextDiceValues(currentDiceValues, rerolls, positionToIncrement + 1);
                }
                else
                {
                    currentDiceValues[positionToIncrement]++;
                }
            }
            else
            {
                GetNextDiceValues(currentDiceValues, rerolls, positionToIncrement + 1);
            }
        }
    }

    private List<KeyValuePair<int, int>> GetNumberPriority(int[] set)
    {
        List<KeyValuePair<int, int>> numPriorityList = new List<KeyValuePair<int, int>>(); // Key: Die Number, Value: Frequency
        int[] valueCounts = new int[6];

        foreach (int i in set)
        {
            valueCounts[i - 1]++;
        }

        for (int i = 0; i < 6; ++i)
        {
            numPriorityList.Add(new KeyValuePair<int, int>(i + 1, valueCounts[0]));
        }

        for (int i = 0; i < 6; ++i)
        {
            int biggestIndex = -1;
            for (int j = 0; j < (6 - i); ++j)
            {
                KeyValuePair<int, int> currentPair = numPriorityList[i];
                if (biggestIndex == -1 || (currentPair.Value >= numPriorityList[biggestIndex].Value))
                {
                    biggestIndex = j;
                }
            }
            KeyValuePair<int, int> biggestValuedPair = numPriorityList[biggestIndex];
            numPriorityList.RemoveAt(biggestIndex);
            numPriorityList.Add(biggestValuedPair);
        }

        return numPriorityList;
    }

    private int GetActionOutcomeValue(SideType myAction, int myStrength, SideType theirAction, int theirStrength)
    {
        switch(myAction)
        {
            case SideType.STRIKE:
                return GetStrikeOutcome(myStrength, theirAction, theirStrength);
            case SideType.GUARD:
                return GetGuardOutcome(myStrength, theirAction, theirStrength);
            case SideType.SUPPORT:
                return GetSupportOutcome(myStrength, theirAction, theirStrength);
            default:
                return -1;
        }
    }

    private int GetStrikeOutcome(int myStrikeStrength, SideType theirAction, int theirStrength)
    {
        switch (theirAction)
        {
            case SideType.STRIKE:
                return GetStrikeVsStrikeOutcome(myStrikeStrength, theirStrength);
            case SideType.GUARD:
                return GetStrikeVsGuardOutcome(myStrikeStrength, theirStrength);
            case SideType.SUPPORT:
                return GetStrikeVsSupportOutcome(myStrikeStrength, theirStrength);
            default:
                return -1;
        }
    }

    private int GetGuardOutcome(int myGuardStrength, SideType theirAction, int theirStrength)
    {
        switch (theirAction)
        {
            case SideType.STRIKE:
                return GetGuardVsStrikeOutcome(myGuardStrength, theirStrength);
            case SideType.GUARD:
                return GetGuardVsGuardOutcome(myGuardStrength, theirStrength);
            case SideType.SUPPORT:
                return GetGuardVsSupportOutcome(myGuardStrength, theirStrength);
            default:
                return -1;
        }
    }

    private int GetSupportOutcome(int mySupportStrength, SideType theirAction, int theirStrength)
    {
        switch (theirAction)
        {
            case SideType.STRIKE:
                return GetSupportVsStrikeOutcome(mySupportStrength, theirStrength);
            case SideType.GUARD:
                return GetSupportVsGuardOutcome(mySupportStrength, theirStrength);
            case SideType.SUPPORT:
                return GetSupportVsSupportOutcome(mySupportStrength, theirStrength);
            default:
                return -1;
        }
    }

    private int GetStrikeVsStrikeOutcome(int myStrikeStrength, int theirStrikeStrength)
    {
        int myHealth = gameMaster.GetLPRemaining(PlayerID);
        int theirHealth = gameMaster.GetLPRemaining((PlayerID == PlayerCode.P1 ? PlayerCode.P2 : PlayerCode.P1));

        if (theirStrikeStrength >= myHealth) { return -100; }
        else if (myStrikeStrength >= theirHealth) { return 100; }
        else { return (myStrikeStrength - theirStrikeStrength); }
    }

    private int GetStrikeVsGuardOutcome(int myStrikeStrength, int theirGuardStrength)
    {
        int myHealth = gameMaster.GetLPRemaining(PlayerID);

        int damageDealt = (myStrikeStrength - theirGuardStrength);
        if (damageDealt < 0) { damageDealt = 0; }
        int damageReflected = (myStrikeStrength >= theirGuardStrength ? theirGuardStrength : myStrikeStrength);
        if (damageReflected >= myHealth) { return -100; }
        else { return (damageDealt - damageReflected); }
    }

    private int GetStrikeVsSupportOutcome(int myStrikeStrength, int theirSupportStrength)
    {
        int theirHealth = gameMaster.GetLPRemaining((PlayerID == PlayerCode.P1 ? PlayerCode.P2 : PlayerCode.P1));

        int damage = (myStrikeStrength + theirSupportStrength);
        if (damage >= theirHealth) { return 100; }
        else { return damage; }
    }

    private int GetGuardVsStrikeOutcome(int myGuardStrength, int theirStrikeStrength)
    {
        int theirHealth = gameMaster.GetLPRemaining((PlayerID == PlayerCode.P1 ? PlayerCode.P2 : PlayerCode.P1));
        int damageTaken = (theirStrikeStrength - myGuardStrength);
        if (damageTaken < 0) { damageTaken = 0; }
        int damageReflected = (theirStrikeStrength >= myGuardStrength ? myGuardStrength : theirStrikeStrength);
        if (damageReflected >= theirHealth) { return 100; }
        else { return (damageReflected - damageTaken); }
    }

    private int GetGuardVsGuardOutcome(int myGuardStrength, int theirGuardStrength)
    {
        return (theirGuardStrength - myGuardStrength);
    }

    private int GetGuardVsSupportOutcome(int myGuardStrength, int theirSupportStrength)
    {
        int theirHealth = gameMaster.GetLPRemaining((PlayerID == PlayerCode.P1 ? PlayerCode.P2 : PlayerCode.P1));
        int healing = (myGuardStrength + theirSupportStrength);
        if ((theirHealth + healing) > 100) { healing = (100 - theirHealth); }
        return -healing;
    }

    private int GetSupportVsStrikeOutcome(int mySupportStrength, int theirStrikeStrength)
    {
        int myHealth = gameMaster.GetLPRemaining(PlayerID);
        int damageTaken = (mySupportStrength + theirStrikeStrength);
        if (damageTaken >= myHealth) { return -100; }
        else { return -damageTaken; }
    }

    private int GetSupportVsGuardOutcome(int mySupportStrength, int theirGuardStrength)
    {
        int myHealth = gameMaster.GetLPRemaining(PlayerID);
        int healing = (mySupportStrength + theirGuardStrength);
        if ((myHealth + healing) > 100) { healing = (100 - myHealth); }
        return healing;
    }

    private int GetSupportVsSupportOutcome(int mySupportStrength, int theirSupportStrength)
    {
        int myHealth = gameMaster.GetLPRemaining(PlayerID);
        int theirHealth = gameMaster.GetLPRemaining((PlayerID == PlayerCode.P1 ? PlayerCode.P2 : PlayerCode.P1));
        int myHealing = ((myHealth + mySupportStrength) > 100 ? (100 - myHealth) : mySupportStrength);
        int theirHealing = ((theirHealth + theirSupportStrength) > 100 ? (100 - theirHealth) : theirSupportStrength);
        return (myHealing - theirHealing);
    }
}
