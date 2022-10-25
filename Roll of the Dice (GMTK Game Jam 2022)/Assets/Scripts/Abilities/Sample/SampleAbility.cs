using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SampleAbility : AbilityBase
{
    protected override IEnumerator AbilityProcedure(int dieNum)
    {
        Debug.Log($"Die number: {dieNum}");
        yield return new WaitForSeconds(2f);
        yield break;
    }
}
