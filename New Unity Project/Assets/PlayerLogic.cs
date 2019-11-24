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
public class PlayerLogic: MonoBehaviour
{
    // TODO: Special Case: at the last 'Move' operation, the player won't have anywhere to 'Burn', so this code will fail.
    public static int totalPlayers = 0; // this will be 2 at the maximum
    public static int globalTurn = 0; // this will fluctuate between 0 and 1
    private readonly int playerTurnIndex; // this is the player's turn index, it does not change throughout the game
    private bool finishedMove;

    // the following variables will be used to allow to use 'Burn'.
    private Indices selectedIndices;

    // Player Logic MUST HAVE ONLY Default constructor.
    public PlayerLogic()
    {
        playerTurnIndex = totalPlayers++;
        selectedIndices = null;
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
        if (GameBoardInformation.isGameOver == true)
        {
            yield break;
        }
        else
        {
            yield return new WaitUntil(()=>playerTurnIndex == globalTurn); // only play when it is the current player's turn
            StartCoroutine(MakeMove()); // This is where the player will actually perform the move.
            yield return new WaitUntil(() => finishedMove == true);
            finishedMove = false; // resets to false, to allow the player to play again
            globalTurn = (globalTurn + 1) % 2; // increase index to say that it is the other player's turn
            yield return PlayTurn();
        }
    }

    // Note: This function is NOT recursive.
    private IEnumerator MakeMove()
    {
        // wait until select the queen
        Debug.Log("Player " + playerTurnIndex + ", Please Select Queen");
        yield return new WaitUntil(() => selectedIndices != null);
        int queen_i = selectedIndices.i;
        int queen_j = selectedIndices.j;
        selectedIndices = null;

        // check if player selected a correct queen.
        if ((int)GameBoardInformation.getPieceAt(queen_i, queen_j) != playerTurnIndex)
        {
            Debug.Log("Player " + playerTurnIndex + ", Inappropriate QUEEN Tile");
            yield return MakeMove();
            yield break;
        }

        // wait until select move location
        Debug.Log("Player " + playerTurnIndex + ", Please Select Move Location");
        yield return new WaitUntil(() => selectedIndices != null);
        int destination_i = selectedIndices.i;
        int destination_j = selectedIndices.j;
        selectedIndices = null;
        
        // move the queen
        bool didMove = MovePiece(queen_i, queen_j, destination_i, destination_j);
        if (didMove == false)
        {
            Debug.Log("Player " + playerTurnIndex + ", Inappropriate MOVE Tile");
            yield return MakeMove();
            yield break;
        }

        // wait until select burn location
        Debug.Log("Player " + playerTurnIndex + ", Please Select BURN Location");
        yield return new WaitUntil(() => selectedIndices != null);
        int burn_i = selectedIndices.i;
        int burn_j = selectedIndices.j;
        selectedIndices = null;

        bool didBurn = BurnPiece(destination_i, destination_j, burn_i, burn_j);
        if (didBurn == false)
        {
            Debug.Log("Player " + playerTurnIndex + ", Inappropriate BURN Tile");
            MovePiece(destination_i, destination_j, queen_i, queen_j); // reset the damage done
            yield return MakeMove();
            yield break;
        }
        finishedMove = true;
    }

    public void SelectIndices(int i, int j)
    {
        selectedIndices = new Indices(i, j);
    }

    private bool MovePiece(int i, int j, int destination_i, int destination_j)
    {
        return GameBoardInformation.movePiece(i, j, destination_i, destination_j);
    }

    private bool BurnPiece(int source_i, int source_j, int destination_i, int destination_j)
    {
        return GameBoardInformation.burnPiece(source_i, source_j, destination_i, destination_j);
    }
}
