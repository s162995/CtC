using System.Collections.Generic;
using System;
using UnityEngine;

public static class Planner
{
    // Initiates a search and assigns the appropriate strategy
    // and evaluation method.
    public static Queue<Location> Search(Node n)
    {
        Queue<Location> q = new Queue<Location>();
        Evaluation e = null;
        Strategy s = null;

        Type t = n.GetType();
        if (t.Equals(typeof(PathNode)))
        {
            e = new Evaluation.AStar(n);
            s = new Strategy.BestFirst(e);
            q = new Queue<Location>(SearchUtil(n, s));
        }
        else if (t.Equals(typeof(FlankNode)))
        {
            e = new Evaluation.AStar(n);
            s = new Strategy.BestFirst(e);
            q = new Queue<Location>(SearchUtil(n, s));
        }
        else if (t.Equals(typeof(FleeNode)))
        {
            e = new Evaluation.Greedy(n);
            s = new Strategy.BestFirst(e);
            q = new Queue<Location>(SearchUtil(n, s));
        }
        else if (t.Equals(typeof(PursueNode)))
        {
            e = new Evaluation.Greedy(n);
            s = new Strategy.BestFirst(e);
            q = new Queue<Location>(SearchUtil(n, s));
        }
        else if (t.Equals(typeof(SoundNode)))
        {
            s = new Strategy.BreadthFirst();
            q = new Queue<Location>(SearchUtil(n, s));
        }

        return q;
    }

    // The search algorithm.
    private static List<Location> SearchUtil(Node initState, Strategy strategy)
    {
        strategy.AddToFrontier(initState);

        int i = 0;

        while (!strategy.FrontierEmpty() && i < 1000)
        {
            i++;

            Node leaf = strategy.DequeueLeaf();

            if (leaf.IsGoalState())
                return strategy.ExtractPlan();

            strategy.Explored.Add(leaf);

            foreach (Node child in leaf.GetExpandedNodes())
            {
                if (!strategy.Explored.Contains(child) && !strategy.InFrontier(child))
                    strategy.AddToFrontier(child);
            }
        }

        Debug.Log("No solution found!");
        return new List<Location>();
    }
}