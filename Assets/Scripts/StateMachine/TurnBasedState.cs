using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PieceTypes;

[RequireComponent(typeof(TurnBasedSM))]
public class TurnBasedState : State
{
    protected TurnBasedSM StateMachine { get; private set; }

     void Awake()
    {
        StateMachine = GetComponent<TurnBasedSM>();
    }
}
