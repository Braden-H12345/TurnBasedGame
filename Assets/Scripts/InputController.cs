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

}
