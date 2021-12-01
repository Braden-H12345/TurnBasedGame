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


    GameObject tempPiece = null;

    public override void Enter()
    {
        EnemyTurnBegan?.Invoke();

        StateMachine.Input.PressedLose += OnPressedLose;


        if (tempPiece == null)
        {
            tempPiece = Spawner();
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



        StartCoroutine(TakeMove(tempPiece));
        EnemyTurnEnded?.Invoke();
        StateMachine.ChangeState<PlayerTurnState>();
    }

    GameObject Spawner()
    {
        Vector3 spawnPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);


        int column = Evaluate(SetupState.Board);

        if(column == -1)
        {
            StateMachine.ChangeState<LoseState>();
        }
        else
        {
            spawnPos = new Vector3(column, 0, 0);
        }

        GameObject g = Instantiate(
                _enemyPiece, 
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

        int x = Mathf.RoundToInt(startPosition.x);

        // round to a grid cell
        startPosition = new Vector3(x, startPosition.y, startPosition.z);

        // is there a free cell in the selected column?
        bool foundFreeCell = false;
        for (int i = 7 - 1; i >= 0; i--)
        {
            if (SetupState.Board[x, i] == 0)
            {
                foundFreeCell = true;
                SetupState.Board[x, i] = (int)PieceTypes.Piece.AI;
                endPosition = new Vector3(x, i * -1, startPosition.z);

                break;
            }
        }

        if (foundFreeCell)
        {
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
            win = CheckWin(2, SetupState.Board);
            if (win)
            {
                OnPressedLose();
            }
            yield return 0;
        }
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
                                Debug.Log("Blocked diag 4");
                                x = i - 3;
                                return x;
                            }
                            else if ((board[i, j] == 1 && board[i - 1, j + 1] == 1 && board[i - 2, j + 2] == 0 && board[i - 3, j + 3] == 1 && board[i - 2, j + 3] != 0))
                            {
                                Debug.Log("Blocked diag 3");
                                x = i - 2;
                                return x;
                            }
                            else if ((board[i, j] == 1 && board[i - 1, j + 1] == 0 && board[i - 2, j + 2] == 1 && board[i - 3, j + 3] == 1 && board[i - 1, j + 2] != 0))
                            {
                                Debug.Log("Blocked diag 2");
                                x = i - 1;
                                return x;
                            }
                            else if ((board[i, j] == 0 && board[i - 1, j + 1] == 1 && board[i - 2, j + 2] == 1 && board[i - 3, j + 3] == 1 && board[i, j + 1] != 0))
                            {
                                Debug.Log("Blocked diag 1");
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
                            else if ((j == 7 && board[i, j] == 0 && board[i - 1, j - 1] == 1 && board[i - 2, j - 2] == 1 && board[i - 3, j - 3] == 1)
                                || (board[i, j] == 0 && board[i - 1, j - 1] == 1 && board[i - 2, j - 2] == 1 && board[i - 3, j - 3] == 1))
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
