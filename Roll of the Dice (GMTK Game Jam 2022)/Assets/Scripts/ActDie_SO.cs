using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum SideType { STRIKE, GUARD, SUPPORT }

[CreateAssetMenu(fileName = "ActionDie", menuName = "ScriptableObjects/ActionDie", order = 1)]
public class ActDie_SO : ScriptableObject
{
    [SerializeField] SideType side1 = SideType.STRIKE;
    [SerializeField] SideType side2 = SideType.GUARD;
    [SerializeField] SideType side3 = SideType.SUPPORT;

    public SideType[] SideList { get { return new SideType[] { SideType.STRIKE, SideType.GUARD, SideType.SUPPORT, side1, side2, side3 }; } }
}
