using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "DiceDeck", menuName = "ScriptableObjects/DiceDeck", order = 2)]

public class DiceDeck_SO : ScriptableObject
{
    [SerializeField] ActDie_SO actionDie1;
    [SerializeField] ActDie_SO actionDie2;
    [SerializeField] ActDie_SO actionDie3;
    [SerializeField] ActDie_SO actionDie4;
    [SerializeField] ActDie_SO actionDie5;

    public ActDie_SO GetActionDie(int number)
    {
        switch (number)
        {
            case 1:
                return actionDie1;
            case 2:
                return actionDie2;
            case 3:
                return actionDie3;
            case 4:
                return actionDie4;
            case 5:
                return actionDie5;
            default:
                return null;
        }
    }

    public void ChangeDice(ActDie_SO[] dieList)
    {
        actionDie1 = dieList[0];
        actionDie2 = dieList[1];
        actionDie3 = dieList[2];
        actionDie4 = dieList[3];
        actionDie5 = dieList[4];
    }
}
