using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public enum GameMode { VS_COMPUTER, SHARED_2PLAYER }
public enum GamePhase { SETUP, ACTION, NUMBER, END }

public class TheGameMaster : MonoBehaviour
{
    [SerializeField] GameMode gameMode = GameMode.SHARED_2PLAYER;

    [SerializeField] TMP_Text announcerText;

    [SerializeField] GameObject evenOrOddButtons;
    [SerializeField] GameObject firstOrSecondButtons;

    private static PlayerCode currentTurn;
    private PlayerCode firstPlayer;
    private static GamePhase currentPhase;

    void Start()
    {
        CoinFlipSetup();
    }

    private void CoinFlipSetup()
    {
        currentPhase = GamePhase.SETUP;
        if (gameMode == GameMode.VS_COMPUTER || Random.Range(0, 2) == 0)
        {
            currentTurn = PlayerCode.P1;
            announcerText.text = "Player 1, call the die roll!";
        }
        else
        {
            currentTurn = PlayerCode.P2;
            announcerText.text = "Player 2, call the die roll!";
        }
        evenOrOddButtons.SetActive(true);
    }

    public void CoinFlipButtonClicked(bool isEvenCalled)
    {
        evenOrOddButtons.SetActive(false);
        StartCoroutine(CoinFlipProcedure(isEvenCalled));
    }

    private IEnumerator CoinFlipProcedure(bool isEvenCalled)
    {
        GameObject tempObj = Instantiate(TheDieFactory.GetNumberDiePrefab(), Vector3.zero, Quaternion.identity);
        DieObj tempDie = tempObj.GetComponent<DieObj>();
        tempDie.Roll();

        bool isDieEven = (tempDie.GetCurrentSideNumber() % 2 == 0);

        if ((isDieEven && isEvenCalled) || (!isDieEven && !isEvenCalled))
        {
            switch (currentTurn)
            {
                case PlayerCode.P1:
                    currentTurn = PlayerCode.P1;
                    announcerText.text = "Player 1 won the die roll!";
                    yield return new WaitForSeconds(2.0f);
                    announcerText.text = "Player 1, would you like to go first or second?\n(You will reroll fewer times this round if you do...)";
                    break;
                case PlayerCode.P2:
                    currentTurn = PlayerCode.P2;
                    announcerText.text = "Player 2 won the die roll!";
                    yield return new WaitForSeconds(2.0f);
                    announcerText.text = "Player 2, would you like to go first or second?\n(You will reroll fewer times this round if you do...)";
                    break;
            }
        }
        else
        {
            switch (currentTurn)
            {
                case PlayerCode.P1:
                    currentTurn = PlayerCode.P2;
                    announcerText.text = "Player 2 won the die roll!";
                    yield return new WaitForSeconds(2.0f);
                    announcerText.text = "Player 2, would you like to go first or second?\n(You will reroll fewer times this round if you do...)";
                    break;
                case PlayerCode.P2:
                    currentTurn = PlayerCode.P1;
                    announcerText.text = "Player 1 won the die roll!";
                    yield return new WaitForSeconds(2.0f);
                    announcerText.text = "Player 1, would you like to go first or second?\n(You will reroll fewer times this round if you do...)";
                    break;
            }
        }

        GameObject.Destroy(tempDie.gameObject);
        firstOrSecondButtons.SetActive(true);
    }

    public void FirstOrSecondButtonClicked(bool isFirstClicked)
    {
        firstOrSecondButtons.SetActive(false);
        switch (currentTurn)
        {
            case PlayerCode.P1:
                if (isFirstClicked)
                {
                    firstPlayer = PlayerCode.P1;
                    StartCoroutine(SwitchTurnToPlayer(PlayerCode.P1));
                }
                else
                {
                    firstPlayer = PlayerCode.P2;
                    StartCoroutine(SwitchTurnToPlayer(PlayerCode.P2));
                }
                break;
            case PlayerCode.P2:
                if (isFirstClicked)
                {
                    firstPlayer = PlayerCode.P2;
                    StartCoroutine(SwitchTurnToPlayer(PlayerCode.P2));
                }
                else
                {
                    firstPlayer = PlayerCode.P1;
                    StartCoroutine(SwitchTurnToPlayer(PlayerCode.P1));
                }
                break;
        }
    }

    private IEnumerator SwitchTurnToPlayer(PlayerCode player)
    {
        currentTurn = player;
        currentPhase = GamePhase.ACTION;
        yield return null;
    }
}
