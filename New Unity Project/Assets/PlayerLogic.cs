using System;
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
}

// Note: Ultimately this is can be the AI.
public abstract class PlayerLogic: MonoBehaviour
{
    // TODO: Special Case: at the last 'Move' operation, the player won't have anywhere to 'Burn', so this code will fail.
    public static int totalPlayers = 0; // this will be 2 at the maximum
    public static int globalTurn = 0; // this will fluctuate between 0 and 1
    protected readonly int playerTurnIndex; // this is the player's turn index, it does not change throughout the game
    protected bool finishedMove;



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
            GameTree.UpdateGameStateAndTree();
            GameBoardInformation.updateGameOver();
            globalTurn = (globalTurn + 1) % 2; // increase index to say that it is the other player's turn
            yield return PlayTurn();
        }
    }

    // Note: This function is NOT recursive.
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
