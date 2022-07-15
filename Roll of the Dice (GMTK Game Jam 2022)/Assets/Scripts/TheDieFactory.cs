using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TheDieFactory : MonoBehaviour
{
    [SerializeField] GameObject actionDiePrefab;
    private static GameObject _actionDiePrefab = null;

    [SerializeField] GameObject numberDiePrefab;
    private static GameObject _numberDiePrefab = null;

    void Awake()
    {
        _actionDiePrefab = actionDiePrefab;
        _numberDiePrefab = numberDiePrefab;
    }

    public static GameObject GetActionDiePrefab()
    {
        return _actionDiePrefab;
    }

    public static GameObject GetNumberDiePrefab()
    {
        return _numberDiePrefab;
    }
}
