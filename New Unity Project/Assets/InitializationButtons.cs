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
    public static int numberOfAIs = 1; // when this is 1, it means AI vs Player, when this is 2, it means AI vs AI.
    public static float time = 1000.0f;
}


public class InitializationButtons : MonoBehaviour
{
    public Toggle StandardBoardSize;
    public Toggle SmallBoardSize;
    public Toggle AgainstSelf;
    public Toggle AgainstPlayer;
    public InputField TimeField;

    public void StartGame()
    {
        SetGameParameters();
        SceneManager.LoadScene("SampleScene");
    }

    private void SetGameParameters()
    {
        if (StandardBoardSize.isOn)
            InitializeStandardBoard();
        else
            InitializeSmallBoard();

        if (AgainstSelf.isOn)
            InitializingParameters.numberOfAIs = 2;
        else
            InitializingParameters.numberOfAIs = 1;

        InitializingParameters.time = int.Parse(TimeField.text);
    }

    private void InitializeStandardBoard()
    {
        InitializingParameters.rows = 10;
        InitializingParameters.columns = 10;

        // Black Queens
        InitializingParameters.BlackQueens.Add(new Indices(0, 3));
        InitializingParameters.BlackQueens.Add(new Indices(0, 6));
        InitializingParameters.BlackQueens.Add(new Indices(3, 0));
        InitializingParameters.BlackQueens.Add(new Indices(3, 9));

        // White Queens
        InitializingParameters.WhiteQueens.Add(new Indices(6, 0));
        InitializingParameters.WhiteQueens.Add(new Indices(6, 9));
        InitializingParameters.WhiteQueens.Add(new Indices(9, 3));
        InitializingParameters.WhiteQueens.Add(new Indices(9, 6));
    }

    private void InitializeSmallBoard()
    {
        InitializingParameters.rows = 6;
        InitializingParameters.columns = 6;

        // Black Queens
        InitializingParameters.BlackQueens.Add(new Indices(0, 3));
        InitializingParameters.BlackQueens.Add(new Indices(5, 2));

        // White Queens
        InitializingParameters.WhiteQueens.Add(new Indices(2, 0));
        InitializingParameters.WhiteQueens.Add(new Indices(3, 5));
    }


    public void StandardBoardSizeToggleChange()
    {
        // CONTROL UI TOGGLE
        SmallBoardSize.isOn = !StandardBoardSize.isOn;
    }

    public void SmallBoardSizeToggleChange()
    {
        // CONTROL UI TOGGLE
        StandardBoardSize.isOn = !SmallBoardSize.isOn;
    }

    public void AgainstSelfToggleChange()
    {
        AgainstPlayer.isOn = !AgainstSelf.isOn;
    }

    public void AgainstPlayerToggleChange()
    {
        AgainstSelf.isOn = !AgainstPlayer.isOn;
    }
}
