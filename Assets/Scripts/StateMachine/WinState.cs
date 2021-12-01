using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class WinState : TurnBasedState
{
    [SerializeField] GameObject _winStatePanel;

    public override void Enter()
    {
        Debug.Log("WinState: ...Entering");
        _winStatePanel.SetActive(true);
        Time.timeScale = 0;
    }

    public override void Exit()
    {
        Debug.Log("Winstate: Exiting...");
        Time.timeScale = 1;
    }
}
