using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(TurnBasedSM))]
public class TurnBasedState : State
{
    protected TurnBasedSM StateMachine { get; private set; }

     void Awake()
    {
        StateMachine = GetComponent<TurnBasedSM>();
    }
}
