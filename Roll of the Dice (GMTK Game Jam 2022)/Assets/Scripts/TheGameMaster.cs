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

    [SerializeField] Color strikeColor;
    [SerializeField] Color guardColor;
    [SerializeField] Color supportColor;

    [SerializeField] PlayerField playerField1;
    [SerializeField] PlayerField playerField2;

    [SerializeField] TMP_Text announcerText;

    [SerializeField] GameObject evenOrOddButtons;
    [SerializeField] GameObject firstOrSecondButtons;

    [SerializeField] GameObject rollButton;
    [SerializeField] TMP_Text rollButtonText;

    [SerializeField] GameObject nextPhaseButton;

    [SerializeField] TMP_Text p1LPText;
    [SerializeField] TMP_Text p2LPText;

    [SerializeField] TMP_Text p1SetBonusText;
    [SerializeField] TMP_Text p2SetBonusText;

    [SerializeField] TMP_Text p1PowerText;
    [SerializeField] TMP_Text p2PowerText;

    private const int MAX_LIFE_POINTS = 100;
    private const int BONUS_HEALING = 1;

    private static PlayerCode currentTurn;
    private PlayerCode firstPlayer;
    private static GamePhase currentPhase;

    private bool rollButtonCooldown = false;
    private bool firstRoll = false;
    private int rollsLeft = 0;
    private bool changePhase = false;

    private int flashCounter = 60;

    private int p1MaxLP;
    private int p1CurrentLP;

    private int p2MaxLP;
    private int p2CurrentLP;

    void Awake()
    {
        Application.targetFrameRate = 60;
    }

    void Start()
    {
        p1MaxLP = MAX_LIFE_POINTS;
        p1CurrentLP = startingLifePoints;

        p2MaxLP = MAX_LIFE_POINTS;
        p2CurrentLP = startingLifePoints;

        CoinFlipSetup();
    }

    void Update()
    {
        p1LPText.text = $"P1\n{p1CurrentLP} LP";
        p1LPText.color = (currentPhase != GamePhase.BATTLE && currentTurn == PlayerCode.P1 && flashCounter > 30 ? Color.blue : Color.white);

        p2LPText.text = $"P2\n{p2CurrentLP} LP";
        p2LPText.color = (currentPhase != GamePhase.BATTLE && currentTurn == PlayerCode.P2 && flashCounter > 30 ? Color.red : Color.white);

        if (flashCounter > 0)
        {
            flashCounter--;
        }
        else
        {
            flashCounter = 60;
        }
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

        yield return StartCoroutine(RollButtonCooldown());

        bool isDieEven = (tempDie.GetCurrentSideNumber() % 2 == 0);

        if ((isDieEven && isEvenCalled) || (!isDieEven && !isEvenCalled))
        {
            switch (currentTurn)
            {
                case PlayerCode.P1:
                    currentTurn = PlayerCode.P1;
                    announcerText.text = "Player 1 won the die roll!";
                    yield return new WaitForSeconds(2.0f);
                    announcerText.text = "Player 1, would you like to go first?\n(You will reroll fewer times if you do...)";
                    break;
                case PlayerCode.P2:
                    currentTurn = PlayerCode.P2;
                    announcerText.text = "Player 2 won the die roll!";
                    yield return new WaitForSeconds(2.0f);
                    announcerText.text = "Player 2, would you like to go first?\n(You will reroll fewer times if you do...)";
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
                    announcerText.text = "Player 2, would you like to go first?\n(You will reroll fewer times if you do...)";
                    break;
                case PlayerCode.P2:
                    currentTurn = PlayerCode.P1;
                    announcerText.text = "Player 1 won the die roll!";
                    yield return new WaitForSeconds(2.0f);
                    announcerText.text = "Player 1, would you like to go first?\n(You will reroll fewer times if you do...)";
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

        announcerText.text = $"{(currentTurn == PlayerCode.P1 ? "Player 1" : "Player 2")}, roll and choose your Action Dice!";

        rollButton.SetActive(true);

        while (!changePhase)
        {
            if (rollButtonCooldown)
            {
                if (rollButton.activeSelf)
                {
                    rollButton.SetActive(false);
                }
            }
            else
            {
                if (!rollButton.activeSelf)
                {
                    rollButton.SetActive(true);
                }

            }

            if (rollButtonText.gameObject.activeSelf)
            {
                rollButtonText.text = $"Rolls: {rollsLeft}";
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

        int[] bonusArray = null;
        SetName currentSet = SetName.NONE;

        firstRoll = true;
        rollsLeft = (currentTurn == firstPlayer ? 2 : 3);

        PlayerField currentPlayer = (currentTurn == PlayerCode.P1 ? playerField1 : playerField2);

        announcerText.text = $"{(currentTurn == PlayerCode.P1 ? "Player 1" : "Player 2")}, roll and pair up your Numbered Dice!.";

        rollButton.SetActive(true);

        while (!changePhase)
        {
            if (rollButtonCooldown)
            {
                if (rollButton.activeSelf)
                {
                    rollButton.SetActive(false);
                }
            }
            else
            {
                if (!rollButton.activeSelf)
                {
                    rollButton.SetActive(true);
                }

            }

            if (rollButtonText.gameObject.activeSelf)
            {
                rollButtonText.text = $"Rolls: {rollsLeft}";
            }

            if (rollButton.activeSelf && rollsLeft <= 0)
            {
                rollButton.SetActive(false);
            }

            nextPhaseButton.SetActive((currentTurn == PlayerCode.P1 ? playerField1 : playerField2).IsNumberOrderFieldFull());

            if (nextPhaseButton.activeSelf)
            {
                if (bonusArray == null)
                {
                    currentSet = SetChecker.CheckGivenSet(currentPlayer.GetNumberOrderArray(), ref bonusArray);
                }
                else
                {
                    if (currentTurn == PlayerCode.P1)
                    {
                        p1SetBonusText.gameObject.SetActive(true);

                        p1SetBonusText.text = $"{SetChecker.GetSetNameString(currentSet)}\n\n+{bonusArray[4]} +{bonusArray[3]} +{bonusArray[2]} +{bonusArray[1]} +{bonusArray[0]}";
                    }
                    else
                    {
                        p2SetBonusText.gameObject.SetActive(true);

                        p2SetBonusText.text = $"{SetChecker.GetSetNameString(currentSet)}\n\n+{bonusArray[0]} +{bonusArray[1]} +{bonusArray[2]} +{bonusArray[3]} +{bonusArray[4]}";
                    }
                }
            }
            else
            {
                if (currentTurn == PlayerCode.P1)
                {
                    p1SetBonusText.gameObject.SetActive(false);
                }
                else
                {
                    p2SetBonusText.gameObject.SetActive(false);
                }

                bonusArray = null;
                currentSet = SetName.NONE;
            }

            yield return null;
        }

        changePhase = false;
        rollsLeft = 0;
        nextPhaseButton.SetActive(false);
        rollButton.SetActive(false);

        currentPlayer.AssignActionStrengths();
        currentPlayer.AssignSetBonuses(bonusArray);

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
        if (rollButtonCooldown) { return; }

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
            StartCoroutine(RollButtonCooldown());
        }
    }

    private IEnumerator RollButtonCooldown()
    {
        rollButtonCooldown = true;
        yield return new WaitForSeconds(1f);
        rollButtonCooldown = false;
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
            int p1TotalPower = (p1ActDie.Strength + p1ActDie.Bonus - p1ActDie.Preference);
            if (p1TotalPower < 0)
            {
                p1TotalPower = 0;
            }

            p1ActDie.transform.position += (Vector3.right * 1.25f);
            p1NumDie.transform.position += (Vector3.right * 1.25f);

            ActionDieObj p2ActDie = playerField2.TakeNextActionDie();
            DieObj p2NumDie = playerField2.TakeNextNumberDie();
            SideType p2Action = p2ActDie.GetCurrentSideType();
            int p2TotalPower = (p2ActDie.Strength + p2ActDie.Bonus - p2ActDie.Preference);
            if (p2TotalPower < 0)
            {
                p2TotalPower = 0;
            }

            p2ActDie.transform.position -= (Vector3.right * 1.25f);
            p2NumDie.transform.position -= (Vector3.right * 1.25f);

            p1SetBonusText.gameObject.SetActive(false);
            p2SetBonusText.gameObject.SetActive(false);

            PlayerAdvantage priority = GetMatchupPriority(p1Action, p2Action);
            yield return StartCoroutine(ResolutionStep(priority, p1Action, p1TotalPower, p2Action, p2TotalPower));
            announcerText.text = "";
            announcerText.color = Color.white;

            GameObject.Destroy(p1ActDie.gameObject);
            GameObject.Destroy(p1NumDie.gameObject);
            GameObject.Destroy(p2ActDie.gameObject);
            GameObject.Destroy(p2NumDie.gameObject);

            p1PowerText.gameObject.SetActive(false);
            p2PowerText.gameObject.SetActive(false);

            yield return new WaitForSeconds(1f);

            if (p1CurrentLP <= 0 || p2CurrentLP <= 0)
            {
                if (p1CurrentLP <= 0)
                {
                    DeclareVictor(PlayerCode.P2);
                    yield break;
                }
                else
                {
                    DeclareVictor(PlayerCode.P1);
                    yield break;
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

        p2PowerText.fontSize = 30f;
        p1PowerText.fontSize = 30f;

        foreach (PlayerCode current in playerOrder)
        {
            SideType thisAction = (current == PlayerCode.P1 ? p1Action : p2Action);
            string thisPlayerString = (current == PlayerCode.P1 ? "Player 1" : "Player 2");
            int thisPlayerPower = (current == PlayerCode.P1 ? p1TotalPower : p2TotalPower);
            TMP_Text thisPlayerPowerText = (current == PlayerCode.P1 ? p1PowerText : p2PowerText);

            PlayerCode otherPlayer = (current == PlayerCode.P1 ? PlayerCode.P2 : PlayerCode.P1);
            SideType otherAction = (otherPlayer == PlayerCode.P1 ? p1Action : p2Action);
            string otherPlayerString = (otherPlayer == PlayerCode.P1 ? "Player 1" : "Player 2");
            int otherPlayerPower = (otherPlayer == PlayerCode.P1 ? p1TotalPower : p2TotalPower);
            TMP_Text otherPlayerPowerText = (otherPlayer == PlayerCode.P1 ? p1PowerText : p2PowerText);

            thisPlayerPowerText.text = $"{thisPlayerPower}";
            otherPlayerPowerText.text = $"{otherPlayerPower}";

            announcerText.color = Color.white;

            if (priority != PlayerAdvantage.NEUTRAL && current == playerWithPriority)
            {
                p1PowerText.gameObject.SetActive(true);
                p2PowerText.gameObject.SetActive(true);
                otherPlayerPowerText.fontSize = 20f;
                int damage = 0;
                switch (thisAction)
                {
                    case SideType.STRIKE:
                        announcerText.color = strikeColor;

                        thisPlayerPowerText.color = strikeColor;
                        otherPlayerPowerText.color = supportColor;

                        announcerText.text = $"{thisPlayerString} stopped {otherPlayerString}'s healing by attacking!";
                        yield return StartCoroutine(WaitForInput());
                        damage = (thisPlayerPower * 2);
                        announcerText.text = $"{thisPlayerString} dealt a boosted {damage} damage to {otherPlayerString}";
                        bool isDead = DealDamageTo(otherPlayer, damage, otherAction == SideType.GUARD);
                        if (isDead) { yield break; }
                        break;
                    case SideType.GUARD:
                        announcerText.color = guardColor;

                        thisPlayerPowerText.color = guardColor;
                        otherPlayerPowerText.color = strikeColor;

                        announcerText.text = $"{thisPlayerString} defended against {otherPlayerString}'s attack!";
                        yield return StartCoroutine(WaitForInput());
                        damage = (otherPlayerPower - thisPlayerPower);
                        if (damage < 0) { damage = 0; }
                        announcerText.text = $"{thisPlayerString} held on and took {damage} damage!";
                        DealDamageTo(current, damage, thisAction == SideType.GUARD);
                        break;
                    case SideType.SUPPORT:
                        announcerText.color = supportColor;

                        thisPlayerPowerText.color = supportColor;
                        otherPlayerPowerText.color = guardColor;

                        announcerText.text = $"{thisPlayerString} healed while {otherPlayerString} defended!";
                        yield return StartCoroutine(WaitForInput());
                        int healing = (thisPlayerPower + BONUS_HEALING);
                        announcerText.text = $"{thisPlayerString} recovered a boosted {healing} life points!";
                        RestoreLifePointsTo(current, healing);
                        break;
                }
                yield return StartCoroutine(WaitForInput());
                yield break;
            }
            else
            {
                thisPlayerPowerText.gameObject.SetActive(true);
                switch (thisAction)
                {
                    case SideType.STRIKE:
                        thisPlayerPowerText.color = strikeColor;
                        announcerText.text = $"{thisPlayerString} attacks {otherPlayerString}!";
                        yield return StartCoroutine(WaitForInput());
                        int damage = thisPlayerPower;
                        announcerText.text = $"{otherPlayerString} took {damage} damage!";
                        bool isDead = DealDamageTo(otherPlayer, damage, otherAction == SideType.GUARD);
                        if (isDead) { yield break; }
                        break;
                    case SideType.GUARD:
                        thisPlayerPowerText.color = guardColor;
                        announcerText.text = $"{thisPlayerString} stood guard...";
                        yield return StartCoroutine(WaitForInput());
                        announcerText.text = "...but nothing happened!";
                        break;
                    case SideType.SUPPORT:
                        thisPlayerPowerText.color = supportColor;
                        announcerText.text = $"{thisPlayerString} took time to heal!";
                        yield return StartCoroutine(WaitForInput());
                        int healing = thisPlayerPower;
                        announcerText.text = $"{thisPlayerString} recovered {healing} life points!";
                        RestoreLifePointsTo(current, healing);
                        break;
                }
                yield return StartCoroutine(WaitForInput());
                thisPlayerPowerText.gameObject.SetActive(false);
            }
        }

        yield return null;
    }

    private IEnumerator WaitForInput()
    {
        yield return new WaitForSeconds(1f);
        nextPhaseButton.SetActive(true);
        while (!changePhase)
        {
            yield return null;
        }
        changePhase = false;
        nextPhaseButton.SetActive(false);
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
