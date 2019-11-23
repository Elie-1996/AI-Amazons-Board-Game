using System;
using System.Collections.Generic;
using UnityEngine;

public enum Piece
{
    WHITEQUEEN,
    BLACKQUEEN,
    DESTROYEDTILE,
    EMPTY
}

public enum MaterialIntensity
{
    LIGHT,
    DARK
}

public class GameBoardInformation
{
    private Piece[,] board;
    public int rows
    {
        get => board.GetLength(0);
    }
    public int columns
    {
        get => board.GetLength(1);
    }

    /* initializes an empty Board */
    public GameBoardInformation(int rows, int columns, List<Vector2> WhiteQueens, List<Vector2> BlackQueens)
    {
        if (rows <= 0 || columns <= 0)
        {
            throw new Exception("Could not initialize GameBoardInformation, rows and columns must be positive integers.");
        }
        board = new Piece[rows, columns];
        for (int i = 0; i < rows; ++i)
        {
            for (int j = 0; j < columns; ++j)
            {
                board[i, j] = Piece.EMPTY;
            }
        }

        foreach (Vector2 position in WhiteQueens)
        {
            board[(int)position.x, (int)position.y] = Piece.WHITEQUEEN;
        }

        foreach (Vector2 position in BlackQueens)
        {
            board[(int)position.x, (int)position.y] = Piece.BLACKQUEEN;
        }
    }

    public MaterialIntensity getPieceIntensity(int i, int j)
    {
        exceptionIfIndicesAreOutOfBounds(i, j, "getPieceIntensity");
        if (i % 2 == 0)
        {
            return j % 2 == 0 ? MaterialIntensity.DARK : MaterialIntensity.LIGHT;
        }
        return j % 2 == 0 ? MaterialIntensity.LIGHT : MaterialIntensity.DARK;
    }

    public Piece getPieceAt(int i, int j)
    {
        exceptionIfIndicesAreOutOfBounds(i, j, "getPieceAt");
        return board[i, j];
    }

    // TODO: Should involve which piece to burn.
    public bool movePiece(int i, int j, int destination_i, int destination_j)
    {
        exceptionIfIndicesAreOutOfBounds(i, j, "movePiece (i,j)");
        exceptionIfIndicesAreOutOfBounds(destination_i, destination_j, "movePiece (destination_i, destination_j)");
        if (i == destination_i && j == destination_j) { return false; }
        if (hasMovablePiece(i, j) == false) { return false; }
        if (isMoveLegal(i, j, destination_i, destination_j)) { return false; }
        
        Piece piece = board[i, j];
        board[i, j] = Piece.EMPTY;
        board[destination_i, destination_j] = piece;
        return true;
    }

    public bool isMoveLegal(int i, int j, int destination_i, int destination_j)
    {
        if (isColumnMove(i, j, destination_i, destination_j) == false &&
            isRowMove(i, j, destination_i, destination_j) == false &&
            isDiagonalMove(i, j, destination_i, destination_j) == false)
            return false;
        if (isMoveBlocked(i, j, destination_i, destination_j) == true) return false;
        return true;
    }

    private bool isMoveBlocked(int i, int j, int destination_i, int destination_j)
    {
        int i_sign = 1;
        if (i == destination_i) { i_sign = 0; }
        else if (i > destination_i) { i_sign = -1; }
        int j_sign = 1;
        if (j == destination_j) { j_sign = 0; }
        else if (j > destination_j) { j_sign = -1; }
        while (i != destination_i || j != destination_j)
        {
            if (board[i, j] == Piece.DESTROYEDTILE) return true;
            i += i_sign;
            j += j_sign;
        }
        return false;
    }

    private bool isColumnMove(int i, int j, int destination_i, int destination_j)
    {
        return i == destination_i;
    }

    private bool isRowMove(int i, int j, int destination_i, int destination_j)
    {
        return j == destination_j;
    }

    private bool isDiagonalMove(int i, int j, int destination_i, int destination_j)
    {
        return Math.Abs(i - destination_i) == Math.Abs(j - destination_j);
    }

    private bool hasMovablePiece(int i, int j)
    {
        exceptionIfIndicesAreOutOfBounds(i, j, "hasMovablePiece");
        return board[i, j] == Piece.WHITEQUEEN || board[i, j] == Piece.BLACKQUEEN;
    }

    // checks if [0,0] <= [i,j] < [m, n] 
    private void exceptionIfIndicesAreOutOfBounds(int i, int j, string source)
    {
        if (i >= rows || i < 0 || j < 0 || j > columns)
        {
            throw new Exception(source + ": Input is out of Boundaries where boardSize is [" + rows + ", " + columns + "] and indices are [" + i + ", " + j + "].");
        }
    }
}
