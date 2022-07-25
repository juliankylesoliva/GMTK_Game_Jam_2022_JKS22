using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ComputerPlayerParameters", menuName = "ScriptableObjects/ComputerPlayerParameters", order = 3)]
public class ComputerPlayerParameters : ScriptableObject
{
    [Range(0, 99)] public int lifePointDifferenceThreshold = 20;
    [Range(1, 99)] public int dangerThreshold = 30;
    public SelectionMethod bonusSelectionMethod = SelectionMethod.BEST_ROLL_EXPECTATION;
    [Range(0, 99)] public int diceRerollProbability = 25;

    [Range(0, 10)] public int strikePriorityWeight = 1;
    [Range(0, 10)] public int guardPriorityWeight = 1;
    [Range(0, 10)] public int supportPriorityWeight = 1;

    public SideType[] strikeMatchupPriorities;
    public SideType[] guardMatchupPriorities;
    public SideType[] supportMatchupPriorities;
}
