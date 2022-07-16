using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public enum GameMode { VS_COMPUTER, SHARED_2PLAYER }
public enum GamePhase { SETUP, ACTION, NUMBER, BATTLE, END }
public enum PlayerAdvantage { PLAYER_ONE, PLAYER_TWO, NEUTRAL }

public class TheGameMaster : MonoBehaviour
{
    [SerializeField] GameMode gameMode = GameMode.SHARED_2PLAYER;
    [SerializeField, Range(1, 100)] int startingLifePoints = 100;

    [SerializeField] PlayerField playerField1;
    [SerializeField] PlayerField playerField2;

    [SerializeField] TMP_Text announcerText;

    [SerializeField] GameObject evenOrOddButtons;
    [SerializeField] GameObject firstOrSecondButtons;

    [SerializeField] GameObject rollButton;
    [SerializeField] TMP_Text rollButtonText;

    [SerializeField] GameObject nextPhaseButton;

    private const int MAX_LIFE_POINTS = 100;
    private const int BONUS_HEALING = 3;

    private static PlayerCode currentTurn;
    private PlayerCode firstPlayer;
    private static GamePhase currentPhase;

    private bool firstRoll = false;
    private int rollsLeft = 0;
    private bool changePhase = false;

    private int p1MaxLP;
    private int p1CurrentLP;

    private int p2MaxLP;
    private int p2CurrentLP;

    void Start()
    {
        p1MaxLP = MAX_LIFE_POINTS;
        p1CurrentLP = startingLifePoints;

        p2MaxLP = MAX_LIFE_POINTS;
        p2CurrentLP = startingLifePoints;

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
        rollButton.SetActive(false);

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
        rollButton.SetActive(false);

        PlayerField currentPlayer = (currentTurn == PlayerCode.P1 ? playerField1 : playerField2);
        currentPlayer.AssignActionStrengths();

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
        currentPhase = GamePhase.BATTLE;

        for (int i = 0; i < 5; ++i)
        {
            ActionDieObj p1ActDie = playerField1.TakeNextActionDie();
            DieObj p1NumDie = playerField1.TakeNextNumberDie();
            SideType p1Action = p1ActDie.GetCurrentSideType();
            int p1TotalPower = (p1ActDie.Strength + p1ActDie.Bonus - p1ActDie.Penalty);
            if (p1TotalPower < 0)
            {
                p1TotalPower = 0;
            }

            p1ActDie.transform.position += (Vector3.right * 2f);
            p1NumDie.transform.position += (Vector3.right * 2f);

            ActionDieObj p2ActDie = playerField2.TakeNextActionDie();
            DieObj p2NumDie = playerField2.TakeNextNumberDie();
            SideType p2Action = p2ActDie.GetCurrentSideType();
            int p2TotalPower = (p2ActDie.Strength + p2ActDie.Bonus - p2ActDie.Penalty);
            if (p2TotalPower < 0)
            {
                p2TotalPower = 0;
            }

            p2ActDie.transform.position -= (Vector3.right * 2f);
            p2NumDie.transform.position -= (Vector3.right * 2f);

            PlayerAdvantage priority = GetMatchupPriority(p1Action, p2Action);
            yield return StartCoroutine(ResolutionStep(priority, p1Action, p1TotalPower, p2Action, p2TotalPower));

            GameObject.Destroy(p1ActDie.gameObject);
            GameObject.Destroy(p1NumDie.gameObject);
            GameObject.Destroy(p2ActDie.gameObject);
            GameObject.Destroy(p2NumDie.gameObject);

            yield return new WaitForSeconds(1f);

            if (p1CurrentLP <= 0 || p2CurrentLP <= 0)
            {
                if (p1CurrentLP <= 0)
                {
                    DeclareVictor(PlayerCode.P2);
                }
                else
                {
                    DeclareVictor(PlayerCode.P1);
                }
            }
        }

        if (p1CurrentLP <= 0 || p2CurrentLP <= 0)
        {
            if (p1CurrentLP <= 0)
            {
                DeclareVictor(PlayerCode.P2);
            }
            else
            {
                DeclareVictor(PlayerCode.P1);
            }
        }
        else
        {
            firstPlayer = (firstPlayer == PlayerCode.P1 ? PlayerCode.P2 : PlayerCode.P1);
            StartCoroutine(ActionPhase(firstPlayer));
        }
    }

    private PlayerAdvantage GetMatchupPriority(SideType p1Action, SideType p2Action)
    {
        switch (p1Action)
        {
            case SideType.STRIKE:
                switch (p2Action)
                {
                    case SideType.STRIKE:
                        return PlayerAdvantage.NEUTRAL;
                    case SideType.GUARD:
                        return PlayerAdvantage.PLAYER_TWO;
                    case SideType.SUPPORT:
                        return PlayerAdvantage.PLAYER_ONE;
                }
                break;
            case SideType.GUARD:
                switch (p2Action)
                {
                    case SideType.STRIKE:
                        return PlayerAdvantage.PLAYER_ONE;
                    case SideType.GUARD:
                        return PlayerAdvantage.NEUTRAL;
                    case SideType.SUPPORT:
                        return PlayerAdvantage.PLAYER_TWO;
                }
                break;
            case SideType.SUPPORT:
                switch (p2Action)
                {
                    case SideType.STRIKE:
                        return PlayerAdvantage.PLAYER_TWO;
                    case SideType.GUARD:
                        return PlayerAdvantage.PLAYER_ONE;
                    case SideType.SUPPORT:
                        return PlayerAdvantage.NEUTRAL;
                }
                break;
        }
        return PlayerAdvantage.NEUTRAL;
    }

    private IEnumerator ResolutionStep(PlayerAdvantage priority, SideType p1Action, int p1TotalPower, SideType p2Action, int p2TotalPower)
    {
        PlayerCode[] playerOrder = new PlayerCode[2];
        PlayerCode playerWithPriority = PlayerCode.P1;
        switch (priority)
        {
            case PlayerAdvantage.PLAYER_ONE:
                playerOrder[0] = PlayerCode.P1;
                playerOrder[1] = PlayerCode.P2;
                playerWithPriority = PlayerCode.P1;
                break;
            case PlayerAdvantage.PLAYER_TWO:
                playerOrder[0] = PlayerCode.P2;
                playerOrder[1] = PlayerCode.P1;
                playerWithPriority = PlayerCode.P2;
                break;
            case PlayerAdvantage.NEUTRAL:
                playerOrder[0] = firstPlayer;
                playerOrder[1] = (firstPlayer == PlayerCode.P1 ? PlayerCode.P2 : PlayerCode.P1);
                break;
        }

        foreach (PlayerCode current in playerOrder)
        {
            SideType thisAction = (current == PlayerCode.P1 ? p1Action : p2Action);
            string thisPlayerString = (current == PlayerCode.P1 ? "Player 1" : "Player2");
            int thisPlayerPower = (current == PlayerCode.P1 ? p1TotalPower : p2TotalPower);

            PlayerCode otherPlayer = (current == PlayerCode.P1 ? PlayerCode.P2 : PlayerCode.P1);
            SideType otherAction = (otherPlayer == PlayerCode.P1 ? p1Action : p2Action);
            string otherPlayerString = (otherPlayer == PlayerCode.P1 ? "Player 1" : "Player 2");
            int otherPlayerPower = (otherPlayer == PlayerCode.P1 ? p1TotalPower : p2TotalPower);

            if (priority != PlayerAdvantage.NEUTRAL && current == playerWithPriority)
            {
                int damage = 0;
                switch (thisAction)
                {
                    case SideType.STRIKE:
                        announcerText.text = $"{thisPlayerString} interrupted {otherPlayerString}'s healing with an attack!";
                        yield return new WaitForSeconds(1f);
                        damage = thisPlayerPower;
                        announcerText.text = $"{thisPlayerString} dealt {damage} damage to {otherPlayerString}";
                        bool isDead = DealDamageTo(otherPlayer, damage, otherAction == SideType.GUARD);
                        Debug.Log($"P1: {p1CurrentLP}/{p1MaxLP} | P2: {p2CurrentLP}/{p2MaxLP}");
                        if (isDead) { yield break; }
                        break;
                    case SideType.GUARD:
                        announcerText.text = $"{thisPlayerString} defended against {otherPlayerString}'s attack!";
                        yield return new WaitForSeconds(1f);
                        damage = (otherPlayerPower - thisPlayerPower);
                        if (damage < 0) { damage = 0; }
                        announcerText.text = $"{thisPlayerString} took {damage} damage and held on!";
                        DealDamageTo(current, damage, thisAction == SideType.GUARD);
                        Debug.Log($"P1: {p1CurrentLP}/{p1MaxLP} | P2: {p2CurrentLP}/{p2MaxLP}");
                        break;
                    case SideType.SUPPORT:
                        announcerText.text = $"{thisPlayerString} healed more since {otherPlayerString} defended!";
                        yield return new WaitForSeconds(1f);
                        int healing = (thisPlayerPower + BONUS_HEALING);
                        announcerText.text = $"{thisPlayerString} recovered a boosted {healing} life points!";
                        RestoreLifePointsTo(current, healing);
                        Debug.Log($"P1: {p1CurrentLP}/{p1MaxLP} | P2: {p2CurrentLP}/{p2MaxLP}");
                        break;
                }
                yield return new WaitForSeconds(1f);
                yield break;
            }
            else
            {
                switch (thisAction)
                {
                    case SideType.STRIKE:
                        announcerText.text = $"{thisPlayerString} attacks {otherPlayerString}!";
                        yield return new WaitForSeconds(1f);
                        int damage = thisPlayerPower;
                        announcerText.text = $"{otherPlayerString} took {damage} damage!";
                        bool isDead = DealDamageTo(otherPlayer, damage, otherAction == SideType.GUARD);
                        Debug.Log($"P1: {p1CurrentLP}/{p1MaxLP} | P2: {p2CurrentLP}/{p2MaxLP}");
                        if (isDead) { yield break; }
                        break;
                    case SideType.GUARD:
                        announcerText.text = $"{thisPlayerString} stood guard...";
                        yield return new WaitForSeconds(1f);
                        announcerText.text = "...but nothing happened!";
                        Debug.Log($"P1: {p1CurrentLP}/{p1MaxLP} | P2: {p2CurrentLP}/{p2MaxLP}");
                        break;
                    case SideType.SUPPORT:
                        announcerText.text = $"{thisPlayerString} took time to heal!";
                        yield return new WaitForSeconds(1f);
                        int healing = thisPlayerPower;
                        announcerText.text = $"{thisPlayerString} recovered {healing} life points!";
                        RestoreLifePointsTo(current, healing);
                        Debug.Log($"P1: {p1CurrentLP}/{p1MaxLP} | P2: {p2CurrentLP}/{p2MaxLP}");
                        break;
                }
                yield return new WaitForSeconds(1f);
            }
        }

        yield return null;
    }

    private bool DealDamageTo(PlayerCode player, int damage, bool isGuarding)
    {
        switch (player)
        {
            case PlayerCode.P1:
                p1CurrentLP -= damage;
                if (p1CurrentLP <= 0)
                {
                    if (isGuarding)
                    {
                        p1CurrentLP = 1;
                        return false;
                    }
                    else
                    {
                        p1CurrentLP = 0;
                        return true;
                    }
                }
                return false;
            case PlayerCode.P2:
                p2CurrentLP -= damage;
                if (p2CurrentLP <= 0)
                {
                    if (isGuarding)
                    {
                        p2CurrentLP = 1;
                        return false;
                    }
                    else
                    {
                        p2CurrentLP = 0;
                        return true;
                    }
                }
                return false;
        }
        return false;
    }

    private void RestoreLifePointsTo(PlayerCode player, int healing)
    {
        switch (player)
        {
            case PlayerCode.P1:
                p1CurrentLP += healing;
                if (p1CurrentLP > p1MaxLP)
                {
                    p1CurrentLP = p1MaxLP;
                }
                break;
            case PlayerCode.P2:
                p2CurrentLP += healing;
                if (p2CurrentLP > p2MaxLP)
                {
                    p2CurrentLP = p2MaxLP;
                }
                break;
        }
    }

    private void DeclareVictor(PlayerCode player)
    {
        currentPhase = GamePhase.END;
        switch (player)
        {
            case PlayerCode.P1:
                announcerText.text = "Player 1 Wins!";
                break;
            case PlayerCode.P2:
                announcerText.text = "Player 2 Wins!";
                break;
        }
    }
}
