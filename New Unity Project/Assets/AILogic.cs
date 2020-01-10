using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class GameTree
{
    public static GameState node;

    // Assumes that the board is already fully initialized. This is highly important as it will present the game state
    // from here on out.
    public static void Initialize()
    {
        // initializes the head of the tree
        node = new GameState(
            new HashSet<Indices>(InitializingParameters.WhiteQueens), 
            new HashSet<Indices>(InitializingParameters.BlackQueens), 
            new HashSet<Indices>(), 0, null);
    }

    // Move to the proper state that the game is in now
    // intuitively this is done after every move in the game.
    public static void UpdateGameStateAndTree()
    {
        throw new NotImplementedException();
    }
}

public class GameState
{
    private readonly HashSet<Indices> WhiteQueens;
    private readonly HashSet<Indices> BlackQueens;
    private readonly HashSet<Indices> BurnedTiles;
    private readonly int depth; // smallest depth = 0
    private readonly GameState parent;

    public bool isWhiteTurn
    {
        get => depth % 2 == 0; // 0 is white queen, (depth = 0 is white).
    }

    public GameState(HashSet<Indices> _WhiteQueens, HashSet<Indices> _BlackQueens, HashSet<Indices> _BurnedTiles, int _depth, GameState _parent)
    {
        WhiteQueens = _WhiteQueens;
        BlackQueens = _BlackQueens;
        BurnedTiles = _BurnedTiles;
        depth = _depth;
        parent = _parent;
    }

    // Reveal all children at depth d+1
    public HashSet<GameState> Expand()
    {
        HashSet<GameState> children = new HashSet<GameState>();

        HashSet<Indices> Queens;
        if (isWhiteTurn)
            Queens = WhiteQueens;
        else
            Queens = BlackQueens;

        foreach (Indices queenIndex in Queens)
        {
            children.UnionWith(GetEveryLegalMoveAtNextDepth(queenIndex));
        }

        return children;
    }

    private HashSet<GameState> GetEveryLegalMoveAtNextDepth(Indices queenIndex)
    {
        return GetEveryLegalMoveInStarAngles(queenIndex, WhiteQueens, BlackQueens, BurnedTiles, true);
    }


    // by Star Angles we mean the way a queen moves in chess. (which is a star *).
    private HashSet<GameState> GetEveryLegalMoveInStarAngles(
        Indices startingIndex,
        HashSet<Indices> currentWhiteQueens,
        HashSet<Indices> currentBlackQueens,
        HashSet<Indices> currentBurnedTiles,
        bool isQueenIndex // this is a crucial parameter in the expand algorithm
                          // when it is set to true, we will assume that @startingIndex is a queen's starting index
                          // essentially we are currently moving the queen, thus completing the FIRST part of the move.
                          // when it is set to false, we will assume that @startingIndex is a queen's END location,
                          // thus, she is looking to throw a BURN tile, therefore completing the move entirely.
        )
    {
        HashSet<GameState> queenMoveChildren = new HashSet<GameState>();
        for (int i_direction = -1; i_direction <= 1; ++i_direction)
        {
            for (int j_direction = -1; j_direction <= 1; ++j_direction)
            {
                if (i_direction == 0 && j_direction == 0)
                    continue;
                queenMoveChildren.UnionWith(GetEveryLegalMoveInGivenDirection(startingIndex.i, startingIndex.j, i_direction, j_direction, currentWhiteQueens, currentBlackQueens, currentBurnedTiles, isQueenIndex));
            }
        }
        return queenMoveChildren;
    }

    private HashSet<GameState> GetEveryLegalMoveInGivenDirection(
        int i, int j, 
        int i_direction, int j_direction, 
        HashSet<Indices> currentWhiteQueens,
        HashSet<Indices> currentBlackQueens,
        HashSet<Indices> currentBurnedTiles, 
        bool isQueenIndex)
    {
        int initial_i = i;
        int initial_j = j;

        int destination_i = 0;
        if (i_direction > 0)
            destination_i = InitializingParameters.rows - 1;

        int destination_j = 0;
        if (j_direction > 0)
            destination_j = InitializingParameters.columns - 1;

        HashSet<GameState> children = new HashSet<GameState>();
        while (i != destination_i || j != destination_j)
        {
            i += i_direction;
            j += j_direction;
            Indices mid_way_index = new Indices(i, j); // where we are currently "standing"
            if (currentWhiteQueens.Contains(mid_way_index) ||
                currentBlackQueens.Contains(mid_way_index) ||
                currentBurnedTiles.Contains(mid_way_index))
            {
                break; // if we reach here, it means the way is blocked for index mid_way_index.
                        // as such, it is pointless to continue moving in the tiles in direction (i_direction, j_direction).
            }
            else
            {
                Indices oldLocation = new Indices(initial_i, initial_j);
                // here, we know that the move thus far is possible, it is time to
                // ask whether we are moving the queen (first part of the move)
                // or throwing a burn tile (completing the move in its entirety)
                if (isQueenIndex) // when this is true, it means we only moved the queen
                {
                    // get all throw possibilities
                    HashSet<Indices> newWhiteQueens = currentWhiteQueens;
                    HashSet<Indices> newBlackQueens = currentBlackQueens;
                    if (isWhiteTurn)
                    {
                        newWhiteQueens.Remove(oldLocation);
                        newWhiteQueens.Add(mid_way_index);
                    }
                    else
                    {
                        newBlackQueens.Remove(oldLocation);
                        newBlackQueens.Add(mid_way_index);
                    }
                    children.UnionWith(GetEveryLegalMoveInStarAngles(mid_way_index, newWhiteQueens, newBlackQueens, currentBurnedTiles, false));
                }
                else
                { // when this is true, it means we are selecting the burn location
                    // add current throw possibiltiy
                    HashSet<Indices> newBurnedTiles = currentBurnedTiles;
                    newBurnedTiles.Remove(oldLocation);
                    newBurnedTiles.Add(mid_way_index);


                    // Add a single gamestate
                    children.Add(new GameState(currentWhiteQueens, currentBlackQueens, newBurnedTiles, depth + 1, this));
                }
            }
        }

        return children;
    }
}

public sealed class AILogic : PlayerLogic
{
    AILogic() { }

    protected sealed override IEnumerator MakeMove()
    {
        throw new NotImplementedException();
    }
}
