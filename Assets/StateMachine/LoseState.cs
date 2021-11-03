﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LoseState : State
{
    [SerializeField] GameObject _loseStatePanel;

    public override void Enter()
    {
        Debug.Log("LoseState: ...Entering");
        _loseStatePanel.SetActive(true);
    }

    public override void Exit()
    {
        Debug.Log("LoseState: Exiting...");
    }
}