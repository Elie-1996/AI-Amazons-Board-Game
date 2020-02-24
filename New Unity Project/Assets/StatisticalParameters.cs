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

    public static string GetLastMoveString(GameState state, double seconds)
    {
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
    }

    // Update is called once per frame
    void Update()
    {
        TimeLeft.text = "Time Left: " + (InitializingParameters.time - TechnicalStatistics.totalTimePassed) + "sec"; // TODO: actually update
        LastMove.text = "Move: " + TechnicalStatistics.LastMoveString; // TODO: actually update
        TreeDepth.text = "Total Tree Depth: " + TechnicalStatistics.TotalDepth;
        LocalDepth.text = "Local Tree Depth: " + TechnicalStatistics.LocalDepth;
        AlphaBetaPruning.text = "Alpha Beta Pruning: " + TechnicalStatistics.AlphaBetaPruning;
        SiblingPruning.text = "Sibling Pruning Percentage: " + string.Format("{0:0.00}", 100.0 * (((double)TechnicalStatistics.PrunedSiblings) / TechnicalStatistics.TotalWouldBeNodes)) + "%";
        DepthPruning.text = "Depth Pruning Percentage: " + string.Format("{0:0.00}", 100.0 * (((double)TechnicalStatistics.PrunedDepth) / TechnicalStatistics.TotalWouldBeNodes)) + "%";
        Threads.text = "All Threads Opened (In Last Move): " + TechnicalStatistics.ThreadAmount;
        ConcurrentThreads.text = "Concurrent Threads Opened (In Last Move): " + TechnicalStatistics.MaxConcurrentThreads;
        TotalNodesCreated.text = "Nodes Created: " + TechnicalStatistics.TotalCreatedNodes;
        TotalNodesIgnored.text = "Nodes Created+Ignored: " + TechnicalStatistics.TotalWouldBeNodes;
    }
}
