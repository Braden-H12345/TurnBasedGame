using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SetupState : TurnBasedState
{
    //can technically play with more players but 2 is default
    [SerializeField] int _numPlayers = 2;

    bool _activated = false;
    public override void Enter()
    {
        Debug.Log("Setup: ... Entering");
        Debug.Log("Creating " + _numPlayers + " players");

        _activated = false;
    }

    public override void Tick()
    {
        if(_activated == false)
        {
            _activated = true;
            StateMachine.ChangeState<PlayerTurnState>();
        }
    }

    public override void Exit()
    {
        Debug.Log("Setup: Exiting...");
    }
}
