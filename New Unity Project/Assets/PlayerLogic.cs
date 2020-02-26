using System;
using System.Diagnostics;
using System.Threading;
using System.Collections;
using UnityEngine;

public class Indices
{
    public readonly int i, j;
    public Indices(int _i, int _j)
    {
        i = _i;
        j = _j;
    }

    public override bool Equals(object obj)
    {
        if (obj is Indices)
        {
            Indices o = (obj as Indices);
            return o.i == i && o.j == j;
        }
        return false;
    }

    public override int GetHashCode()
    {
        return (i + "," + j).GetHashCode();
    }

    public override string ToString()
    {
        return "(" + i + "," + j + ")";
    }
}

public class PlayerMove
{
    public readonly Piece QueenType;
    public readonly Indices oldLocation;
    public readonly Indices newLocation;
    public readonly Indices burnLocation;

    public PlayerMove(Piece _QueenType, int old_i, int old_j, int new_i, int new_j, int burn_i, int burn_j)
    {
        QueenType = _QueenType;
        oldLocation = new Indices(old_i, old_j);
        newLocation = new Indices(new_i, new_j);
        burnLocation = new Indices(burn_i, burn_j);
    }

    // for debugging
    public override string ToString()
    {
        return "from:" + oldLocation.ToString() + ", to:" + newLocation.ToString() + ", burn:" + burnLocation.ToString();
    }

    // for user-friendly printing
    public string ToPrintableString()
    {
        return IndexToLetter(oldLocation.j) + (GameBoardInformation.rows - 1 - oldLocation.i) + "-" + IndexToLetter(newLocation.j) + (GameBoardInformation.rows - 1 - newLocation.i) + "/" + IndexToLetter(burnLocation.j) + (GameBoardInformation.rows - 1 - burnLocation.i);
    }

    private static string IndexToLetter(int x)
    {
        return ((char)('A' + (char)x)).ToString();
    }
}

// Note: Ultimately this is can be the AI.
public abstract class PlayerLogic: MonoBehaviour
{
    // TODO: Special Case: at the last 'Move' operation, the player won't have anywhere to 'Burn', so this code will fail.
    public static int totalPlayers = 0; // this will be 2 at the maximum
    public static int globalTurn = 0; // this will fluctuate between 0 and 1
    protected readonly int playerTurnIndex; // this is the player's turn index, it does not change throughout the game
    protected bool finishedMove;
    protected PlayerMove lastMove;
    protected double secondsPassed = 0;


    // Player Logic MUST HAVE ONLY Default constructor.
    public PlayerLogic()
    {
        playerTurnIndex = totalPlayers++;
        finishedMove = false;
    }

    public static void reset()
    {
        totalPlayers = 0;
        globalTurn = 0;
    }

    // This function should be called when we want this player to play
    public IEnumerator PlayTurn()
    {
        yield return new WaitUntil(() => playerTurnIndex == globalTurn); // only play when it is the current player's turn
        if (GameBoardInformation.isGameOver == true)
        {
            yield break;
        }
        else
        {
            StartCoroutine(MakeMove()); // This is where the player will actually perform the move.
            yield return new WaitUntil(() => finishedMove == true);

            finishedMove = false; // resets to false, to allow the player to play again
            GameTree.UpdateGameStateAndTree(lastMove);
            TechnicalStatistics.LastHeuristic = GameTree.head.HeuristicValue;
            TechnicalStatistics.LastMoveString = TechnicalStatistics.GetLastMoveString(GameTree.head, secondsPassed);
            TechnicalStatistics.totalTimePassed += secondsPassed;
            GameBoardInformation.updateGameOver();
            globalTurn = (globalTurn + 1) % 2; // increase index to say that it is the other player's turn
            yield return PlayTurn();
        }
    }

    protected abstract IEnumerator MakeMove();

    protected bool MovePiece(int i, int j, int destination_i, int destination_j)
    {
        return GameBoardInformation.movePiece(i, j, destination_i, destination_j);
    }

    protected bool BurnPiece(int source_i, int source_j, int destination_i, int destination_j)
    {
        return GameBoardInformation.burnPiece(source_i, source_j, destination_i, destination_j);
    }
}
