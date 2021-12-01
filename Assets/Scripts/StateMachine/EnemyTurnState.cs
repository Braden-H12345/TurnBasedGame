using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class EnemyTurnState : TurnBasedState
{
    public static event Action EnemyTurnBegan;
    public static event Action EnemyTurnEnded;

    [SerializeField] float _pauseDuration = 1.5f;
    [SerializeField] GameObject _enemyPiece;
    [SerializeField] AudioClip _pieceDropSound;

    GameObject _tempPiece = null;

    public override void Enter()
    {
        EnemyTurnBegan?.Invoke();

        StateMachine.Input.PressedLose += OnPressedLose;


        if (_tempPiece == null)
        {
            _tempPiece = Spawner();
        }

        StartCoroutine(EnemyThinkingRoutine(_pauseDuration));
    }

    public override void Exit()
    {
        StateMachine.Input.PressedLose -= OnPressedLose;
    }

    IEnumerator EnemyThinkingRoutine(float pauseDuration)
    {
        yield return new WaitForSeconds(pauseDuration);



        StartCoroutine(TakeMove(_tempPiece));
        EnemyTurnEnded?.Invoke();
        StateMachine.ChangeState<PlayerTurnState>();
    }

    GameObject Spawner()
    {
        Vector3 _spawnPos = new Vector3(0,0,0);

        int column = Evaluate(SetupState.Board);

        if(column == -1)
        {
            StateMachine.ChangeState<LoseState>();
        }
        else
        {
            _spawnPos = new Vector3(column, 0, 0);
        }

        GameObject _tempObj = Instantiate(
                _enemyPiece,
                new Vector3(
                Mathf.Clamp(_spawnPos.x, 0, 8 - 1),
                1, 0), Quaternion.identity);

        return _tempObj;
    }

    IEnumerator TakeMove(GameObject _pieceToMove)
    {
        Vector3 _startPosition = _pieceToMove.transform.position;
        Vector3 _endPosition = new Vector3();

        int column = Mathf.RoundToInt(_startPosition.x);

        _startPosition = new Vector3(column, _startPosition.y, _startPosition.z);


            //gets to the lowest free cell in that column
             for (int row = 7 - 1; row >= 0; row--)
            {
                if (SetupState.Board[column, row] == 0)
                {
                    SetupState.Board[column, row] = (int)PieceTypes.Piece.AI;
                    _endPosition = new Vector3(column, row * -1, _startPosition.z);
                    break;
                }
            }


            Feedback();


            GameObject _piece = Instantiate(_pieceToMove);
            _tempPiece.GetComponent<Renderer>().enabled = false;

            float _dropDistance = Vector3.Distance(_startPosition, _endPosition);

            float _dropTime = 0;
            while (_dropTime < 1)
            {
                _dropTime += Time.deltaTime * 2 * ((7 - _dropDistance) + 1);

                _piece.transform.position = Vector3.Lerp(_startPosition, _endPosition, _dropTime);
                yield return null;
            }

            _piece.transform.parent = SetupState._boardObjectParent.transform;

            // remove temp piece
            DestroyImmediate(_tempPiece);

            bool win = CheckWin(2, SetupState.Board);
            if (win)
            {
                OnPressedLose();
            }
            yield return 0;
        
    }

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

    bool CheckWin(int playerVal, int[,] arr)
    {
        //Horizontal win check
        for (int i = 0; i < 8 - 3; i++)
        {
            for (int j = 0; j < 7; j++)
            {
                if (arr[i, j] == playerVal && arr[i + 1, j] == playerVal && arr[i + 2, j] == playerVal && arr[i + 3, j] == playerVal)
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
                if (arr[i, j] == playerVal && arr[i - 1, j + 1] == playerVal && arr[i - 2, j + 2] == playerVal && arr[i - 3, j + 3] == playerVal)
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
                if (arr[i, j] == playerVal && arr[i - 1, j - 1] == playerVal && arr[i - 2, j - 2] == playerVal && arr[i - 3, j - 3] == playerVal)
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



    //this function acts as a priority system of moves
    //it navigates through multiple if statements to determine which move to take
    //the if statements are in the following order (simulating priority)
    // Win -> Block win -> Connect 3 pieces - > block horizontal connection of 3 (fringe case)-> Random move if none of those
    public int Evaluate(int[,] board)
    {
        int x = 0;
        bool iterator = true;
        while (iterator)
        {
            bool canWin = false;
            bool canBlockWin = false;
            bool canConnectThree = false;
            bool blockThreeHorizontal = false;

            //this checks if the AI can win on this turn.
            //if it can it returns that column number so that it can play it
            //otherwise it continues to the next check
            if (canWin == false)
            {
                //Horizantal win check
                for (int i = 0; i < 8 - 3; i++)
                {
                    for (int j = 0; j < 7; j++)
                    {
                        if ((board[i, j] == 2 && board[i + 1, j] == 2 && board[i + 2, j] == 2 && board[i + 3, j] == 0) ||
                            (board[i, j] == 2 && board[i + 1, j] == 2 && board[i + 2, j] == 0 && board[i + 3, j] == 2) ||
                            (board[i, j] == 2 && board[i + 1, j] == 0 && board[i + 2, j] == 2 && board[i + 3, j] == 2) ||
                            (board[i, j] == 0 && board[i + 1, j] == 2 && board[i + 2, j] == 2 && board[i + 3, j] == 2))
                        {
                            if ((j == 6 && board[i, j] == 2 && board[i + 1, j] == 2 && board[i + 2, j] == 2 && board[i + 3, j] == 0)
                                || (board[i, j] == 2 && board[i + 1, j] == 2 && board[i + 2, j] == 2 && board[i + 3, j] == 0 && board[i + 3, j + 1] != 0))
                            {
                                x = i + 3;
                                return x;
                            }
                            else if ((j == 6 && board[i, j] == 0 && board[i + 1, j] == 2 && board[i + 2, j] == 2 && board[i + 3, j] == 2)
                                || (board[i, j] == 0 && board[i + 1, j] == 2 && board[i + 2, j] == 2 && board[i + 3, j] == 2 && board[i, j + 1] != 0))
                            {
                                x = i;
                                return x;

                            }
                            else if ((j == 6 && board[i, j] == 2 && board[i + 1, j] == 2 && board[i + 2, j] == 0 && board[i + 3, j] == 2)
                                || (board[i, j] == 2 && board[i + 1, j] == 2 && board[i + 2, j] == 0 && board[i + 3, j] == 2 && board[i + 2, j + 1] != 0))
                            {
                                x = i + 2;
                                return x;
                            }
                            else if ((j == 6 && board[i, j] == 2 && board[i + 1, j] == 0 && board[i + 2, j] == 2 && board[i + 3, j] == 2)
                                || (board[i, j] == 2 && board[i + 1, j] == 0 && board[i + 2, j] == 2 && board[i + 3, j] == 2 && board[i + 1, j + 1] != 0))
                            {
                                x = i + 1;
                                return x;
                            }
                        }
                    }
                }

                //Vertical win check
                for (int i = 0; i < 8; i++)
                {
                    for (int j = 0; j < 7 - 3; j++)
                    {
                        if (board[i, j] == 0 && board[i, j + 1] == 2 && board[i, j + 2] == 2 && board[i, j + 3] == 2)
                        {
                            x = i;
                            return x;
                        }
                    }
                }

                //Diagonal / win check
                for (int i = 3; i < 8; i++)
                {
                    for (int j = 0; j < 7 - 3; j++)
                    {
                        if ((board[i, j] == 2 && board[i - 1, j + 1] == 2 && board[i - 2, j + 2] == 2 && board[i - 3, j + 3] == 0) ||
                            (board[i, j] == 0 && board[i - 1, j + 1] == 2 && board[i - 2, j + 2] == 2 && board[i - 3, j + 3] == 2) ||
                            (board[i, j] == 2 && board[i - 1, j + 1] == 0 && board[i - 2, j + 2] == 2 && board[i - 3, j + 3] == 2) ||
                            (board[i, j] == 2 && board[i - 1, j + 1] == 2 && board[i - 2, j + 2] == 0 && board[i - 3, j + 3] == 2))
                        {
                            if ((j == 3 && board[i, j] == 2 && board[i - 1, j + 1] == 2 && board[i - 2, j + 2] == 2 && board[i - 3, j + 3] == 0)
                                || (board[i, j] == 2 && board[i - 1, j + 1] == 2 && board[i - 2, j + 2] == 2 && board[i - 3, j + 3] == 0 && board[i - 3, j + 4] != 0))
                            {
                                x = i - 3;
                                return x;
                            }
                            else if ((board[i, j] == 2 && board[i - 1, j + 1] == 2 && board[i - 2, j + 2] == 0 && board[i - 3, j + 3] == 2 && board[i - 2, j + 3] != 0))
                            {
                                x = i - 2;
                                return x;
                            }
                            else if ((board[i, j] == 2 && board[i - 1, j + 1] == 0 && board[i - 2, j + 2] == 2 && board[i - 3, j + 3] == 2 && board[i - 1, j + 2] != 0))
                            {
                                x = i - 1;
                                return x;
                            }
                            else if ((board[i, j] == 0 && board[i - 1, j + 1] == 2 && board[i - 2, j + 2] == 2 && board[i - 3, j + 3] == 2 && board[i, j + 1] != 0))
                            {
                                x = i;
                                return x;
                            }
                        }
                    }
                }

                //Diagonal \ win check
                for (int i = 3; i < 8; i++)
                {
                    for (int j = 3; j < 7; j++)
                    {
                        if ((board[i, j] == 2 && board[i - 1, j - 1] == 2 && board[i - 2, j - 2] == 2 && board[i - 3, j - 3] == 0) ||
                            (board[i, j] == 0 && board[i - 1, j - 1] == 2 && board[i - 2, j - 2] == 2 && board[i - 3, j - 3] == 2) ||
                            (board[i, j] == 2 && board[i - 1, j - 1] == 0 && board[i - 2, j - 2] == 2 && board[i - 3, j - 3] == 2) ||
                            (board[i, j] == 2 && board[i - 1, j - 1] == 2 && board[i - 2, j - 2] == 0 && board[i - 3, j - 3] == 2))
                        {
                            if ((board[i, j] == 2 && board[i - 1, j - 1] == 2 && board[i - 2, j - 2] == 2 && board[i - 3, j - 3] == 0 && board[i - 3, j - 2] != 0))
                            {
                                x = i - 3;
                                return x;
                            }
                            else if ((j == 6 && board[i, j] == 0 && board[i - 1, j - 1] == 2 && board[i - 2, j - 2] == 2 && board[i - 3, j - 3] == 2) || (board[i, j] == 0 && board[i - 1, j - 1] == 2 && board[i - 2, j - 2] == 2 && board[i - 3, j - 3] == 2))
                            {
                                return i;
                            }
                            else if ((board[i, j] == 2 && board[i - 1, j - 1] == 0 && board[i - 2, j - 2] == 2 && board[i - 3, j - 3] == 2 && board[i - 3, j] != 0))
                            {
                                x = i - 1;
                                return x;
                            }
                            else if ((board[i, j] == 2 && board[i - 1, j - 1] == 2 && board[i - 2, j - 2] == 0 && board[i - 3, j - 3] == 2 && board[i - 3, j - 1] != 0))
                            {
                                x = i - 2;
                                return x;
                            }
                        }
                    }
                }
                canWin = true;
            }


            //this checks if the AI can block the player from winning on this turn.
            //if it can it returns that column number so that it can play it
            //otherwise it continues to the next check
            if (canBlockWin == false)
            {
                //Horizantal block check
                for (int i = 0; i < 8 - 3; i++)
                {
                    for (int j = 0; j < 7; j++)
                    {

                        if ((board[i, j] == 1 && board[i + 1, j] == 1 && board[i + 2, j] == 1 && board[i + 3, j] == 0) ||
                            (board[i, j] == 1 && board[i + 1, j] == 1 && board[i + 2, j] == 0 && board[i + 3, j] == 1) ||
                            (board[i, j] == 1 && board[i + 1, j] == 0 && board[i + 2, j] == 1 && board[i + 3, j] == 1) ||
                            (board[i, j] == 0 && board[i + 1, j] == 1 && board[i + 2, j] == 1 && board[i + 3, j] == 1))
                        {
                            if ((j == 6 && board[i, j] == 1 && board[i + 1, j] == 1 && board[i + 2, j] == 1 && board[i + 3, j] == 0)
                                || (board[i, j] == 1 && board[i + 1, j] == 1 && board[i + 2, j] == 1 && board[i + 3, j] == 0 && board[i + 3, j + 1] != 0))
                            {
                                x = i + 3;
                                return x;
                            }
                            else if ((j == 6 && board[i, j] == 0 && board[i + 1, j] == 1 && board[i + 2, j] == 1 && board[i + 3, j] == 1)
                                || (board[i, j] == 0 && board[i + 1, j] == 1 && board[i + 2, j] == 1 && board[i + 3, j] == 1 && board[i, j + 1] != 0))
                            {
                                x = i;
                                return x;

                            }
                            else if ((j == 6 && board[i, j] == 1 && board[i + 1, j] == 1 && board[i + 2, j] == 0 && board[i + 3, j] == 1)
                                || (board[i, j] == 1 && board[i + 1, j] == 1 && board[i + 2, j] == 0 && board[i + 3, j] == 1 && board[i + 2, j + 1] != 0))
                            {
                                x = i + 2;
                                return x;
                            }
                            else if ((j == 6 && board[i, j] == 1 && board[i + 1, j] == 0 && board[i + 2, j] == 1 && board[i + 3, j] == 1)
                                || (board[i, j] == 1 && board[i + 1, j] == 0 && board[i + 2, j] == 1 && board[i + 3, j] == 1 && board[i + 1, j + 1] != 0))
                            {
                                x = i + 1;
                                return x;
                            }
                        }
                    }
                }

                //Vertical block check
                for (int i = 0; i < 8; i++)
                {
                    for (int j = 0; j < 7 - 3; j++)
                    {
                        if ((board[i, j] == 0 && board[i, j + 1] == 1 && board[i, j + 2] == 1 && board[i, j + 3] == 1))
                        {
                            
                            x = i;
                            return x;
                        }
                    }
                }

                //Diagonal / block check
                for (int i = 3; i < 8; i++)
                {
                    for (int j = 0; j < 7 - 3; j++)
                    {
                        if ((board[i, j] == 1 && board[i - 1, j + 1] == 1 && board[i - 2, j + 2] == 1 && board[i - 3, j + 3] == 0) ||
                            (board[i, j] == 0 && board[i - 1, j + 1] == 1 && board[i - 2, j + 2] == 1 && board[i - 3, j + 3] == 1) ||
                            (board[i, j] == 1 && board[i - 1, j + 1] == 0 && board[i - 2, j + 2] == 1 && board[i - 3, j + 3] == 1) ||
                            (board[i, j] == 1 && board[i - 1, j + 1] == 1 && board[i - 2, j + 2] == 0 && board[i - 3, j + 3] == 1))
                        {
                            if ((j == 3 && board[i, j] == 1 && board[i - 1, j + 1] == 1 && board[i - 2, j + 2] == 1 && board[i - 3, j + 3] == 0)
                                || (board[i, j] == 1 && board[i - 1, j + 1] == 1 && board[i - 2, j + 2] == 1 && board[i - 3, j + 3] == 0 && board[i - 3, j + 4] != 0))
                            {
                                x = i - 3;
                                return x;
                            }
                            else if ((board[i, j] == 1 && board[i - 1, j + 1] == 1 && board[i - 2, j + 2] == 0 && board[i - 3, j + 3] == 1 && board[i - 2, j + 3] != 0))
                            {
                                x = i - 2;
                                return x;
                            }
                            else if ((board[i, j] == 1 && board[i - 1, j + 1] == 0 && board[i - 2, j + 2] == 1 && board[i - 3, j + 3] == 1 && board[i - 1, j + 2] != 0))
                            {
                                x = i - 1;
                                return x;
                            }
                            else if ((board[i, j] == 0 && board[i - 1, j + 1] == 1 && board[i - 2, j + 2] == 1 && board[i - 3, j + 3] == 1 && board[i, j + 1] != 0))
                            {
                                x = i;
                                return x;
                            }
                        }
                    }
                }

                //Diagonal \ block check
                for (int i = 3; i < 8; i++)
                {
                    for (int j = 3; j < 7; j++)
                    {
                        if ((board[i, j] == 1 && board[i - 1, j - 1] == 1 && board[i - 2, j - 2] == 1 && board[i - 3, j - 3] == 0) ||
                            (board[i, j] == 0 && board[i - 1, j - 1] == 1 && board[i - 2, j - 2] == 1 && board[i - 3, j - 3] == 1) ||
                            (board[i, j] == 1 && board[i - 1, j - 1] == 0 && board[i - 2, j - 2] == 1 && board[i - 3, j - 3] == 1) ||
                            (board[i, j] == 1 && board[i - 1, j - 1] == 1 && board[i - 2, j - 2] == 0 && board[i - 3, j - 3] == 1))
                        {
                            if ((board[i, j] == 1 && board[i - 1, j - 1] == 1 && board[i - 2, j - 2] == 1 && board[i - 3, j - 3] == 0 && board[i - 3, j - 2] != 0))
                            {
                                x = i - 3;
                                return x;
                            }
                            else if ((j == 6 && board[i, j] == 0 && board[i - 1, j - 1] == 1 && board[i - 2, j - 2] == 1 && board[i - 3, j - 3] == 1)
                                || (board[i, j] == 0 && board[i - 1, j - 1] == 1 && board[i - 2, j - 2] == 1 && board[i - 3, j - 3] == 1 && board[i, j + 1] != 0))
                            {
                                return i;
                            }
                            else if ((board[i, j] == 1 && board[i - 1, j - 1] == 0 && board[i - 2, j - 2] == 1 && board[i - 3, j - 3] == 1 && board[i - 3, j] != 0))
                            {
                                x = i - 1;
                                return x;
                            }
                            else if ((board[i, j] == 1 && board[i - 1, j - 1] == 1 && board[i - 2, j - 2] == 0 && board[i - 3, j - 3] == 1 && board[i - 3, j - 1] != 0))
                            {
                                x = i - 2;
                                return x;
                            }
                        }

                    }
                }
                canBlockWin = true;
            }


            //this is a fringe check but simply is there to not allow the player to instantly win in the first few moves
            //simply to allow for the bot to not instantly lose in the event the player manages to get 3 in a row on first column at start
            if (blockThreeHorizontal == false)
            {
                for (int i = 0; i < 8 - 2; i++)
                {
                    for (int j = 0; j < 7; j++)
                    {
                        if ((board[i, j] == 0 && board[i + 1, j] == 1 && board[i + 2, j] == 1) ||
                            (board[i, j] == 1 && board[i + 1, j] == 1 && board[i + 2, j] == 0) ||
                            (board[i, j] == 1 && board[i + 1, j] == 0 && board[i + 2, j] == 1))
                        {
                            if ((j == 6 && board[i, j] == 0 && board[i + 1, j] == 1 && board[i + 2, j] == 1)
                                || (board[i, j] == 0 && board[i + 1, j] == 1 && board[i + 2, j] == 1 && board[i, j + 1] != 0))
                            {

                                x = i;
                                return x;
                            }
                            else if ((j == 6 && board[i, j] == 1 && board[i + 1, j] == 0 && board[i + 2, j] == 1)
                                || (board[i, j] == 1 && board[i + 1, j] == 0 && board[i + 2, j] == 1 && board[i + 1, j + 1] != 0))
                            {

                                x = i + 1;
                                return x;
                            }
                            else if ((j == 6 && board[i, j] == 1 && board[i + 1, j] == 1 && board[i + 2, j] == 0)
                                || (board[i, j] == 1 && board[i + 1, j] == 2 && board[i + 2, j] == 0 && board[i + 2, j + 1] != 0))
                            {

                                x = i + 2;
                                return x;
                            }
                        }
                    }
                }
                blockThreeHorizontal = true;
            }


            //this checks if the AI can connect 3 pieces on this turn.
            //if it can it returns that column number so that it can play it
            //otherwise it continues to the next check
            if (canConnectThree == false)
            {
                

                //Vertical connect check
                for (int i = 0; i < 8; i++)
                {
                    for (int j = 0; j < 7 - 2; j++)
                    {
                        if (board[i, j] == 0 && board[i, j + 1] == 2 && board[i, j + 2] == 2)
                        {
                            
                            x = i;
                            return x;
                        }
                    }
                }

                //Diagonal / connect check
                for (int i = 2; i < 8; i++)
                {
                    for (int j = 0; j < 7 - 2; j++)
                    {
                        if ((board[i, j] == 0 && board[i - 1, j + 1] == 2 && board[i - 2, j + 2] == 2) ||
                            (board[i, j] == 2 && board[i - 1, j + 1] == 0 && board[i - 2, j + 2] == 2)||
                            (board[i, j] == 2 && board[i - 1, j + 1] == 2 && board[i - 2, j + 2] == 0))
                        {
                            if((j == 4 && board[i, j] == 2 && board[i - 1, j + 1] == 2 && board[i - 2, j + 2] == 0) 
                                || (board[i, j] == 2 && board[i - 1, j + 1] == 2 && board[i - 2, j + 2] == 0 && board[i - 2, j + 3] != 0))
                            {
                                
                                x = i - 2;
                                return x;
                            }
                            else if((board[i, j] == 2 && board[i - 1, j + 1] == 0 && board[i - 2, j + 2] == 2 && board[i - 1, j + 2] != 0))
                            {
                                
                                x = i - 1;
                                return x;
                            }
                            else if ((board[i, j] == 0 && board[i - 1, j + 1] == 2 && board[i - 2, j + 2] == 2 && board[i, j + 1] != 0))
                            {
                                
                                x = i;
                                return x;
                            }
                        }
                    }
                }

                //Diagonal \ connect check
                for (int i = 2; i < 8; i++)
                {
                    for (int j = 2; j < 7; j++)
                    {
                        if ((board[i, j] == 0 && board[i - 1, j - 1] == 2 && board[i - 2, j - 2] == 2) ||
                            (board[i, j] == 2 && board[i - 1, j - 1] == 0 && board[i - 2, j - 2] == 2) ||
                            (board[i, j] == 2 && board[i - 1, j - 1] == 2 && board[i - 2, j - 2] == 0))
                        {
                            if((j == 6 && board[i, j] == 0 && board[i - 1, j - 1] == 2 && board[i - 2, j - 2] == 2) 
                                || (board[i, j] == 0 && board[i - 1, j - 1] == 2 && board[i - 2, j - 2] == 2 && board[i, j +1] != 0))
                            {
                                
                                x = i;
                                return x;
                            }
                            else if((board[i, j] == 2 && board[i - 1, j - 1] == 0 && board[i - 2, j - 2] == 2 && board[i-1, j] != 0))
                            {
                                
                                x = i - 1;
                                return x;
                            }
                            else if((board[i, j] == 2 && board[i - 1, j - 1] == 2 && board[i - 2, j - 2] == 0 && board[i-2, j-1] != 0))
                            {
                                
                                x = i - 2;
                                return x;
                            }
                        }
                    }
                }

                //Horizantal connect check, this at end as there are some cases where connecting can be detrimental 
                //yet it will anyways (such as on edge of board when 4 is not possible)
                //this ensures that more cases are checked first
                for (int i = 0; i < 8 - 2; i++)
                {
                    for (int j = 0; j < 7; j++)
                    {
                        if ((board[i, j] == 0 && board[i + 1, j] == 2 && board[i + 2, j] == 2) ||
                            (board[i, j] == 2 && board[i + 1, j] == 2 && board[i + 2, j] == 0) ||
                            (board[i, j] == 2 && board[i + 1, j] == 0 && board[i + 2, j] == 2))
                        {
                            if ((j == 6 && board[i, j] == 0 && board[i + 1, j] == 2 && board[i + 2, j] == 2)
                                || (board[i, j] == 0 && board[i + 1, j] == 2 && board[i + 2, j] == 2 && board[i, j + 1] != 0))
                            {
                                
                                x = i;
                                return x;
                            }
                            else if ((j == 6 && board[i, j] == 2 && board[i + 1, j] == 0 && board[i + 2, j] == 2)
                                || (board[i, j] == 2 && board[i + 1, j] == 0 && board[i + 2, j] == 2 && board[i + 1, j + 1] != 0))
                            {
                                
                                x = i + 1;
                                return x;
                            }
                            else if ((j == 6 && board[i, j] == 2 && board[i + 1, j] == 2 && board[i + 2, j] == 0)
                                || (board[i, j] == 2 && board[i + 1, j] == 2 && board[i + 2, j] == 0 && board[i + 2, j + 1] != 0))
                            {
                                
                                x = i + 2;
                                return x;
                            }
                        }
                    }
                }
                canConnectThree = true;
            }

            //after checking everything it will play randomly if no other prioritized moves are valid
            if (canBlockWin && canWin && canConnectThree && blockThreeHorizontal)
            {
                List<int> moves = CalculateMoves();
                if (moves.Count > 0)
                {
                    x = moves[UnityEngine.Random.Range(0, moves.Count)];
                    return x;
                }
                else
                {
                    //draw case
                    return -1;
                }
                
            }
            iterator = false;
        }
        return x;
    }


    void OnPressedLose()
    {
        StateMachine.ChangeState<LoseState>();
    }
}
