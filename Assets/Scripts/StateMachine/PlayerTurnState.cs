using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerTurnState : TurnBasedState
{

    [SerializeField] GameObject _playerPiece;
    [SerializeField] AudioClip _pieceDropSound;

    bool turnComplete = false;

    GameObject tempPiece = null;

    public override void Enter()
    {
        turnComplete = false;
        StateMachine.Input.PressedWin += OnPressedWin;
        StateMachine.Input.PressedLose += OnPressedLose;

        if (tempPiece == null)
        {
            tempPiece = Spawner();
        }
    }

    public override void Tick()
    {
        List<int> moves = CalculateMoves();

        //draw checking
        if (moves.Count == 0)
        {
            StateMachine.ChangeState<LoseState>();
        }

        
        if (Time.timeScale == 1)
        {
            Vector3 pos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            tempPiece.transform.position = new Vector3(Mathf.Clamp(pos.x, 0f, 8 - 1f), 1, 0);

            
            if (Input.GetMouseButtonDown(0))
            {
                Feedback();
                StartCoroutine(TakeMove(tempPiece));
            }

            if (turnComplete)
            {
                StateMachine.ChangeState<EnemyTurnState>();
            }
        }
    }

    public override void Exit()
    {


        StateMachine.Input.PressedWin -= OnPressedWin;
        StateMachine.Input.PressedLose -= OnPressedLose;

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
        Vector3 _spawnPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);

        GameObject _piece = Instantiate( _playerPiece, new Vector3
            (Mathf.Clamp(_spawnPos.x, 0, 8 - 1),1, 0),Quaternion.identity);

        return _piece;
    }

    IEnumerator TakeMove(GameObject _pieceToUse)
    {
        Vector3 _startPosition = _pieceToUse.transform.position;
        Vector3 _endPosition = new Vector3();

        // round to a grid cell
        int x = Mathf.RoundToInt(_startPosition.x);
        _startPosition = new Vector3(x, _startPosition.y, _startPosition.z);

        bool _isColumnLegal = false;

        //gets first empty space on the column. making sure it is legal before attempting to complete move
        for (int i = 7 - 1; i >= 0; i--)
        {
            if (SetupState.Board[x, i] == 0)
            {
                _isColumnLegal = true;
                SetupState.Board[x, i] = (int)PieceTypes.Piece.Player;
                _endPosition = new Vector3(x, i * -1, _startPosition.z);

                break;
            }
        }

        if (_isColumnLegal)
        {
            turnComplete = true;


            GameObject _pieceBeingPlayed = Instantiate(_pieceToUse);
            tempPiece.GetComponent<Renderer>().enabled = false;

            float distance = Vector3.Distance(_startPosition, _endPosition);

            float _dropTime = 0;
            while (_dropTime < 1)
            {
                _dropTime += Time.deltaTime * 2 * ((7 - distance) + 1);

                _pieceBeingPlayed.transform.position = Vector3.Lerp(_startPosition, _endPosition, _dropTime);
                yield return null;
            }

            _pieceBeingPlayed.transform.parent = SetupState._boardObjectParent.transform;

            DestroyImmediate(tempPiece);

            bool win = CheckWin(1, SetupState.Board);
            if(win)
            {
                OnPressedWin();
            }
            yield return 0;
        }
    }

    //checks if the player has won
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

    private void Feedback()
    {

        if (_pieceDropSound != null)
        {
            AudioHelper.PlayClip2D(_pieceDropSound, 1f);
        }
    }

    //used to determine if a draw must be given. if it finds no moves then the game will draw (player loses)
    public List<int> CalculateMoves()
    {
        List<int> possibleMoves = new List<int>();
        for (int x = 0; x < 8; x++)
        {
            for (int y = 7 - 1; y >= 0; y--)
            {
                if (SetupState.Board[x, y] == (int)PieceTypes.Piece.Empty)
                {
                    possibleMoves.Add(x);
                    break;
                }
            }
        }
        return possibleMoves;
    }
}
