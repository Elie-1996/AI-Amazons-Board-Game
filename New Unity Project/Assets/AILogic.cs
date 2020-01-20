using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading;

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
    private bool IsFullyExpanded; // asks whether all children have been generated, this is needed when playing against someone who is not doing a minimax decision. (for instance, a human player).



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
        IsFullyExpanded = false;
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
        else if (currentState.IsFullyExpanded)
        {
            List<Thread> threads = new List<Thread>();
            foreach (GameState child in currentState.Children)
            {
                var thread = new Thread(() =>
                {
                    ExpandDFS(child, additionalDepth - 1);
                });
                threads.Add(thread);
                thread.Start();
            }
            foreach (Thread thread in threads)
            {
                thread.Join();
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

            List<Thread> threads = new List<Thread>();
            foreach (Indices queenIndex in Queens)
            {
                var thread = new Thread(() =>
                {
                    UpdateWithEveryLegalMoveAtDepth(currentState, queenIndex, additionalDepth);
                });
                threads.Add(thread);
                thread.Start();
            }
            foreach (Thread thread in threads)
            {
                thread.Join();
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
        bool isFullyExpanded = true;
        for (int i_direction = -1; i_direction <= 1; ++i_direction)
        {
            for (int j_direction = -1; j_direction <= 1; ++j_direction)
            {
                if (i_direction == 0 && j_direction == 0)
                    continue;
                bool shouldPrune = false;

                shouldPrune = GetEveryLegalMoveInGivenDirection(currentState, startingIndex.i, startingIndex.j, i_direction, j_direction, new HashSet<Indices>(currentWhiteQueens), new HashSet<Indices>(currentBlackQueens), new HashSet<Indices>(currentBurnedTiles), queen_started_at_i, queen_started_at_j, isQueenIndex, additionalDepth);

                lock (currentState)
                {
                    // consider alpha-beta pruning
                    if (shouldPrune)
                    {
                        isFullyExpanded = false;
                    }
                }
                if (shouldPrune) break;
            }
        }
        lock (currentState)
        {
            currentState.IsFullyExpanded = isFullyExpanded;
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
                    GameState child = null;
                    lock (currentState.Children)
                    {
                        child = new GameState(new HashSet<Indices>(currentWhiteQueens), new HashSet<Indices>(currentBlackQueens), newBurnIndices, currentState.depth + 1, currentState, new PlayerMove(currentState.isWhiteTurn ? Piece.WHITEQUEEN : Piece.BLACKQUEEN, queen_started_at_i, queen_started_at_j, initial_i, initial_j, mid_way_index.i, mid_way_index.j));
                        currentState.Children.Add(child);
                    }
                    child.ExpandDFS(additionalDepth - 1); // go to the next depth, this line garauntees DFS procedure.
                    // child should have a heuristic value at this point, and the parent should 
                    // have their heuristic updated as required by minimax
                    bool shouldPrune = false;
                    lock (currentState)
                    {
                        currentState.EvaluateHeuristicAsParent(child.HeuristicValue);
                        shouldPrune = CheckIfAlphaBetaPruningIsPossible(currentState);
                    }
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


    private void EvaluateHeuristicAsLeaf()
    {
        Piece winner = Winner();
        if (winner == Piece.WHITEQUEEN) HeuristicValue = double.PositiveInfinity;
        else if (winner == Piece.BLACKQUEEN) HeuristicValue = double.NegativeInfinity;
        else
        {
            if (depth <= GameBoardInformation.VeryEarlyInTheGame)
                HeuristicValue = FastEvaluation(WhiteQueens, BlackQueens, BurnedTiles);
            else
                HeuristicValue = TerritorialMobility(WhiteQueens, BlackQueens, BurnedTiles);
        }
    }

    private static double FastEvaluation(HashSet<Indices> whiteQueens, HashSet<Indices> blackQueens, HashSet<Indices> burnedTiles)
    {
        HashSet<Indices> BlockingTiles = new HashSet<Indices>(burnedTiles);
        BlockingTiles.UnionWith(whiteQueens);
        BlockingTiles.UnionWith(blackQueens);

        int whiteMoves = 0;
        foreach (Indices whiteQueen in whiteQueens)
        {
            whiteMoves += GetAmountOfPossibleMoves(whiteQueen, BlockingTiles);
        }

        int blackMoves = 0;
        foreach (Indices blackQueen in blackQueens)
        {
            blackMoves += GetAmountOfPossibleMoves(blackQueen, BlockingTiles);
        }

        return 0.65 * whiteMoves - 0.35 * blackMoves;
    }

    private static int GetAmountOfPossibleMoves(Indices queen, HashSet<Indices> BlockingTiles)
    {
        int totalPossibleMoves = 0;
        for (int i_dir = -1; i_dir <= 1; ++i_dir)
        {
            for (int j_dir = -1; j_dir <= 1; ++j_dir)
            {
                if (i_dir == 0 && j_dir == 0) continue;
                int curr_i = queen.i, curr_j = queen.j;
                curr_i += i_dir; curr_j = j_dir;
                while ((0 <= curr_i && curr_i < GameBoardInformation.rows) &&
                (0 <= curr_j && curr_j < GameBoardInformation.columns))
                {
                    if (BlockingTiles.Contains(new Indices(curr_i, curr_j))) break;

                    ++totalPossibleMoves;
                    curr_i += i_dir;
                    curr_j += j_dir;
                }
            }
        }
        return totalPossibleMoves;
    }

    // source: https://core.ac.uk/download/pdf/81108035.pdf
    // very nice explanation imo! :)
    // although, I only evaluate the t part of the heuristic, it is enough for me that it plays reasonably :)
    private static double TerritorialMobility(
        HashSet<Indices> WhiteQueens,
        HashSet<Indices> BlackQueens,
        HashSet<Indices> BurnedTiles
        )
    {
        HashSet<Indices> BlockingTiles = new HashSet<Indices>();
        BlockingTiles.UnionWith(BurnedTiles);
        BlockingTiles.UnionWith(WhiteQueens);
        BlockingTiles.UnionWith(BlackQueens);
        return TerritorialMobilityParameterSetter(WhiteQueens, BlackQueens, BlockingTiles);
    }


    private static double TerritorialMobilityParameterSetter(
        HashSet<Indices> WhiteQueens, 
        HashSet<Indices> BlackQueens, 
        HashSet<Indices> BlockingTiles
        )
    {
        Tuple<double[], double[]> listOfAll_D1_1_and_D2_1 = null;
        var thread1 = new Thread(() => {
            listOfAll_D1_1_and_D2_1 = GetQueenAndKingDistances(WhiteQueens, BlockingTiles);
        });
        Tuple<double[], double[]> listOfAll_D1_2_and_D2_2 = null;
        var thread2 = new Thread(() => {
            listOfAll_D1_2_and_D2_2 = GetQueenAndKingDistances(BlackQueens, BlockingTiles);
        });

        thread1.Start();
        thread2.Start();
        thread1.Join();
        thread2.Join();

        Tuple<double, double> t1_t2_w = GetTs(listOfAll_D1_1_and_D2_1, listOfAll_D1_2_and_D2_2);
        double t1 = t1_t2_w.Item1, t2 = t1_t2_w.Item2;

        Tuple<double, double> c1_c2 = GetCs(listOfAll_D1_1_and_D2_1.Item1, listOfAll_D1_2_and_D2_2.Item1,
            listOfAll_D1_2_and_D2_2.Item2, listOfAll_D1_1_and_D2_1.Item2);
        double c1 = c1_c2.Item1, c2 = c1_c2.Item2;

        Tuple<double, double, double, double> all_fs = GetFs(BlockingTiles.Count);
        double f1 = all_fs.Item1, f2 = all_fs.Item2, f3 = all_fs.Item3, f4 = all_fs.Item4;

        // step4: apply t = f1 * t1 + f2 * c1 + f3 * c2 + f4 * t2
        double t = f1 * t1 + f2 * c1 + f3 * c2 + f4 * t2;

        //step5: find m ---- unfortunately not enough time to understand + implement :(
        //double m = 0;
        // step6: return t+m
        //return t + m;

        // should also compute m, but I was running out of time :(
        return t;
    }

    private static Tuple<double, double> GetTs(
        Tuple<double[], double[]> listOfAll_D1_1_and_D2_1,
        Tuple<double[], double[]> listOfAll_D1_2_and_D2_2
        )
    {
        double t1 = 0;
        var thread1 = new Thread(() => {
            t1 = sum_of_deltas(listOfAll_D1_1_and_D2_1.Item1, listOfAll_D1_2_and_D2_2.Item1);
        });

        double t2 = 0;
        var thread2 = new Thread(() => {
            t2 = sum_of_deltas(listOfAll_D1_1_and_D2_1.Item2, listOfAll_D1_2_and_D2_2.Item2);
        });

        thread1.Start();
        thread2.Start();
        thread1.Join();
        thread2.Join();
        return new Tuple<double, double>(t1, t2);
    }

    private static Tuple<double[], double[]> GetQueenAndKingDistances(HashSet<Indices> PlayerQueens, HashSet<Indices> BlockingTiles)
    {
        List<Tuple<double[,], double[,]>> possibleBoards = new List<Tuple<double[,], double[,]>>();
        foreach (Indices Queen in PlayerQueens)
        {
            Tuple<double[,], double[,]> minBoardForQueen = FindShortestRoutes(Queen.i, Queen.j, BlockingTiles);
            possibleBoards.Add(minBoardForQueen);
        }

        Tuple<double[,], double[,]> minIntersection = FindMinimumIntersection(possibleBoards);
        Tuple<double[], double[]> minIntersectionAsOneDimensionalArray = TupleTwoDimensionalToTupleOneDimensional(minIntersection);

        return new Tuple<double[], double[]>(minIntersectionAsOneDimensionalArray.Item1, minIntersectionAsOneDimensionalArray.Item2);
    }

    private static Tuple<double[,], double[,]> FindShortestRoutes(int i, int j, HashSet<Indices> BlockingTiles)
    {
        int n = GameBoardInformation.rows;
        int m = GameBoardInformation.columns;
        double[,] kingMoveBoard = new double[n, m];
        double[,] queenMoveBoard = new double[n, m];

        for (int row = 0; row < n; ++row)
        {
            for (int col = 0; col < m; ++col)
            {
                kingMoveBoard[row, col] = double.PositiveInfinity;
                queenMoveBoard[row, col] = double.PositiveInfinity;
            }
        }

        kingMoveBoard[i, j] = 0;
        queenMoveBoard[i, j] = 0;
        FindShortestRoutesHelper(kingMoveBoard, queenMoveBoard, BlockingTiles, new List<Indices>() { new Indices(i, j) }, 1);

        return new Tuple<double[,], double[,]>(kingMoveBoard, queenMoveBoard);
    }

    public static void PrintBoardValues(int ii, int jj, double[,] board)
    {
        string str = "board on" + new Indices(ii,jj).ToString() + "=\n";
        for (int i = 0; i < board.GetLength(0); ++i)
        {
            for (int j = 0; j < board.GetLength(1); ++j)
            {
                str += board[i, j] + "\t";
            }
            str += "\n";
        }

        Debug.Log(str);
    }

    public static void FindShortestRoutesHelper(double[,] kingMoveBoard, double[,] queenMoveBoard, HashSet<Indices> BlockingTiles, List<Indices> recentlyChanged, int moveNumber)
    {
        List<Indices> newRecentlyChanged = new List<Indices>();
        foreach (Indices currentLocation in recentlyChanged)
        {
            for (int i_dir = -1; i_dir <= 1; ++i_dir)
            {
                for (int j_dir = -1; j_dir <= 1; ++j_dir)
                {
                    if (i_dir == 0 && j_dir == 0) continue; // such direction doesn't exist

                    int current_i = currentLocation.i, current_j = currentLocation.j;
                    int distance = (int)kingMoveBoard[current_i, current_j] + 1;

                    // in order to make sure we don't try to move at the current location which makes no sense
                    current_i += i_dir; current_j += j_dir;

                    while (0 <= current_i  && current_i < GameBoardInformation.rows
                        && 0 <= current_j && current_j < GameBoardInformation.columns)
                    {
                        if (BlockingTiles.Contains(new Indices(current_i, current_j)) == true)
                        {
                            // if its blocked, no reason to continue.
                            break;
                        }

                        bool hasChanged = false;

                        if (distance < kingMoveBoard[current_i, current_j])
                        {
                            kingMoveBoard[current_i, current_j] = distance;
                            hasChanged = true;
                        }

                        if (moveNumber < queenMoveBoard[current_i, current_j])
                        {
                            queenMoveBoard[current_i, current_j] = moveNumber;
                            hasChanged = true;
                        }

                        if (hasChanged)
                        {
                            newRecentlyChanged.Add(new Indices(current_i, current_j));
                        }

                        // update distance from start, and update the new location
                        ++distance;
                        current_i += i_dir;
                        current_j += j_dir;
                    }
                }
            }
        }

        // we stop when newRecentlyChanged.Count = 0 !
        if (newRecentlyChanged.Count > 0)
            FindShortestRoutesHelper(kingMoveBoard, queenMoveBoard, BlockingTiles, newRecentlyChanged, moveNumber + 1);
    }

    private static Tuple<double[,], double[,]> FindMinimumIntersection(List<Tuple<double[,], double[,]>> boards)
    {
        int n = boards[0].Item1.GetLength(0);
        int m = boards[0].Item1.GetLength(1);
        double[,] board1 = new double[n, m];
        double[,] board2 = new double[n, m];

        for (int i = 0; i < n; ++i)
        {
            for (int j = 0; j < m; ++j)
            {
                board1[i, j] = double.PositiveInfinity;
                board2[i, j] = double.PositiveInfinity;
            }
        }

        foreach (Tuple<double[,], double[,]> kingAndqueenBoard in boards)
        {
            double[,] kingBoard = kingAndqueenBoard.Item1;
            double[,] queenBoard = kingAndqueenBoard.Item2;
            for (int i = 0; i < n; ++i)
            {
                for (int j = 0; j < m; ++j)
                {
                    if (kingBoard[i, j] < board1[i, j]) board1[i, j] = kingBoard[i, j];
                    if (queenBoard[i, j] < board2[i, j]) board2[i, j] = queenBoard[i, j];
                }
            }
        }

        return new Tuple<double[,], double[,]>(board1, board2);
    }

    // source: https://stackoverflow.com/questions/5132397/fast-way-to-convert-a-two-dimensional-array-to-a-list-one-dimensional
    private static Tuple<double[], double[]> TupleTwoDimensionalToTupleOneDimensional(Tuple<double[,], double[,]> TupleTwoDimensional)
    {
        // first item to 1D array
        double[] tmp1 = new double[TupleTwoDimensional.Item1.GetLength(0) * TupleTwoDimensional.Item1.GetLength(1)];
        Buffer.BlockCopy(TupleTwoDimensional.Item1, 0, tmp1, 0, tmp1.Length * sizeof(double));

        // second item to 1D array
        double[] tmp2 = new double[TupleTwoDimensional.Item2.GetLength(0) * TupleTwoDimensional.Item2.GetLength(1)];
        Buffer.BlockCopy(TupleTwoDimensional.Item2, 0, tmp2, 0, tmp2.Length * sizeof(double));

        return new Tuple<double[], double[]>(tmp1, tmp2);
    }

    // TODO: can be threaded! (can have 3 parts of the sum and then sum them together)
    private static double sum_of_deltas(double[] listOf_D_i_1, double[] listOf_D_i_2)
    {
        double sum = 0;
        for (int from = 0, until = listOf_D_i_1.Length; from < until; ++from)
        {
            if (listOf_D_i_1[from] == double.PositiveInfinity) continue;
            sum += delta(listOf_D_i_1[from], listOf_D_i_2[from]);
        }
        return sum;
    }

    // it was suggested that K should satisfy: K <=0.2.
    // garauntees a random value of K s.t: 0.1 <= |K| <= 0.2
    private static double K = (new System.Random().NextDouble()*0.1 + 0.1)*(new System.Random().NextDouble() < 0.5 ? 1 : -1);
    private static double delta(double di1a, double di2a)
    {
        if (di1a == di2a) return K;
        if (di1a < di2a) return 1;
        return -1;
    }

    private static Tuple<double, double> GetCs(
        double[] listOfAll_D1_1,
        double[] listOfAll_D1_2,
        double[] listOfAll_D2_2,
        double[] listOfAll_D2_1
        )
    {

        double c1 = 0;
        var thread1 = new Thread(() =>
        {
            for (int i = 0, until = listOfAll_D1_1.Length; i < until; ++i)
            {
                if (listOfAll_D1_1[i] == double.PositiveInfinity) continue;
                c1 += GetDifferenceBetween(Math.Pow(2, -listOfAll_D1_1[i]), Math.Pow(2, -listOfAll_D1_2[i]));
            }
            c1 = 2 * c1;
        });


        double c2 = 0;
        var thread2 = new Thread(() =>
        {
            for (int i = 0, until = listOfAll_D2_2.Length; i < until; ++i)
            {
                double diff = GetDifferenceBetween(listOfAll_D2_2[i], listOfAll_D2_1[i]);
                c2 += Math.Min(1.0, Math.Max(-1, diff / 6));
            }
        });

        thread1.Start();
        thread2.Start();
        thread1.Join();
        thread2.Join();

        return new Tuple<double, double>(c1, c2);
    }

    private static double GetDifferenceBetween(double a, double b)
    {
        if (a == double.PositiveInfinity)
        {
            if (b == double.PositiveInfinity)
            {
                return 0;
            }
            else
            {
                return a - b;
            }
        }
        else if (a == double.NegativeInfinity)
        {
            if (b == double.NegativeInfinity)
            {
                return 0;
            }
            else
            {
                return a - b;
            }
        }
        else
        {
            return a - b;
        }
    }

    // important: f1+f2+f3+f4 = 1.0, fi>=0.
    private static Tuple<double, double, double, double> GetFs(int numberOfMoves)
    {
        // TODO: use w.
        double f1, f2, f3, f4;
        if (numberOfMoves >= GameBoardInformation.ManyMoves) // num of moves very high
        {
            f1 = 0.8;
            f2 = 0.05;
            f3 = 0.05;
        }
        else if (numberOfMoves >= GameBoardInformation.ModerateMoves) // num of moves very low
        {
            f1 = 0.2;
            f2 = 0.35;
            f3 = 0.35;
        }
        else // num of moves are very low
        {
            f1 = 0.1;
            f2 = 0.1;
            f3 = 0.1;
        }
        f4 = 1 - f1 - f2 - f3;

        return new Tuple<double, double, double, double>(f1, f2, f3, f4);
    }



    private Piece Winner()
    {
        bool doesWhiteHaveFreeQueen = false;
        foreach (Indices indices in WhiteQueens)
        {
            if (isQueenSurrounded(indices.i, indices.j) == false)
            {
                doesWhiteHaveFreeQueen = true;
                break;
            }
        }

        bool doesBlackHaveFreeQueen = false;
        foreach (Indices indices in BlackQueens)
        {
            if (isQueenSurrounded(indices.i, indices.j) == false)
            {
                doesBlackHaveFreeQueen = true;
            }
        }

        if (doesWhiteHaveFreeQueen == true)
        {
            if (doesBlackHaveFreeQueen == false) return Piece.WHITEQUEEN;
            else return Piece.EMPTY;
        }
        else
        {
            if (doesBlackHaveFreeQueen == true) return Piece.BLACKQUEEN;
            else return Piece.EMPTY;
        }
    }

    // Note: Assumes that i,j is a queen
    private bool isQueenSurrounded(int i, int j)
    {
        for (int k = -1; k <= 1; ++k)
        {
            for (int m = -1; m <= 1; ++m)
            {
                if (!WhiteQueens.Contains(new Indices(i + k, j + m)) &&
                    !BlackQueens.Contains(new Indices(i + k, j + m)) &&
                    !BurnedTiles.Contains(new Indices(i + k, j + m)))
                {
                    return false;
                }
            }
        }
        return true;
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
        if (currentState.depth >= GameBoardInformation.ManyMoves)
        {
            currentState.ExpandDFS(3);
        }
        else if (currentState.depth >= GameBoardInformation.ModerateMoves)
        {
            currentState.ExpandDFS(2);
        }
        else
        {
            currentState.Expand();
        }

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
