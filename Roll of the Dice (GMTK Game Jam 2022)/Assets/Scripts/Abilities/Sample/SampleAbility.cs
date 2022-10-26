using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SampleAbility : AbilityBase
{
    protected override IEnumerator AbilityProcedure(int dieNum)
    {
        gameMasterRef.MakeAnnouncement($"Die number selected: {dieNum}");
        yield return StartCoroutine(gameMasterRef.WaitForInput());
        yield break;
    }

    protected override bool CounterClickCondition()
    {
        return gameMasterRef.GetLPDifference(assignedField.PlayerCode) < 0;
    }
}
