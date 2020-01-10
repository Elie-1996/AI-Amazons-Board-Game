using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public sealed class HumanLogic : PlayerLogic
{

    // the following variables will be used to allow to use 'Burn'.
    private Indices selectedIndices;

    HumanLogic()
    {
        selectedIndices = null;
    }

    public void SelectIndices(int i, int j)
    {
        selectedIndices = new Indices(i, j);
    }

    protected sealed override IEnumerator MakeMove()
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
}
