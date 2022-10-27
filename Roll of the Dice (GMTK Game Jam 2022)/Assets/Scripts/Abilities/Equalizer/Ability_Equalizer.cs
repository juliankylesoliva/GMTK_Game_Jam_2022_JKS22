using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ability_Equalizer : AbilityBase
{
    protected override IEnumerator AbilityProcedure(int dieNum)
    {
        PlayerCode player = assignedField.PlayerCode;
        int baseDamage = -gameMasterRef.GetLPDifference(player);
        baseDamage *= assignedField.GetAmountOfDiceWithGivenNumber(dieNum);
        baseDamage /= 5;

        gameMasterRef.MakeAnnouncement($"{(player == PlayerCode.P1 ? "Player 1" : "Player 2")} dealt {baseDamage} damage to {(player == PlayerCode.P1 ? "Player 2" : "Player 1")}!");
        gameMasterRef.PlaySound("shipCritDamage", 0.75f);
        gameMasterRef.DealDamageTo((player == PlayerCode.P1 ? PlayerCode.P2 : PlayerCode.P1), baseDamage, false);
        yield return StartCoroutine(gameMasterRef.WaitForInput());
        yield break;
    }

    protected override bool CounterClickCondition()
    {
        return gameMasterRef.GetLPDifference(assignedField.PlayerCode) < 0;
    }
}
