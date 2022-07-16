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

    [SerializeField] PlayerField playerField1;
    [SerializeField] PlayerField playerField2;

    [SerializeField] TMP_Text announcerText;

    [SerializeField] GameObject evenOrOddButtons;
    [SerializeField] GameObject firstOrSecondButtons;

    [SerializeField] GameObject rollButton;
    [SerializeField] TMP_Text rollButtonText;

    [SerializeField] GameObject nextPhaseButton;

    private static PlayerCode currentTurn;
    private PlayerCode firstPlayer;
    private static GamePhase currentPhase;

    private bool firstRoll = false;
    private int rollsLeft = 0;
    private bool changePhase = false;

    void Start()
    {
        CoinFlipSetup();
    }

    public static PlayerCode GetCurrentTurn()
    {
        return currentTurn;
    }

    public static GamePhase GetCurrentPhase()
    {
        return currentPhase;
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
                    StartCoroutine(ActionPhase(PlayerCode.P1));
                }
                else
                {
                    firstPlayer = PlayerCode.P2;
                    StartCoroutine(ActionPhase(PlayerCode.P2));
                }
                break;
            case PlayerCode.P2:
                if (isFirstClicked)
                {
                    firstPlayer = PlayerCode.P2;
                    StartCoroutine(ActionPhase(PlayerCode.P2));
                }
                else
                {
                    firstPlayer = PlayerCode.P1;
                    StartCoroutine(ActionPhase(PlayerCode.P1));
                }
                break;
        }
    }

    private IEnumerator ActionPhase(PlayerCode player)
    {
        currentTurn = player;
        currentPhase = GamePhase.ACTION;

        firstRoll = true;
        rollsLeft = (currentTurn == firstPlayer ? 2 : 3);

        announcerText.text = $"{(currentTurn == PlayerCode.P1 ? "Player 1" : "Player 2")}, click on the dice at the Roll Zone to send it to the Action Queue and keep its value. " +
                                 "The opposing pair of dice that are closest to the center go first, followed by the next closest pair and so on. " +
                                 "Click on a die from the Action Queue to send it back to the Roll Zone.";

        rollButton.SetActive(true);

        while (!changePhase)
        {
            if (rollButtonText.gameObject.activeSelf)
            {
                rollButtonText.text = $"Roll ({rollsLeft} Left)";
            }

            if (rollButton.activeSelf && rollsLeft <= 0)
            {
                rollButton.SetActive(false);
            }

            nextPhaseButton.SetActive((currentTurn == PlayerCode.P1 ? playerField1 : playerField2).IsActionOrderFieldFull());

            yield return null;
        }

        changePhase = false;
        rollsLeft = 0;
        nextPhaseButton.SetActive(false);

        StartCoroutine(NumberPhase());
    }

    private IEnumerator NumberPhase()
    {
        currentPhase = GamePhase.NUMBER;

        firstRoll = true;
        rollsLeft = (currentTurn == firstPlayer ? 2 : 3);

        announcerText.text = $"{(currentTurn == PlayerCode.P1 ? "Player 1" : "Player 2")}, click on the dice at the Roll Zone to send it to the Number Queue and keep its value. " +
                                 "Number Dice are paired with their respective Action Dice in the queue and determine the Action Die's strength alongside bonus points for certain number arrangements. " +
                                 "Click on a die from the Number to send it back to the Roll Zone (You cannot send back your Action Dice).";

        rollButton.SetActive(true);

        while (!changePhase)
        {
            if (rollButtonText.gameObject.activeSelf)
            {
                rollButtonText.text = $"Roll ({rollsLeft} Left)";
            }

            if (rollButton.activeSelf && rollsLeft <= 0)
            {
                rollButton.SetActive(false);
            }

            nextPhaseButton.SetActive((currentTurn == PlayerCode.P1 ? playerField1 : playerField2).IsNumberOrderFieldFull());

            yield return null;
        }

        changePhase = false;
        rollsLeft = 0;
        nextPhaseButton.SetActive(false);

        if (currentTurn == firstPlayer)
        {
            PlayerCode nextPlayer = (currentTurn == PlayerCode.P1 ? PlayerCode.P2 : PlayerCode.P1);
            StartCoroutine(ActionPhase(nextPlayer));
        }
        else
        {
            StartCoroutine(BattlePhase());
        }
    }

    public void RollButtonClick()
    {
        PlayerField player = (currentTurn == PlayerCode.P1 ? playerField1 : playerField2);

        if (rollsLeft > 0 && (firstRoll || player.DoesCurrentRollsFieldHaveDice()) && (currentPhase == GamePhase.ACTION || currentPhase == GamePhase.NUMBER))
        {
            firstRoll = false;

            if (currentPhase == GamePhase.ACTION)
            {
                player.RollActionDice();
            }
            else
            {
                player.RollNumberDice();
            }

            rollsLeft--;
        }
    }

    public void PhaseChangeButtonClick()
    {
        if (!changePhase)
        {
            changePhase = true;
        }
    }

    private IEnumerator BattlePhase()
    {
        yield return null;
    }
}
