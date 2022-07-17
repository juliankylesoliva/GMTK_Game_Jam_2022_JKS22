using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum SetName { NONE, THREE_OF_A_KIND, FOUR_OF_A_KIND, FULL_HOUSE, LITTLE_STRAIGHT, BIG_STRAIGHT, YACHT }

public class SetChecker : MonoBehaviour
{
    public static SetName CheckGivenSet(int[] set, ref int[] bonusArray)
    {
        if (set.Length != 5) { return SetName.NONE; }

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

    public static string GetSetNameString(SetName set)
    {
        switch (set)
        {
            case SetName.NONE:
                return "No Bonus";
            case SetName.THREE_OF_A_KIND:
                return "Three-of-a-Kind";
            case SetName.FOUR_OF_A_KIND:
                return "Four-of-a-Kind";
            case SetName.FULL_HOUSE:
                return "Full House";
            case SetName.LITTLE_STRAIGHT:
                return "Small Straight";
            case SetName.BIG_STRAIGHT:
                return "Large Straight";
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
}
