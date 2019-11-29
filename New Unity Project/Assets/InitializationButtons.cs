using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public static class InitializingParameters
{
    public static int rows = 10;
    public static int columns = 10;
    public static List<Indices> WhiteQueens = new List<Indices>();
    public static List<Indices> BlackQueens = new List<Indices>();
}


public class InitializationButtons : MonoBehaviour
{

    public InputField Rows;
    public InputField Columns;
    public InputField BlackQueenX;
    public InputField BlackQueenY;
    public InputField WhiteQueenX;
    public InputField WhiteQueenY;

    public void StartGame()
    {
        SceneManager.LoadScene("SampleScene");
    }

    public void AddWhiteQueen()
    {
        int x = int.Parse(WhiteQueenX.text);
        int y = int.Parse(WhiteQueenY.text);
        Indices indices = new Indices(x, y);
        if (InitializingParameters.WhiteQueens.Contains(indices) || InitializingParameters.BlackQueens.Contains(indices)) { return; }
        InitializingParameters.WhiteQueens.Add(indices);
        WhiteQueenX.text = "";
        WhiteQueenY.text = "";
    }

    public void AddBlackQueen()
    {
        int x = int.Parse(BlackQueenX.text);
        int y = int.Parse(BlackQueenY.text);
        Indices indices = new Indices(x, y);
        if (InitializingParameters.WhiteQueens.Contains(indices) || InitializingParameters.BlackQueens.Contains(indices)) { return; }
        InitializingParameters.BlackQueens.Add(indices);
        BlackQueenX.text = "";
        BlackQueenY.text = "";
    }

    public void SetRowsAndColumns()
    {
        InitializingParameters.rows = int.Parse(Rows.text);
        InitializingParameters.columns = int.Parse(Columns.text);
    }
}
