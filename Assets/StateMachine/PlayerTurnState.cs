using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerTurnState : TurnBasedState
{
    [SerializeField] Text _playerTurnTextUI = null;
    [SerializeField] Button _endTurnBtn = null;

    int _playerTurnCount = 0;

    public override void Enter()
    {
        Debug.Log("Player Turn: ...Entering");
        _playerTurnTextUI.gameObject.SetActive(true);
        _endTurnBtn.gameObject.SetActive(true);

        _playerTurnCount++;
        _playerTurnTextUI.text = "Player turn: " + _playerTurnCount.ToString();

        StateMachine.Input.PressedEndTurn += OnPressedEndTurn;
        StateMachine.Input.PressedWin += OnPressedWin;
        StateMachine.Input.PressedLose += OnPressedLose;
    }

    public override void Exit()
    {
        _playerTurnTextUI.gameObject.SetActive(false);
        _endTurnBtn.gameObject.SetActive(false);
        StateMachine.Input.PressedEndTurn -= OnPressedEndTurn;
        StateMachine.Input.PressedWin -= OnPressedWin;
        StateMachine.Input.PressedLose -= OnPressedLose;
        Debug.Log("Player Turn: Exiting...");

    }

    void OnPressedWin()
    {
        StateMachine.ChangeState<WinState>();
    }

    void OnPressedLose()
    {
        StateMachine.ChangeState<LoseState>();
    }


    void OnPressedEndTurn()
    {
        StateMachine.ChangeState<EnemyTurnState>();
    }
}
