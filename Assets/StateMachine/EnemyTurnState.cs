using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class EnemyTurnState : TurnBasedState
{
    public static event Action EnemyTurnBegan;
    public static event Action EnemyTurnEnded;

    [SerializeField] float _pauseDuration = 1.5f;

    public override void Enter()
    {
        Debug.Log("Enemy turn:... Enter");
        EnemyTurnBegan?.Invoke();

        StateMachine.Input.PressedWin += OnPressedWin;
        StateMachine.Input.PressedLose += OnPressedLose;

        StartCoroutine(EnemyThinkingRoutine(_pauseDuration));
    }

    public override void Exit()
    {
        StateMachine.Input.PressedWin -= OnPressedWin;
        StateMachine.Input.PressedLose -= OnPressedLose;
        Debug.Log("Enemy Turn: Exit....");
    }

    IEnumerator EnemyThinkingRoutine(float pauseDuration)
    {
        Debug.Log("Enemy thinking...");
        yield return new WaitForSeconds(pauseDuration);

        Debug.Log("Enemy performs action");

        EnemyTurnEnded?.Invoke();

        StateMachine.ChangeState<PlayerTurnState>();
    }

    void OnPressedWin()
    {
        StateMachine.ChangeState<WinState>();
    }

    void OnPressedLose()
    {
        StateMachine.ChangeState<LoseState>();
    }
}
