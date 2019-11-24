using System;
using System.Collections;
using UnityEngine;

public class PlayerLogic
{
    public static int totalPlayers = 0; // this will be 2 at the maximum
    public static int globalTurn = 0; // this will fluctuate between 0 and 1
    private readonly int playerTurnIndex; // this is the player's turn index, it does not change throughout the game

    public PlayerLogic()
    {
        playerTurnIndex = totalPlayers++;
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
            yield return null;
        }
        else
        {
            yield return new WaitUntil(()=>playerTurnIndex == globalTurn); // only play when it is the current player's turn
            MakeMove(); // This is where the player will actually perform the move.
            Debug.Log("Player " + playerTurnIndex + ", waiting..."); // This is to help debug the logic, imitiating player "makemove" time
            yield return new WaitForSeconds(5.0f);
            Debug.Log("Increasing globalTurn");
            globalTurn = (globalTurn + 1) % 2; // increase index to say that it is the other player's turn
            yield return PlayTurn();
        }

    }

    private void MakeMove()
    {
        // TODO: Implement Logical moves on board.
    }
}
