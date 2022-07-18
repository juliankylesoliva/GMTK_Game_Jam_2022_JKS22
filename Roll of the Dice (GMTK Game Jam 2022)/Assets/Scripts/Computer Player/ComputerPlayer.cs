using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ComputerPlayer : MonoBehaviour
{
    [SerializeField] DiceDeck_SO deckLists;

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
        yield return null;
    }
}
