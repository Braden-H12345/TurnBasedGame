using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

public class InputController : MonoBehaviour
{


    public event Action PressedEndTurn = delegate { };
    public event Action PressedWin = delegate { };
    public event Action PressedLose = delegate { };



    public void DetectEndTurn()
    {
        PressedEndTurn?.Invoke();
    }

    public void DetectWin()
    {

        PressedWin?.Invoke();

    }

    public void DetectLost()
    {
        PressedLose?.Invoke();
    }
}
