using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class GameTree
{
    public static GameState head;

    // Assumes that the board is already fully initialized. This is highly important as it will present the game state
    // from here on out.
    public static void Initialize()
    {
        // initializes the head of the tree
        head = new GameState(
            new HashSet<Indices>(InitializingParameters.WhiteQueens), 
            new HashSet<Indices>(InitializingParameters.BlackQueens), 
            new HashSet<Indices>(), 0, null, null);
    }

    // Move to the proper state that the game is in now
    // intuitively this is done after every move in the game.
    public static void UpdateGameStateAndTree(PlayerMove lastMove)
    {
        HashSet<Indices> newWhiteQueens = head.WhiteQueens;
        HashSet<Indices> newBlackQueens = head.BlackQueens;
        HashSet<Indices> newBurnedTiles = head.BurnedTiles;
        if (lastMove.QueenType == Piece.WHITEQUEEN)
        {
            newWhiteQueens.Remove(lastMove.oldLocation);
            newWhiteQueens.Add(lastMove.newLocation);
        }
        else
        {
            newBlackQueens.Remove(lastMove.oldLocation);
            newBlackQueens.Add(lastMove.newLocation);
        }
        newBurnedTiles.Add(lastMove.burnLocation);



        // the reason for putting parent = null is because we no longer care what we had before, we only need to
        // keep the game from this point on out
        GameState result = new GameState(newWhiteQueens, newBlackQueens, newBurnedTiles, head.depth + 1, null, lastMove);
        foreach (GameState child in head.Children)
        {
            if (child.Equals(result)) {
                result = child;
                break;
            }
        }

        head = result;
        head.parentAndMove = new Tuple<GameState, PlayerMove>(
            null,
            new PlayerMove(
                head.parentAndMove.Item2.QueenType,
                head.parentAndMove.Item2.oldLocation.i,
                head.parentAndMove.Item2.oldLocation.j,
                head.parentAndMove.Item2.newLocation.i,
                head.parentAndMove.Item2.newLocation.j,
                head.parentAndMove.Item2.burnLocation.i,
                head.parentAndMove.Item2.burnLocation.j
            )
        );
    }
}

public class GameState
{
    public readonly HashSet<Indices> WhiteQueens;
    public readonly HashSet<Indices> BlackQueens;
    public readonly HashSet<Indices> BurnedTiles;
    public readonly int depth; // smallest depth = 0
    public Tuple<GameState, PlayerMove> parentAndMove; // the move we needed to make from the parent to get to this state
    public HashSet<GameState> Children { get; private set; }
    public double HeuristicValue { get; private set; }


    public override bool Equals(object obj)
    {
        if (!(obj is GameState)) return false;
        GameState other = obj as GameState;
        if (other.depth != depth) return false;
        foreach (Indices queen in WhiteQueens)
        {
            if (!other.WhiteQueens.Contains(queen))
            {
                return false;
            }
        }
        foreach (Indices queen in BlackQueens)
        {
            if(!other.BlackQueens.Contains(queen))
            {
                return false;
            }
        }
        foreach (Indices burned in BurnedTiles)
        {
            if (!other.BurnedTiles.Contains(burned))
            {
                return false;
            }
        }
        return true;
    }

    public override int GetHashCode()
    {
        var hashCode = -1856427188;
        hashCode = hashCode * -1521134295 + EqualityComparer<HashSet<Indices>>.Default.GetHashCode(WhiteQueens);
        hashCode = hashCode * -1521134295 + EqualityComparer<HashSet<Indices>>.Default.GetHashCode(BlackQueens);
        hashCode = hashCode * -1521134295 + EqualityComparer<HashSet<Indices>>.Default.GetHashCode(BurnedTiles);
        hashCode = hashCode * -1521134295 + depth.GetHashCode();
        return hashCode;
    }

    // for debugging
    public override string ToString()
    {
        string str = "GameState:\n";

        str = str + "WhiteQueens: ";
        foreach (Indices whiteQueen in WhiteQueens)
        {
            str = str + ", " + whiteQueen.ToString();
        }
        str = str + "\n";

        str = str + "BlackQueens: ";
        foreach (Indices blackQueen in BlackQueens)
        {
            str = str + ", " + blackQueen.ToString();
        }
        str = str + "\n";

        str = str + "BurnedTiles: ";
        foreach (Indices burnedTile in BurnedTiles)
        {
            str = str + ", " + burnedTile.ToString();
        }
        str = str + "\n";

        if (parentAndMove.Item1 != null)
        {
            str = str + "Move=" + parentAndMove.Item2.ToString();
            str = str + "\n";
        }

        str = str + "Children Count = " + Children.Count;
        str = str + "\n";

        return str;
    }

    public bool isWhiteTurn
    {
        get => depth % 2 == 0; // 0 is white queen, (depth = 0 is white).
    }

    public GameState(HashSet<Indices> _WhiteQueens, HashSet<Indices> _BlackQueens, HashSet<Indices> _BurnedTiles, int _depth, GameState _parent, PlayerMove _move)
    {
        WhiteQueens = _WhiteQueens;
        BlackQueens = _BlackQueens;
        BurnedTiles = _BurnedTiles;
        depth = _depth;
        parentAndMove = new Tuple<GameState, PlayerMove>(_parent, _move);
        Children = new HashSet<GameState>();
        HeuristicValue = isWhiteTurn == true ? double.NegativeInfinity : double.PositiveInfinity; // this decision is due to the fact that white is trying to maximize, while black is trying to minimize.
    }

    public void Expand() { ExpandDFS(1); }

    public void ExpandDFS(int additionalDepth) { ExpandDFS(this, additionalDepth); }

    // Reveal all children at depth d+1
    private static void ExpandDFS(GameState currentState, int additionalDepth)
    {
        // if we reached the required depth, end call.
        if (additionalDepth <= 0)
        {
            if (currentState.Children.Count == 0)
            {
                // evaluate heuristic as a leaf.
                currentState.EvaluateHeuristicAsLeaf();
            }
            return;
        }
        // if we have already expanded the current state, then try to expand its children. skip expanding again.
        else if (currentState.Children.Count > 0)
        {
            foreach (GameState child in currentState.Children)
            {
                ExpandDFS(child, additionalDepth - 1);
            }
            return;
        }
        // if the state hasn't expanded already, then expand with the depth.
        else
        {
            HashSet<Indices> Queens = new HashSet<Indices>();
            if (currentState.isWhiteTurn)
                Queens.UnionWith(currentState.WhiteQueens);
            else
                Queens.UnionWith(currentState.BlackQueens);

            foreach (Indices queenIndex in Queens)
            {
                UpdateWithEveryLegalMoveAtDepth(currentState, queenIndex, additionalDepth);
            }
        }
    }

    private static void UpdateWithEveryLegalMoveAtDepth(GameState currentState, Indices queenIndex, int additionalDepth)
    {
        GetEveryLegalMoveInStarAngles(currentState, queenIndex, new HashSet<Indices>(currentState.WhiteQueens), new HashSet<Indices>(currentState.BlackQueens), new HashSet<Indices>(currentState.BurnedTiles), -1, -1, true, additionalDepth);
    }


    // by Star Angles we mean the way a queen moves in chess. (which is a star *).
    private static void GetEveryLegalMoveInStarAngles(
        GameState currentState,
        Indices startingIndex,
        HashSet<Indices> currentWhiteQueens,
        HashSet<Indices> currentBlackQueens,
        HashSet<Indices> currentBurnedTiles,
        int queen_started_at_i, int queen_started_at_j, // these two parameters simply say where the queen started, they are usable only at the end of the move (after burning, i.e when adding a new GameState)
        bool isQueenIndex, // this is a crucial parameter in the expand algorithm
                           // when it is set to true, we will assume that @startingIndex is a queen's starting index
                           // essentially we are currently moving the queen, thus completing the FIRST part of the move.
                           // when it is set to false, we will assume that @startingIndex is a queen's END location,
                           // thus, she is looking to throw a BURN tile, therefore completing the move entirely.
        int additionalDepth // how deep to DFS expand within the tree
        )
    {
        for (int i_direction = -1; i_direction <= 1; ++i_direction)
        {
            for (int j_direction = -1; j_direction <= 1; ++j_direction)
            {
                if (i_direction == 0 && j_direction == 0)
                    continue;

                bool shouldPrune = GetEveryLegalMoveInGivenDirection(currentState, startingIndex.i, startingIndex.j, i_direction, j_direction, new HashSet<Indices>(currentWhiteQueens), new HashSet<Indices>(currentBlackQueens), new HashSet<Indices>(currentBurnedTiles), queen_started_at_i, queen_started_at_j, isQueenIndex, additionalDepth);

                // consider alpha-beta pruning
                if (shouldPrune)
                    break;
            }
        }
    }

    // returns true if we should perform alpha-beta pruning!
    private static bool GetEveryLegalMoveInGivenDirection(
        GameState currentState,
        int i, int j, 
        int i_direction, int j_direction, 
        HashSet<Indices> currentWhiteQueens,
        HashSet<Indices> currentBlackQueens,
        HashSet<Indices> currentBurnedTiles, 
        int queen_started_at_i, int queen_started_at_j,
        bool isQueenIndex, int additionalDepth)
    {
        int initial_i = i;
        int initial_j = j;

        int destination_i = 0;
        if (i_direction > 0)
            destination_i = InitializingParameters.rows - 1;

        int destination_j = 0;
        if (j_direction > 0)
            destination_j = InitializingParameters.columns - 1;

        Indices oldBurnLocation = new Indices(-1, -1); // only useful when performing a burn move
        while (i != destination_i || j != destination_j)
        {
            i += i_direction;
            j += j_direction;
            Indices mid_way_index = new Indices(i, j); // where we are currently "standing"
            if (currentWhiteQueens.Contains(mid_way_index) ||
                currentBlackQueens.Contains(mid_way_index) ||
                currentBurnedTiles.Contains(mid_way_index) ||
                i < 0 || i >= GameBoardInformation.rows  ||
                j < 0 || j >= GameBoardInformation.columns)
            {
                break; // if we reach here, it means the way is blocked for index mid_way_index.
                        // as such, it is pointless to continue moving in the tiles in direction (i_direction, j_direction).
            }
            else
            {
                // here, we know that the move thus far is possible, it is time to
                // ask whether we are moving the queen (first part of the move)
                // or throwing a burn tile (completing the move in its entirety)
                if (isQueenIndex) // when this is true, it means we only moved the queen
                {
                    Indices oldLocation = new Indices(initial_i, initial_j);
                    // get all throw possibilities
                    HashSet<Indices> newWhiteQueens = new HashSet<Indices>(currentWhiteQueens);
                    HashSet<Indices> newBlackQueens = new HashSet<Indices>(currentBlackQueens);
                    if (currentState.isWhiteTurn)
                    {
                        newWhiteQueens.Remove(oldLocation);
                        newWhiteQueens.Add(mid_way_index);
                    }
                    else
                    {
                        newBlackQueens.Remove(oldLocation);
                        newBlackQueens.Add(mid_way_index);
                    }
                    GetEveryLegalMoveInStarAngles(currentState, mid_way_index, newWhiteQueens, newBlackQueens, currentBurnedTiles, initial_i, initial_j, false, additionalDepth);
                }
                else
                { // when this is true, it means we are selecting the burn location
                    // add current throw possibiltiy
                    HashSet<Indices> newBurnIndices = new HashSet<Indices>(currentBurnedTiles);
                    newBurnIndices.Remove(oldBurnLocation);
                    newBurnIndices.Add(mid_way_index);
                    oldBurnLocation = mid_way_index; // update this to be the last location which burned.

                    // Add a single gamestate
                    GameState child = new GameState(new HashSet<Indices>(currentWhiteQueens), new HashSet<Indices>(currentBlackQueens), newBurnIndices, currentState.depth + 1, currentState, new PlayerMove(currentState.isWhiteTurn ? Piece.WHITEQUEEN : Piece.BLACKQUEEN, queen_started_at_i, queen_started_at_j, initial_i, initial_j, mid_way_index.i, mid_way_index.j));
                    currentState.Children.Add(child);
                    child.ExpandDFS(additionalDepth - 1); // go to the next depth, this line garauntees DFS procedure.

                    // child should have a heuristic value at this point, and the parent should 
                    // have their heuristic updated as required by minimax
                    currentState.EvaluateHeuristicAsParent(child.HeuristicValue);
                    bool shouldPrune = CheckIfAlphaBetaPruningIsPossible(currentState);
                    if (shouldPrune)
                        return true;
                }
            }
        }

        return false;
    }

    private static bool CheckIfAlphaBetaPruningIsPossible(GameState state)
    {
        GameState parent = state.parentAndMove.Item1;
        if (parent == null) return false; // when you're the head, you can't prune anything.
        if (parent.isWhiteTurn)
        {
            // if parent is a white turn, it will try to maximize the heuristic
            // thus, we can ignore values that are smaller than its heuristic value
            // can be visualized well at time 6:06 at vid: https://www.youtube.com/watch?v=l-hh51ncgDI&t=368s
            if (parent.HeuristicValue > state.HeuristicValue) return true;
        }
        else
        {
            // if parent is a black turn, it will try to minimize the heuristic
            // thus, we can ignore values that are larger than its heuristic value
            // can be visualized well at time 5:32 at vid: https://www.youtube.com/watch?v=l-hh51ncgDI&t=368s
            if (parent.HeuristicValue < state.HeuristicValue)
                return true;
        }

        return false;
    }

    private void EvaluateHeuristicAsParent(double recentChildHeuristicValue)
    {
        if (isWhiteTurn)
        {
            // try to maximize heuristic
            if (recentChildHeuristicValue > HeuristicValue)
            {
                HeuristicValue = recentChildHeuristicValue;
            }
        }
        else
        {
            // try to minimize the heuristic
            if (recentChildHeuristicValue < HeuristicValue)
            {
                HeuristicValue = recentChildHeuristicValue;
            }
        }
    }


    private static System.Random randomizer = new System.Random();
    private void EvaluateHeuristicAsLeaf()
    {
        HeuristicValue = randomizer.NextDouble()*50.0; // some evaluation, change this later!
    }
}

public sealed class AILogic : PlayerLogic
{

    private GameState currentState;

    AILogic() {}

    protected sealed override IEnumerator MakeMove()
    {
        currentState = GameTree.head;
        lastMove = ThinkThenDecide();
        MakeMoveOnBoard(lastMove);
        yield break;
    }

    private PlayerMove ThinkThenDecide()
    {
        // explore the tree
        //currentState.Expand();
        currentState.ExpandDFS(2);

        // play your move
        GameState playState = FindBestMove();
        return playState.parentAndMove.Item2;
    }

    private GameState FindBestMove()
    {
        HashSet<GameState> children = currentState.Children;

        if (currentState.isWhiteTurn)
        {
            GameState highestState = null;
            double highestHeuristic = double.NegativeInfinity;
            foreach (GameState child in children)
            {
                if (child.HeuristicValue > highestHeuristic)
                {
                    highestHeuristic = child.HeuristicValue;
                    highestState = child;
                }
            }
            return highestState;
        }
        else
        {
            GameState lowestState = null;
            double lowestHeuristic = double.PositiveInfinity;
            foreach (GameState child in children)
            {
                if (child.HeuristicValue < lowestHeuristic)
                {
                    lowestHeuristic = child.HeuristicValue;
                    lowestState = child;
                }
            }
            return lowestState;
        }
    }

    private void MakeMoveOnBoard(PlayerMove move)
    {
        GameBoardInformation.movePiece(move.oldLocation.i, move.oldLocation.j, move.newLocation.i, move.newLocation.j);
        GameBoardInformation.burnPiece(move.newLocation.i, move.newLocation.j, move.burnLocation.i, move.burnLocation.j);
        finishedMove = true;
    }
}
