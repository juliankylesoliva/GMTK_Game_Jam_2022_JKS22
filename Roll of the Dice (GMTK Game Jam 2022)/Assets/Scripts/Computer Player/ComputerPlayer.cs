using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ComputerPlayer : MonoBehaviour
{
    [SerializeField] DiceDeck_SO overrideDeck = null; // For debug purposes
    [SerializeField] ActDie_SO[] initialDicePool;

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

            yield return new WaitForSeconds(0.5f);

            gameMaster.DieSelectButtonClicked(chosenDie);
            gameMaster.ShowSelectedDie(chosenDie);

            float timer = 1f;
            while (!Input.GetMouseButtonDown(0) && timer > 0)
            {
                yield return null;
                timer -= Time.deltaTime;
            }
            gameMaster.HideSelectedDie();

            yield return new WaitForSeconds(0.5f);
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
}
