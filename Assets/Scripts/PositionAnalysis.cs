using System.Collections;
using System.Collections.Generic;
using UnityEngine;


//This is a class that has all the functionality to analyize the board and choose the best column to play
public class PositionAnalysis
{
    private int[,] _analysisBoard;

    public int[,] GetAnalysisBoard()
    {
        return _analysisBoard;
    }

    public void SetAnalysisBoard(int [,] temp)
    {
        this._analysisBoard = temp;
    }

    public int Evaluate(int depth)
    {
        return 2;
    }
}
