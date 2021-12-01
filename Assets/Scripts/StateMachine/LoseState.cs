using System.Collections;
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
        Time.timeScale = 0;
    }

    public override void Exit()
    {
        Debug.Log("LoseState: Exiting...");
        Time.timeScale = 1;
    }
}
