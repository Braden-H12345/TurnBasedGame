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
        Debug.Log("Enemy turn:... Enter");
        EnemyTurnBegan?.Invoke();

        StateMachine.Input.PressedWin += OnPressedWin;
        StateMachine.Input.PressedLose += OnPressedLose;


        if (tempPiece == null)
        {
            tempPiece = Spawner();
        }

        StartCoroutine(EnemyThinkingRoutine(_pauseDuration));
    }

    public override void Exit()
    {
        StateMachine.Input.PressedWin -= OnPressedWin;
        StateMachine.Input.PressedLose -= OnPressedLose;
        Debug.Log("Enemy Turn: Exit....");
    }

    IEnumerator EnemyThinkingRoutine(float pauseDuration)
    {
        Debug.Log("Enemy thinking...");
        yield return new WaitForSeconds(pauseDuration);

        Debug.Log("Enemy performs action");


        StartCoroutine(TakeMove(tempPiece));
        EnemyTurnEnded?.Invoke();
        StateMachine.ChangeState<PlayerTurnState>();
    }

    GameObject Spawner()
    {
        Vector3 spawnPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);

        List<int> moves = CalculateMoves();

        if (moves.Count > 0)
        {
            int column = moves[UnityEngine.Random.Range(0, moves.Count)];

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
            if (win)
            {
                OnPressedWin();
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

    void OnPressedWin()
    {
        StateMachine.ChangeState<WinState>();
    }

    void OnPressedLose()
    {
        StateMachine.ChangeState<LoseState>();
    }
}
