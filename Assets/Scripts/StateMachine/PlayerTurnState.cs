using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerTurnState : TurnBasedState
{
    [SerializeField] Text _playerTurnTextUI = null;

    [SerializeField] GameObject _playerPiece;

    int _playerTurnCount = 0;

    int _count = 0;
    bool turnComplete = false;

    GameObject tempPiece = null;

    public override void Enter()
    {
        turnComplete = false;
        Debug.Log("Player Turn: ...Entering");
        _playerTurnTextUI.gameObject.SetActive(true);


        _playerTurnCount++;
        _playerTurnTextUI.text = "Player turn: " + _playerTurnCount.ToString();


        StateMachine.Input.PressedWin += OnPressedWin;
        StateMachine.Input.PressedLose += OnPressedLose;

        if (tempPiece == null)
        {
            tempPiece = Spawner();
        }
    }

    public override void Tick()
    {
        // update the objects position
        Vector3 pos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        tempPiece.transform.position = new Vector3(Mathf.Clamp(pos.x, 0f, 8 - 1f), 1, 0);

        // click the left mouse button to drop the piece into the selected column
        if (Input.GetMouseButtonDown(0))
        {
            StartCoroutine(TakeMove(tempPiece));
        }

        if(turnComplete)
        {
            StateMachine.ChangeState<EnemyTurnState>();
        }
    }

    public override void Exit()
    {
        _playerTurnTextUI.gameObject.SetActive(false);


        StateMachine.Input.PressedWin -= OnPressedWin;
        StateMachine.Input.PressedLose -= OnPressedLose;
        Debug.Log("Player Turn: Exiting...");

    }

    void OnPressedWin()
    {
        StateMachine.ChangeState<WinState>();
    }

    void OnPressedLose()
    {
        StateMachine.ChangeState<LoseState>();
    }




    GameObject Spawner()
    {
        Vector3 spawnPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);

        GameObject g = Instantiate(
                _playerPiece, // is players turn = spawn blue, else spawn red
                new Vector3(
                Mathf.Clamp(spawnPos.x, 0, 8 - 1),
                1, 0), // spawn it above the first row
                Quaternion.identity) as GameObject;

        return g;
    }

    IEnumerator TakeMove(GameObject pieceMoving)
    {
        Vector3 startPosition = pieceMoving.transform.position;
        Vector3 endPosition = new Vector3();

        // round to a grid cell
        int x = Mathf.RoundToInt(startPosition.x);
        startPosition = new Vector3(x, startPosition.y, startPosition.z);

        // is there a free cell in the selected column?
        bool foundFreeCell = false;
        for (int i = 7 - 1; i >= 0; i--)
        {
            if (SetupState.Board[x, i] == 0)
            {
                foundFreeCell = true;
                SetupState.Board[x, i] = (int)PieceTypes.Piece.Player;
                endPosition = new Vector3(x, i * -1, startPosition.z);

                break;
            }
        }

        if (foundFreeCell)
        {
            turnComplete = true;
            // Instantiate a new Piece, disable the temporary
            GameObject g = Instantiate(pieceMoving) as GameObject;
            tempPiece.GetComponent<Renderer>().enabled = false;

            float distance = Vector3.Distance(startPosition, endPosition);

            float t = 0;
            while (t < 1)
            {
                t += Time.deltaTime * 2 * ((7 - distance) + 1);

                g.transform.position = Vector3.Lerp(startPosition, endPosition, t);
                yield return null;
            }

            g.transform.parent = SetupState.boardObjectParent.transform;

            // remove the temporary gameobject
            DestroyImmediate(tempPiece);

            bool win = false;
            win = CheckWin(1, SetupState.Board);
            if(win)
            {
                OnPressedWin();
            }
            yield return 0;
        }
    }

    bool CheckWin(int playerVal, int[,] arr)
    {
        //Horizontal win check
        for (int i = 0; i < 8 - 3; i++)
        {
            for (int j = 0; j < 7; j++)
            {
                if (arr[i,j] == playerVal && arr[i + 1,j] == playerVal && arr[i + 2,j] == playerVal && arr[i + 3,j] == playerVal)
                {
                    return true;
                }
            }
        }

        //Vertical win check
        for (int i = 0; i < 8; i++)
        {
            for (int j = 0; j < 7 - 3; j++)
            {
                if (arr[i, j] == playerVal && arr[i, j + 1] == playerVal && arr[i, j + 2] == playerVal && arr[i, j + 3] == playerVal)
                {
                    return true;
                }
            }
        }

        //Diagonal / win check
        for (int i = 3; i < 8; i++)
        {
            for (int j = 0; j < 7 - 3; j++)
            {
                if (arr[i, j] == playerVal && arr[i- 1, j + 1] == playerVal && arr[i - 2, j + 2] == playerVal && arr[i - 3, j + 3] == playerVal)
                {
                    return true;
                }
            }
        }

        //Diagonal \ win check
        // descendingDiagonalCheck
        for (int i = 3; i < 8; i++)
        {
            for (int j = 3; j < 7; j++)
            {
                if (arr[i, j] == playerVal && arr[i - 1,j - 1] == playerVal && arr[i - 2,j - 2] == playerVal && arr[i - 3,j - 3] == playerVal)
                    return true;
            }
        }


        return false;
    }
}
