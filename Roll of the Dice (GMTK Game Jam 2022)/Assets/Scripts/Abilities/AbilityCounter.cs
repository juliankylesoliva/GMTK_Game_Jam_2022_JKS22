using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum AbilityCounterState { Unselected, Selected, Used }

public class AbilityCounter : MonoBehaviour
{
    SpriteRenderer counterSprite;

    [SerializeField] Sprite[] spriteList;
    [SerializeField] Color selectedColor;
    [SerializeField] Color usedColor;

    private AbilityCounterState currentCounterState = AbilityCounterState.Unselected;
    public AbilityCounterState CurrentState { get { return currentCounterState; } }

    private int currentNumber = 1;
    public int CurrentNumber { get { return currentNumber; } }

    void Awake()
    {
        counterSprite = this.gameObject.GetComponent<SpriteRenderer>();
    }

    void Start()
    {
        SetState(AbilityCounterState.Unselected);
    }


    public void DoCounterClick()
    {
        switch (currentCounterState)
        {
            case AbilityCounterState.Unselected:
                SetState(AbilityCounterState.Selected);
                break;
            case AbilityCounterState.Selected:
                SetState(AbilityCounterState.Unselected);
                break;
            case AbilityCounterState.Used:
                return;
        }
    }

    public void SetState(AbilityCounterState state)
    {
        currentCounterState = state;

        switch (currentCounterState)
        {
            case AbilityCounterState.Unselected:
                counterSprite.color = Color.white;
                break;
            case AbilityCounterState.Selected:
                counterSprite.color = selectedColor;
                break;
            case AbilityCounterState.Used:
                counterSprite.color = usedColor;
                break;
        }
    }

    public void SetNumber(int num)
    {
        if (num < 1 || num > 6) { return; }
        counterSprite.sprite = spriteList[num - 1];
        currentNumber = num;
    }
}
