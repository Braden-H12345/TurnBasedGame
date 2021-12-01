using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class WinState : TurnBasedState
{
    [SerializeField] GameObject _winStatePanel;

    public override void Enter()
    {
        _winStatePanel.SetActive(true);
        Time.timeScale = 0;
    }

    public override void Exit()
    {
        Time.timeScale = 1;
    }
}
