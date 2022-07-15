using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TheDieFactory : MonoBehaviour
{
    [SerializeField] GameObject actionDiePrefab;
    private static GameObject _actionDiePrefab = null;

    void Awake()
    {
        _actionDiePrefab = actionDiePrefab;
    }

    public static GameObject GetActionDiePrefab()
    {
        return _actionDiePrefab;
    }
}
