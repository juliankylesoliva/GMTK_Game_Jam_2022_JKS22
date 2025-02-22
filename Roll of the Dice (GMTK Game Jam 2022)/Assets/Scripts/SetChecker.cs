using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum SetName { NONE, FOUR_OF_A_KIND, FULL_HOUSE, LITTLE_STRAIGHT, BIG_STRAIGHT, YACHT }

public class SetChecker : MonoBehaviour
{
    public static SetName CheckGivenSet(int[] set, ref int[] bonusArray, bool sort = false)
    {
        if (set.Length != 5) { return SetName.NONE; }

        if (sort)
        {
            for (int i = 0; i < 5; ++i)
            {
                int highestIndex = -1;
                int highestNumber = -1;
                for (int j = 0; j < 5 - i; ++j)
                {
                    int currentNumber = set[j];
                    if (currentNumber > highestNumber)
                    {
                        highestIndex = j;
                        highestNumber = currentNumber;
                    }
                }
                int temp = set[0];
                set[0] = highestNumber;
                set[highestIndex] = temp;
            }
        }

        if (IsYacht(set, ref bonusArray))
        {
            return SetName.YACHT;
        }
        else if (IsFourofAKind(set, ref bonusArray))
        {
            return SetName.FOUR_OF_A_KIND;
        }
        else if (IsFullHouse(set, ref bonusArray))
        {
            return SetName.FULL_HOUSE;
        }
        else if (IsLittleStraight(set, ref bonusArray))
        {
            return SetName.LITTLE_STRAIGHT;
        }
        else if (IsBigStraight(set, ref bonusArray))
        {
            return SetName.BIG_STRAIGHT;
        }
        else
        {
            bonusArray = new int[] { 0, 0, 0, 0, 0 };
            return SetName.NONE;
        }
    }

    public static List<KeyValuePair<float, bool[]>> GetPossibleRerolls(int[] set)
    {
        List<KeyValuePair<float, bool[]>> result = new List<KeyValuePair<float, bool[]>>();

        SetName[] setNames = new SetName[] { SetName.YACHT, SetName.BIG_STRAIGHT, SetName.LITTLE_STRAIGHT, SetName.FOUR_OF_A_KIND, SetName.FULL_HOUSE};

        foreach (SetName n in setNames)
        {
            bool[] tempReroll = new bool[5];
            float expected = GetExpectedValueOfGivenSet(n, set, ref tempReroll);
            Debug.Log($"{GetSetNameString(n)}: {expected}");
            
            result.Add(new KeyValuePair<float, bool[]>(expected, tempReroll));
        }

        for (int j = 0; j < result.Count; ++j)
        {
            int highestIndex = -1;
            float highestExpectation = -1f;
            for (int i = 0; i < (result.Count - j); ++i)
            {
                KeyValuePair<float, bool[]> currentKvp = result[i];
                if (currentKvp.Key > highestExpectation)
                {
                    highestExpectation = currentKvp.Key;
                    highestIndex = i;
                }
            }
            KeyValuePair<float, bool[]> selectedKvp = result[highestIndex];
            result.RemoveAt(highestIndex);
            result.Add(selectedKvp);
        }

        bool[] chanceRerollArray = new bool[5];
        float chanceExpectation = GetExpectedValueOfGivenSet(SetName.NONE, set, ref chanceRerollArray);
        result.Add(new KeyValuePair<float, bool[]>(chanceExpectation, chanceRerollArray));

        return result;
    }

    public static int GetBestExpectedValue(int[] set)
    {
        int[] sorted = set;
        for (int i = 0; i < 5; ++i)
        {
            int highestIndex = -1;
            int highestValue = -1;
            for (int j = i; j < 5; ++j)
            {
                int currentValue = sorted[j];
                if (currentValue > highestValue)
                {
                    highestIndex = j;
                    highestValue = currentValue;
                }
            }
            int tempValue = sorted[i];
            sorted[i] = highestValue;
            sorted[highestIndex] = tempValue;
        }

        int[] bonusArray = new int[5];
        CheckGivenSet(sorted, ref bonusArray);

        int totalValue = 0;
        for (int i = 0; i < 5; ++i)
        {
            totalValue += (sorted[i] + bonusArray[i]);
        }

        return totalValue;
    }

    public static string GetSetNameString(SetName set)
    {
        switch (set)
        {
            case SetName.NONE:
                return "No Bonus";
            case SetName.FOUR_OF_A_KIND:
                return "Four-of-a-Kind";
            case SetName.FULL_HOUSE:
                return "Full House";
            case SetName.LITTLE_STRAIGHT:
                return "Little Straight";
            case SetName.BIG_STRAIGHT:
                return "Big Straight";
            case SetName.YACHT:
                return "Yacht!";
            default:
                return "No Bonus";
        }
    }

    private static bool IsYacht(int[] set, ref int[] bonusArray)
    {
        if (set.Length != 5) { return false; }
        if (set[0] == set[1] && set[1] == set[2] && set[2] == set[3] && set[3] == set[4])
        {
            bonusArray = new int[] { 6, 6, 6, 6, 6 };
            return true;
        }
        return false;
    }

    private static bool IsBigStraight(int[] set, ref int[] bonusArray)
    {
        if (set.Length != 5) { return false; }
        if ((set[0] == 2 && set[1] == 3 && set[2] == 4 && set[3] == 5 && set[4] == 6) || (set[0] == 6 && set[1] == 5 && set[2] == 4 && set[3] == 3 && set[4] == 2))
        {
            bonusArray = new int[] { 5, 5, 5, 5, 5 };
            return true;
        }
        return false;
    }

    private static bool IsLittleStraight(int[] set, ref int[] bonusArray)
    {
        if (set.Length != 5) { return false; }
        if ((set[0] == 1 && set[1] == 2 && set[2] == 3 && set[3] == 4 && set[4] == 5) || (set[0] == 5 && set[1] == 4 && set[2] == 3 && set[3] == 2 && set[4] == 1))
        {
            bonusArray = new int[] { 4, 4, 4, 4, 4 };
            return true;
        }
        return false;
    }

    private static bool IsFourofAKind(int[] set, ref int[] bonusArray)
    {
        if (set.Length != 5) { return false; }

        for (int i = 0; i < 2; ++i)
        {
            if (set[i] == set[i + 1] && set[i + 1] == set[i + 2] && set[i + 2] == set[i + 3])
            {
                bonusArray = new int[] { 0, 0, 0, 0, 0 };
                bonusArray[i] = 4;
                bonusArray[i + 1] = 4;
                bonusArray[i + 2] = 4;
                bonusArray[i + 3] = 4;
                return true;
            }
        }
        return false;
    }

    private static bool IsFullHouse(int[] set, ref int[] bonusArray)
    {
        if (set.Length != 5) { return false; }

        int threeNum = -1;
        for (int i = 0; i < 3 && threeNum <= 0; ++i)
        {
            if (set[i] == set[i + 1] && set[i + 1] == set[i + 2])
            {
                bonusArray = new int[] { 0, 0, 0, 0, 0 };
                bonusArray[i] = 3;
                bonusArray[i + 1] = 3;
                bonusArray[i + 2] = 3;
                threeNum = set[i];
            }
        }

        if (threeNum < 1 || threeNum > 6) { return false; }

        for (int i = 0; i < 4; ++i)
        {
            if (set[i] == set[i + 1] && set[i] != threeNum)
            {
                bonusArray[i] = 2;
                bonusArray[i + 1] = 2;
                return true;
            }
        }
        return false;
    }

    private static float GetExpectedValueOfGivenSet(SetName setName, int[] set, ref bool[] tempReroll)
    {
        switch (setName)
        {
            case SetName.YACHT:
                return GetExpectedValueOfBestYacht(set, ref tempReroll);
            case SetName.BIG_STRAIGHT:
                return GetExpectedValueOfStraight(set, ref tempReroll, true);
            case SetName.LITTLE_STRAIGHT:
                return GetExpectedValueOfStraight(set, ref tempReroll, false);
            case SetName.FOUR_OF_A_KIND:
                return GetExpectedValueOfFourOfAKind(set, ref tempReroll);
            case SetName.FULL_HOUSE:
                return GetExpectedValueOfFullHouse(set, ref tempReroll);
            case SetName.NONE:
                return GetExpectedValueOfChance(set, ref tempReroll);
            default:
                return -1f;
        }
    }

    private static float GetExpectedValueOfBestYacht(int[] set, ref bool[] tempReroll)
    {
        int[] valueCounts = new int[6];

        for (int i = 0; i < 5; ++i)
        {
            int currentValue = set[i];
            valueCounts[currentValue - 1]++;
        }

        int mode = -1;
        int count = 0;
        for (int i = 0; i < 6; ++i)
        {
            int currentCount = valueCounts[i];
            if (currentCount >= count)
            {
                count = currentCount;
                mode = (i + 1);
            }
        }

        for (int i = 0; i < 5; ++i)
        {
            int currentValue = set[i];
            if (currentValue != mode)
            {
                tempReroll[i] = true;
            }
        }

        if (count == 5)
        {
            for (int i = 0; i < 5; ++i)
            {
                tempReroll[i] = false;
            }
            return 0f;
        }

        float value = (float)(30 + (mode * 5));
        float probability = Mathf.Pow((1f / 6f), (5 - count));

        return (value * probability);
    }

    private static float GetExpectedValueOfStraight(int[] set, ref bool[] tempReroll, bool isBigStraight = false)
    {
        for (int i = (isBigStraight ? 2 : 1); i <= (isBigStraight ? 6 : 5); ++i)
        {
            for (int j = 0; j < 5; ++j)
            {
                int currentValue = set[j];
                if (currentValue == i)
                {
                    tempReroll[j] = true; // Mark the numbers that are part of the big straight as true
                    break;
                }
            }
        }

        int numDiceToReroll = 0;

        // Flip all of the booleans
        for (int i = 0; i < 5; ++i)
        {
            tempReroll[i] = !tempReroll[i];
            if (tempReroll[i])
            {
                numDiceToReroll++;
            }
        }

        if (numDiceToReroll == 5)
        {
            for (int i = 0; i < 5; ++i)
            {
                tempReroll[i] = false;
            }
            return 0f;
        }

        float value = (isBigStraight ? 45f : 35f);
        float probability = Mathf.Pow((1f / 6f), numDiceToReroll);

        return (value * probability);
    }

    private static float GetExpectedValueOfFourOfAKind(int[] set, ref bool[] tempReroll)
    {
        int[] valueCounts = new int[6];

        for (int i = 0; i < 5; ++i)
        {
            int currentValue = set[i];
            valueCounts[currentValue - 1]++;
        }

        int mode = -1;
        int count = 0;
        for (int i = 0; i < 6; ++i)
        {
            int currentCount = valueCounts[i];
            if (currentCount >= count)
            {
                count = currentCount;
                mode = (i + 1);
            }
        }

        int highest = -1;
        for (int i = 0; i < 6; ++i)
        {
            int currentCount = valueCounts[i];
            if (currentCount > 0 && ((i + 1) != mode) && (i + 1) >= highest)
            {
                highest = (i + 1);
            }
        }

        if (count == 5 || count == 4)
        {
            for (int i = 0; i < 5; ++i)
            {
                tempReroll[i] = false;
            }
            return 0f;
        }

        for (int i = 0; i < 5; ++i)
        {
            int currentValue = set[i];
            if (currentValue != mode && currentValue != highest)
            {
                tempReroll[i] = true;
            }
        }

        float value = (float)(16 + highest + (4 * mode));
        float probability = Mathf.Pow((1f / 6f), (5 - (count + (highest != -1 ? 1 : 0))));

        return (value * probability);
    }

    private static float GetExpectedValueOfFullHouse(int[] set, ref bool[] tempReroll)
    {
        int[] valueCounts = new int[6];

        for (int i = 0; i < 5; ++i)
        {
            int currentValue = set[i];
            valueCounts[currentValue - 1]++;
        }

        int mode = -1;
        int count = 0;
        for (int i = 0; i < 6; ++i)
        {
            int currentCount = valueCounts[i];
            if (currentCount >= count)
            {
                count = currentCount;
                mode = (i + 1);
            }
        }

        int mode2 = -1;
        int count2 = 0;
        for (int i = 0; i < 6; ++i)
        {
            int currentCount = valueCounts[i];
            if (currentCount >= count2 && ((i + 1) != mode))
            {
                count2 = currentCount;
                mode2 = (i + 1);
            }
        }

        for (int i = 0; i < 5; ++i)
        {
            int currentValue = set[i];
            if (currentValue != mode && currentValue != mode2)
            {
                tempReroll[i] = true;
            }
        }

        if ((count + count2) == 5)
        {
            for (int i = 0; i < 5; ++i)
            {
                tempReroll[i] = false;
            }
            return 0f;
        }

        float value = (float)(13 + (3 * mode) + (2 * mode2));
        float probability = Mathf.Pow((1f / 6f), (5 - (count + count2)));
        return (value * probability);
    }

    private static float GetExpectedValueOfChance(int[] set, ref bool[] tempReroll)
    {
        int[] valueCounts = new int[6];

        for (int i = 0; i < 5; ++i)
        {
            int currentValue = set[i];
            valueCounts[currentValue - 1]++;
        }

        int mode = -1;
        int count = 0;
        for (int i = 0; i < 6; ++i)
        {
            int currentCount = valueCounts[i];
            if (currentCount >= count)
            {
                count = currentCount;
                mode = (i + 1);
            }
        }

        int highest = -1;
        for (int i = 0; i < 6; ++i)
        {
            int currentCount = valueCounts[i];
            if (currentCount > 0 && ((i + 1) != mode) && (i + 1) >= highest)
            {
                highest = (i + 1);
            }
        }

        int highest2 = -1;
        for (int i = 0; i < 6; ++i)
        {
            int currentCount = valueCounts[i];
            if (currentCount >= 0 && ((i + 1) != mode) && (i + 1) >= highest2)
            {
                highest2 = (i + 1);
            }
        }

        int[] valueCounts2 = new int[6];
        for (int i = 0; i < 5; ++i)
        {
            int currentValue = set[i];
            if (currentValue != mode && currentValue != highest && currentValue != highest2 && ((currentValue == highest || currentValue == highest2) && valueCounts2[currentValue - 1] > 0))
            {
                tempReroll[i] = true;
            }
            else
            {
                valueCounts2[currentValue - 1]++;
            }
        }

        if (highest == -1 || highest2 == -1)
        {
            for (int i = 0; i < 5; ++i)
            {
                tempReroll[i] = false;
            }
            return 0f;
        }

        float value = (float)((3 * mode) + highest + highest2);
        float probability = Mathf.Pow((1f / 6f), (5 - (count + (highest != -1 ? 1 : 0) + (highest2 != -1 ? 1 : 0))));
        return (value * probability);
    }
}
