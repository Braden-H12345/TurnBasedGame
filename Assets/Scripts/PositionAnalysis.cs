/*using System.Collections;
using System.Collections.Generic;
using UnityEngine;


//This is a class that has all the functionality to analyize the board and choose the best column to play
public class PositionAnalysis : MonoBehaviour
{
    /*private int[,] _analysisBoard;

    public int[,] GetAnalysisBoard()
    {
        return _analysisBoard;
    }

    public void SetAnalysisBoard(int [,] temp)
    {
        this._analysisBoard = temp;
    }*/
    //dont think i need this ^^
    /*public int Evaluate(int[,] board )
    {
        int x = 0;
        bool iterator = true;
        while(iterator)
        {
            bool canWin = false;
            bool canBlockWin = false;
            bool canConnectThree = false;
            bool canConnectTwo = false;

            if(canWin == false)
            {
                //Horizantal win check
                for (int i = 0; i < 8 - 3; i++)
                {
                    for (int j = 0; j < 7; j++)
                    {
                        if (board[i, j] == 2 && board[i + 1, j] == 2 && board[i + 2, j] == 2 && board[i + 3, j] == 0 && board[i +3, j -1] != 0)
                        {
                            x = i + 3;
                            return x;
                        }
                    }
                }

                //Vertical win check
                for (int i = 0; i < 8; i++)
                {
                    for (int j = 0; j < 7 - 3; j++)
                    {
                        if (board[i, j] == 2 && board[i, j + 1] == 2 && board[i, j + 2] == 2 && board[i, j + 3] == 0)
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
                        if (board[i, j] == 2 && board[i - 1, j + 1] == 2 && board[i - 2, j + 2] == 2 && board[i - 3, j + 3] == 0 && board[i - 3, j + 2] != 0)
                        {
                            x = i - 3;
                            return x;
                        }
                    }
                }

                //Diagonal \ win check
                // descendingDiagonalCheck
                for (int i = 3; i < 8; i++)
                {
                    for (int j = 3; j < 7; j++)
                    {
                        if (board[i, j] == 2 && board[i - 1, j - 1] == 2 && board[i - 2, j - 2] == 2 && board[i - 3, j - 3] == 0 && board[i - 3, j - 2] != 0)
                            x = i - 3;
                            return x;
                    }
                }
                canWin = true;
            }

            if (canBlockWin == false)
            {
                Debug.Log("Entered here");
                //Horizantal win check
                for (int i = 0; i < 8 - 3; i++)
                {
                    for (int j = 0; j < 7; j++)
                    {
                        if (board[i, j] == 1 && board[i + 1, j] == 1 && board[i + 2, j] == 1 && board[i + 3, j] == 0 && board[i + 3, j - 1] != 0)
                        {
                            x = i + 3;
                            return x;
                        }
                    }
                }

                //Vertical win check
                for (int i = 0; i < 8; i++)
                {
                    for (int j = 0; j < 7 - 3; j++)
                    {
                        if (board[i, j] == 1 && board[i, j + 1] == 1 && board[i, j + 2] == 1 && board[i, j + 3] == 0)
                        {
                            Debug.Log("BLOCK!");
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
                        if (board[i, j] == 1 && board[i - 1, j + 1] == 1 && board[i - 2, j + 2] == 1 && board[i - 3, j + 3] == 0 && board[i - 3, j + 2] != 0)
                        {
                            x = i - 3;
                            return x;
                        }
                    }
                }

                //Diagonal \ win check
                // descendingDiagonalCheck
                for (int i = 3; i < 8; i++)
                {
                    for (int j = 3; j < 7; j++)
                    {
                        if (board[i, j] == 1 && board[i - 1, j - 1] == 1 && board[i - 2, j - 2] == 1 && board[i - 3, j - 3] == 0 && board[i - 3, j - 2] != 0)
                            x = i - 3;
                        return x;
                    }
                }
                canBlockWin = true;
            }
            if(canBlockWin && canWin)
            {
                x = Random.Range(0, 7);
            }
            iterator = false;
        }
        return x;
    }
}*/
