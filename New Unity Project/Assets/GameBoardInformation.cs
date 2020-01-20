using System;
using System.Collections.Generic;
using UnityEngine;

public enum Piece
{
    WHITEQUEEN = 0, // white queen must be 0
    BLACKQUEEN = 1, // black queen must be 1
    DESTROYEDTILE = 2,
    EMPTY = 3
}

public enum MaterialIntensity
{
    LIGHT,
    DARK
}

// This class will hold information on which Tiles to update
public class UpdatedTile
{
    public readonly int i, j;
    public readonly Piece piece;
    public UpdatedTile(int _i, int _j, Piece _piece)
    {
        i = _i;
        j = _j;
        piece = _piece;
    }
}

public static class GameBoardInformation
{
    public static bool isGameOver = false;
    public static bool playAgain = false;
    private static Piece[,] board;
    private static Indices[] WhiteQueens;
    private static Indices[] BlackQueens;
    private static HashSet<Indices> BurnedTiles;
    private static Piece Winner;
    public static Queue<UpdatedTile> UIUpdatesQueue; // The elements should only be removed in UI, should only be increased in this class.
    public static int rows
    {
        get => board.GetLength(0);
    }
    public static int columns
    {
        get => board.GetLength(1);
    }


    public static int ManyMoves { get => (int)(rows * columns * GetPercentage(MoveQuantity.MANY)); }
    public static int ModerateMoves { get => (int)(rows * columns * GetPercentage(MoveQuantity.MODERATE)); }
    public static int VeryEarlyInTheGame { get => (int)(rows * columns * GetPercentage(MoveQuantity.FEW)); }

    private enum MoveQuantity
    {
        MANY,
        MODERATE,
        FEW
    }
    private static double GetPercentage(MoveQuantity quantity)
    {
        if (InitializingParameters.numberOfAIs == 2)
        {
            if (rows == 10)
            {
                if (quantity == MoveQuantity.MANY) return 0.45;
                if (quantity == MoveQuantity.MODERATE) return 0.2;
                if (quantity == MoveQuantity.FEW) return 0.1;
            }
            else
            {
                if (quantity == MoveQuantity.MANY) return 0.45;
                if (quantity == MoveQuantity.MODERATE) return 0.2;
                if (quantity == MoveQuantity.FEW) return 0.1;
            }
        }
        else
        {
            if (rows == 10)
            {
                if (quantity == MoveQuantity.MANY) return 0.55;
                if (quantity == MoveQuantity.MODERATE) return 0.25;
                if (quantity == MoveQuantity.FEW) return 0.15;
            }
            else
            {
                if (quantity == MoveQuantity.MANY) return 0.45;
                if (quantity == MoveQuantity.MODERATE) return 0.15;
                if (quantity == MoveQuantity.FEW) return 0.1;
            }
        }

        // should never happen
        return 0.1;
    }


    public static void reset()
    {
        Debug.LogError("Please Implement GameBoardInformation reset!");
        // TODO: Missing Implementation
        // TODO: Please delete (or check if needed) to delete Queue memory.
        // Winner = Piece.EMPTY;
    }

    /* initializes an empty Board */
    public static void InitializeBoard(int rows, int columns, List<Indices> _WhiteQueens, List<Indices> _BlackQueens)
    {
        BurnedTiles = new HashSet<Indices>();
        WhiteQueens = new Indices[_WhiteQueens.Count];
        BlackQueens = new Indices[_BlackQueens.Count];
        Winner = Piece.EMPTY;
        UIUpdatesQueue = new Queue<UpdatedTile>();
        if (rows <= 0 || columns <= 0)
        {
            throw new Exception("Could not initialize GameBoardInformation, rows and columns must be positive integers.");
        }
        board = new Piece[rows, columns];
        for (int i = 0; i < rows; ++i)
        {
            for (int j = 0; j < columns; ++j)
            {
                changeBoardIndices(i, j, Piece.EMPTY);
            }
        }

        int white_queen_i = 0;
        foreach (Indices position in _WhiteQueens)
        {
            changeBoardIndices(position.i, position.j, Piece.WHITEQUEEN);
            WhiteQueens[white_queen_i++] = position;
        }

        int black_queen_i = 0;
        foreach (Indices position in _BlackQueens)
        {
            changeBoardIndices(position.i, position.j, Piece.BLACKQUEEN);
            BlackQueens[black_queen_i++] = position;
        }

        GameTree.Initialize();
    }

    public static MaterialIntensity getPieceIntensity(int i, int j)
    {
        exceptionIfIndicesAreOutOfBounds(i, j, "getPieceIntensity");
        if (i % 2 == 0)
        {
            return j % 2 == 0 ? MaterialIntensity.DARK : MaterialIntensity.LIGHT;
        }
        return j % 2 == 0 ? MaterialIntensity.LIGHT : MaterialIntensity.DARK;
    }

    public static Piece getPieceAt(int i, int j)
    {
        exceptionIfIndicesAreOutOfBounds(i, j, "getPieceAt");
        return board[i, j];
    }

    public static bool movePiece(int i, int j, int destination_i, int destination_j)
    {
        exceptionIfIndicesAreOutOfBounds(i, j, "movePiece (i,j)");
        exceptionIfIndicesAreOutOfBounds(destination_i, destination_j, "movePiece (destination_i, destination_j)");
        if (i == destination_i && j == destination_j) { return false; }
        if (hasMovablePiece(i, j) == false) { return false; }
        if (isMoveLegal(i, j, destination_i, destination_j) == false) { return false; }

        Piece piece = board[i, j];
        changeBoardIndices(i, j, Piece.EMPTY);
        changeBoardIndices(destination_i, destination_j, piece);
        UpdateQueenLocation(piece, i, j, destination_i, destination_j);
        return true;
    }

    private static void UpdateQueenLocation(Piece piece, int old_i, int old_j, int new_i, int new_j)
    {
        if (piece == Piece.BLACKQUEEN)
        {
            for (int i = 0; i < BlackQueens.Length; ++i)
            {
                if (BlackQueens[i].i == old_i && BlackQueens[i].j == old_j)
                {
                    BlackQueens[i] = new Indices(new_i, new_j);
                    break;
                }
            }
        }
        else if (piece == Piece.WHITEQUEEN)
        {
            for (int i = 0; i < WhiteQueens.Length; ++i)
            {
                if (WhiteQueens[i].i == old_i && WhiteQueens[i].j == old_j)
                {
                    WhiteQueens[i] = new Indices(new_i, new_j);
                    break;
                }
            }
        }
    }

    public static bool burnPiece(int queen_i, int queen_j, int burn_i, int burn_j)
    {
        exceptionIfIndicesAreOutOfBounds(queen_i, queen_j, "burnPiece (i,j)");
        exceptionIfIndicesAreOutOfBounds(burn_i, burn_j, "burnPiece (destination_i, destination_j)");
        if (queen_i == burn_i && queen_j == burn_j) { return false; }
        if (isMoveLegal(queen_i, queen_j, burn_i, burn_j) == false) { return false; }
        changeBoardIndices(burn_i, burn_j, Piece.DESTROYEDTILE);
        BurnedTiles.Add(new Indices(burn_i, burn_j));
        return true;
    }

    private static void changeBoardIndices(int i, int j, Piece piece)
    {
        board[i, j] = piece;
        UIUpdatesQueue.Enqueue(new UpdatedTile(i, j, piece));
    }

    public static bool isMoveLegal(int i, int j, int destination_i, int destination_j)
    {
        if (isColumnMove(i, j, destination_i, destination_j) == false &&
            isRowMove(i, j, destination_i, destination_j) == false &&
            isDiagonalMove(i, j, destination_i, destination_j) == false)
            return false;
        if (isMoveBlocked(i, j, destination_i, destination_j) == true) return false;
        return true;
    }

    private static bool isMoveBlocked(int i, int j, int destination_i, int destination_j)
    {
        int i_sign = 1;
        if (i == destination_i) { i_sign = 0; }
        else if (i > destination_i) { i_sign = -1; }

        int j_sign = 1;
        if (j == destination_j) { j_sign = 0; }
        else if (j > destination_j) { j_sign = -1; }
        
        while (i != destination_i || j != destination_j)
        {
            i += i_sign;
            j += j_sign;
            if (board[i, j] != Piece.EMPTY) return true;
        }
        return false;
    }

    private static bool isColumnMove(int i, int j, int destination_i, int destination_j)
    {
        return i == destination_i;
    }

    private static bool isRowMove(int i, int j, int destination_i, int destination_j)
    {
        return j == destination_j;
    }

    private static bool isDiagonalMove(int i, int j, int destination_i, int destination_j)
    {
        return Math.Abs(i - destination_i) == Math.Abs(j - destination_j);
    }

    public static void updateGameOver()
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

        if (doesWhiteHaveFreeQueen == false || doesBlackHaveFreeQueen == false)
        {
            isGameOver = true;
        }
        if (isGameOver)
        {
            Winner = doesBlackHaveFreeQueen == true ? Piece.BLACKQUEEN : Piece.WHITEQUEEN;
        }
    }

    public static Piece GetWinner()
    {
        return Winner;
    }

    // Note: Assumes that i,j is a queen
    private static bool isQueenSurrounded(int i, int j)
    {
        for (int k = -1; k <= 1; ++k)
        {
            for (int m = -1; m <= 1; ++m)
            {
                if (isTileEmptyOrExistent(i + k, j + m))
                {
                    return false;
                }
            }
        }
        return true;
    }

    private static bool isTileEmptyOrExistent(int i, int j)
    {
        if (i < 0 || i >= rows || j < 0 || j >= columns) return false; // non-existent
        return board[i, j] == Piece.EMPTY;
    }

    // Note: This function considers whose turn it is to move pieces.
    private static bool hasMovablePiece(int i, int j)
    {
        exceptionIfIndicesAreOutOfBounds(i, j, "hasMovablePiece");
        if (PlayerLogic.globalTurn == (int)Piece.WHITEQUEEN)
        {
            return board[i, j] == Piece.WHITEQUEEN;
        }
        return board[i, j] == Piece.BLACKQUEEN;
    }

    // checks if [0,0] <= [i,j] < [m, n] 
    private static void exceptionIfIndicesAreOutOfBounds(int i, int j, string source)
    {
        if (i >= rows || i < 0 || j < 0 || j > columns)
        {
            throw new Exception(source + ": Input is out of Boundaries where boardSize is [" + rows + ", " + columns + "] and indices are [" + i + ", " + j + "].");
        }
    }
}
