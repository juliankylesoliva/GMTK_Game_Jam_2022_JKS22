using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AbilityLibrary : MonoBehaviour
{
    [SerializeField] GameObject[] abilityPrefabList;

    private Dictionary<string, GameObject> abilityDict = null;

    void Awake()
    {
        InitializeAbilityDictionary();
    }

    private void InitializeAbilityDictionary()
    {
        if (abilityDict != null) { return; }

        abilityDict = new Dictionary<string, GameObject>();
        foreach (GameObject abilityObj in abilityPrefabList)
        {
            abilityDict.Add(abilityObj.name, abilityObj);
        }
    }

    public GameObject GetAbilityObject(string name)
    {
        if (abilityDict.ContainsKey(name))
        {
            return abilityDict[name];
        }
        return null;
    }
}
