using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SetupState : TurnBasedState
{
    //can technically play with more players but 2 is default
    [SerializeField] int _numPlayers = 2;
    [SerializeField] public GameObject squareField;

    bool _activated = false;
    static int[,] boardArray;

    public static int[,] Board
    {
        get => boardArray; 
    }

    public static GameObject boardObjectParent;
    public override void Enter()
    {
        Debug.Log("Setup: ... Entering");
        Debug.Log("Creating " + _numPlayers + " players");


        boardObjectParent = GameObject.Find("Board");
        if (boardObjectParent != null)
        {
            DestroyImmediate(boardObjectParent);
        }
        boardObjectParent = new GameObject("Board");

        // create an empty field and instantiate the cells
        boardArray = new int[8, 7];
        for (int x = 0; x < 8; x++)
        {
            for (int y = 0; y < 7; y++)
            {
                boardArray[x, y] = (int)PieceTypes.Piece.Empty;
                GameObject g = Instantiate(squareField, new Vector3(x, y * -1, -1), Quaternion.identity) as GameObject;
                g.transform.parent = boardObjectParent.transform;
            }
        }

        // center camera
        Camera.main.transform.position = new Vector3(
            (8 - 1) / 2.0f, -((7 - 1) / 2.0f), Camera.main.transform.position.z);


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
