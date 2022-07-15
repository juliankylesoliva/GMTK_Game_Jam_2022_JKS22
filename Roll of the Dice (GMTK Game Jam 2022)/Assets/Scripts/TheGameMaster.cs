using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum GameMode { VS_COMPUTER, SHARED_2PLAYER }

public class TheGameMaster : MonoBehaviour
{
    [SerializeField] GameMode gameMode = GameMode.SHARED_2PLAYER;

    void Start()
    {
        
    }

    void Update()
    {
        
    }
}
