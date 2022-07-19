using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum SideType { STRIKE, GUARD, SUPPORT }
public enum DiceType { SPECIALIZED, MIXED, BALANCED }

[CreateAssetMenu(fileName = "ActionDie", menuName = "ScriptableObjects/ActionDie", order = 1)]
public class ActDie_SO : ScriptableObject
{
    [SerializeField] SideType side1 = SideType.STRIKE;
    [SerializeField] SideType side2 = SideType.STRIKE;
    [SerializeField] SideType side3 = SideType.GUARD;
    [SerializeField] SideType side4 = SideType.GUARD;
    [SerializeField] SideType side5 = SideType.SUPPORT;
    [SerializeField] SideType side6 = SideType.SUPPORT;

    [SerializeField] DiceType diceType = DiceType.BALANCED;
    public DiceType DieCategory { get { return diceType; } }

    [SerializeField] SideType primaryType = SideType.STRIKE;
    public SideType Primary { get { return primaryType; } }

    [SerializeField] SideType secondaryType = SideType.GUARD;
    public SideType Secondary { get { return secondaryType; } }

    public SideType[] SideList { get { return new SideType[] { side1, side2, side3, side4, side5, side6 }; } }
}
