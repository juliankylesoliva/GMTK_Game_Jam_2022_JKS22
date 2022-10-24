using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class AbilityBase : MonoBehaviour
{
    [SerializeField] GameObject counterPrefab;
    [SerializeField] Transform[] counterLocations;

    [SerializeField] bool is1Available = false;
    [SerializeField] bool is2Available = false;
    [SerializeField] bool is3Available = false;
    [SerializeField] bool is4Available = false;
    [SerializeField] bool is5Available = false;
    [SerializeField] bool is6Available = false;

    private bool[] numbersAvailable = new bool[6];
    private AbilityCounter[] counterRefs = new AbilityCounter[6];

    void Start()
    {
        Setup();
    }

    protected abstract IEnumerator AbilityProcedure(int dieNum);

    public IEnumerator ActivateAbility(int dieNum)
    {
        yield return StartCoroutine(AbilityProcedure(dieNum));
    }

    private void Setup()
    {
        numbersAvailable[0] = is1Available;
        numbersAvailable[1] = is2Available;
        numbersAvailable[2] = is3Available;
        numbersAvailable[3] = is4Available;
        numbersAvailable[4] = is5Available;
        numbersAvailable[5] = is6Available;

        for (int i = 0; i < 6; ++i)
        {
            if (numbersAvailable[i])
            {
                InsertCounterInNextOpenSlot(i + 1);
            }
        }
    }

    private void InsertCounterInNextOpenSlot(int num)
    {
        for (int i = 0; i < 6; ++i)
        {
            if (counterLocations[i].transform.childCount <= 0)
            {
                GameObject tempObj = Instantiate(counterPrefab, counterLocations[i]);
                AbilityCounter tempCounter = tempObj.GetComponent<AbilityCounter>();
                tempCounter.SetNumber(num);
                counterRefs[i] = tempCounter;
                break;
            }
        }
    }
}
