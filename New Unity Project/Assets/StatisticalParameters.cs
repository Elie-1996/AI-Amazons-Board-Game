using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;

public static class TechnicalStatistics
{
    public static int TotalCreatedNodes = 1; // counts every node thats been created
    public static int TotalWouldBeNodes = 1; // counts every node thats been created or skipped
    public static int TotalDepth = 1;
    public static int LocalDepth = 0;
    public static int AlphaBetaPruning = 0;
    public static int PrunedDepth = 0;
    public static int PrunedSiblings = 0;
    public static int ThreadAmount = 1;
    public static int ConcurrentThreads = 1;
    public static int MaxConcurrentThreads = 1;
    public static string LastMoveString = "";
    public static double totalTimePassed = 0.0;
    public static double timePassedLastTime = 50.0;
    public static double LastHeuristic = 0.0;
    public static double UltimateHeuristic = 0.0;

    public static string GetLastMoveString(GameState state, double seconds)
    {
        timePassedLastTime = seconds;
        string str = "";
        str += state.parentAndMove.Item2.ToPrintableString();
        str += "/" + string.Format("{0:0.00}", StateEvaluation(state));
        str += "/" + string.Format("{0:0.00}", seconds);
        return str;
    }

    private static double StateEvaluation(GameState state)
    {
        Tuple<int, int> tmp = GameState.GetAmountOfPossibleMovesForBothParties(state.WhiteQueens, state.BlackQueens, state.BurnedTiles);
        int white = tmp.Item1;
        int black = tmp.Item2;
        return 0.5*(white - black);
    }

    public static string ParametersAsText()
    {
        string str = "";
        str += "----------------------------------------------------";
        str += "\n";
        str += "Move #" + GameTree.head.depth;
        str += "\n";
        str += "Time Left: " + string.Format("{0:0.00}", (InitializingParameters.time - TechnicalStatistics.totalTimePassed)) + "sec";
        str += "\n";
        str += "Move: " + TechnicalStatistics.LastMoveString;
        str += "\n";
        str += "Total Tree Depth: " + TechnicalStatistics.TotalDepth;
        str += "\n";
        str += "Local Tree Depth: " + TechnicalStatistics.LocalDepth;
        str += "\n";
        str += "Alpha Beta Pruning: " + TechnicalStatistics.AlphaBetaPruning;
        str += "\n";
        str += "Sibling Pruning Percentage: " + string.Format("{0:0.00}", 100.0 * (((double)TechnicalStatistics.PrunedSiblings) / TechnicalStatistics.TotalWouldBeNodes)) + "%";
        str += "\n";
        str += "Depth Pruning Percentage: " + string.Format("{0:0.00}", 100.0 * (((double)TechnicalStatistics.PrunedDepth) / TechnicalStatistics.TotalWouldBeNodes)) + "%";
        str += "\n";
        str += "All Threads Opened (In Last Move): " + TechnicalStatistics.ThreadAmount;
        str += "\n";
        str += "Concurrent Threads Opened (In Last Move): " + TechnicalStatistics.MaxConcurrentThreads;
        str += "\n";
        str += "Nodes Created: " + TechnicalStatistics.TotalCreatedNodes;
        str += "\n";
        str += "Nodes Created+Ignored: " + TechnicalStatistics.TotalWouldBeNodes;
        str += "\n";
        str += "The Main Variant: " + string.Format("{0:0.00}", TechnicalStatistics.LastHeuristic);
        str += "\n";
        str += "The Best Heuristic: " + string.Format("{0:0.00}", TechnicalStatistics.UltimateHeuristic);
        str += "\n";
        return str;
    }
}

public class StatisticalParameters : MonoBehaviour
{
    public Text TimeLeft;
    public Text LastMove;
    public Text TreeDepth;
    public Text LocalDepth;
    public Text AlphaBetaPruning;
    public Text SiblingPruning;
    public Text DepthPruning;
    public Text Threads;
    public Text ConcurrentThreads;
    public Text TotalNodesCreated;
    public Text TotalNodesIgnored;
    public Text HeuristicValue;
    public Text UltimateValue;
    public Text WinnerValue;

    // Start is called before the first frame update
    void Start()
    {
        TimeLeft.text = "Time Left: ";
        LastMove.text = "Move: ";
        TreeDepth.text = "Total Tree Depth: ";
        LocalDepth.text = "Local Tree Depth: ";
        AlphaBetaPruning.text = "Alpha Beta Pruning: ";
        SiblingPruning.text = "Sibling Pruning Percentage: ";
        DepthPruning.text = "Depth Pruning Percentage: ";
        Threads.text = "Threads Amount: ";
        ConcurrentThreads.text = "Concurrent Threads Opened (In Last Move): ";
        TotalNodesCreated.text = "Nodes Created: ";
        TotalNodesIgnored.text = "Nodes Created+Ignored: ";
        HeuristicValue.text = "The Main Heuristic: ";
        UltimateValue.text = "The Best Heuristic: ";
        WinnerValue.text = "Winner: ";
        WinnerValue.transform.localScale = new Vector3(0, 0, 0);
    }

    // Update is called once per frame
    void Update()
    {
        TimeLeft.text = "Time Left: " + string.Format("{0:0.00}", (InitializingParameters.time - TechnicalStatistics.totalTimePassed)) + "sec";
        LastMove.text = "Move: " + TechnicalStatistics.LastMoveString;
        TreeDepth.text = "Total Tree Depth: " + TechnicalStatistics.TotalDepth;
        LocalDepth.text = "Local Tree Depth: " + TechnicalStatistics.LocalDepth;
        AlphaBetaPruning.text = "Alpha Beta Pruning: " + TechnicalStatistics.AlphaBetaPruning;
        SiblingPruning.text = "Sibling Pruning Percentage: " + string.Format("{0:0.00}", 100.0 * (((double)TechnicalStatistics.PrunedSiblings) / TechnicalStatistics.TotalWouldBeNodes)) + "%";
        DepthPruning.text = "Depth Pruning Percentage: " + string.Format("{0:0.00}", 100.0 * (((double)TechnicalStatistics.PrunedDepth) / TechnicalStatistics.TotalWouldBeNodes)) + "%";
        Threads.text = "All Threads Opened (In Last Move): " + TechnicalStatistics.ThreadAmount;
        ConcurrentThreads.text = "Concurrent Threads Opened (In Last Move): " + TechnicalStatistics.MaxConcurrentThreads;
        TotalNodesCreated.text = "Nodes Created: " + TechnicalStatistics.TotalCreatedNodes;
        TotalNodesIgnored.text = "Nodes Created+Ignored: " + TechnicalStatistics.TotalWouldBeNodes;
        HeuristicValue.text = "The Main Variant: " + string.Format("{0:0.00}", TechnicalStatistics.LastHeuristic);
        UltimateValue.text = "The Best Heuristic: " + string.Format("{0:0.00}", TechnicalStatistics.UltimateHeuristic);
        Piece Winner = GameBoardInformation.GetWinner();
        WinnerValue.text = "Winner: " + Winner;
        if (Winner != Piece.EMPTY)
            WinnerValue.transform.localScale = new Vector3(1, 1, 1);
    }
}


public static class WriteTextFile
{
    private static List<string> MoveList = new List<string>();

    // this function should be called only at the end of the game, where it will write
    // all the moves to the files
    public static void Write()
    {
        MoveList.Add("Winner: " + GameBoardInformation.GetWinner());
        string[] lines = MoveList.ToArray();
        System.IO.File.WriteAllLines(@".\GameMoves.txt", lines);
    }

    // this function should be called at the end of every move, and AFTER all TechnicalStatistics have been updated, otherwise it will not output correct values
    public static void AddMove()
    {
        string str = TechnicalStatistics.ParametersAsText();
        MoveList.Add(str);
    }
}