using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System.Collections;

public sealed class Manager
{
    private static readonly Manager instance = new Manager();
    public static Manager Instance { get { return instance; } }
    public static Board Board { get; private set; }

    private Manager()
    {
        Board = Board.Instance;
    }

    public void CreateBoard(char[,] layout)
    {
        Board.CreateBoard(layout);
        Board.CreateAgents();
    }

    // Initiates the agents and the win checker.
    public void Run()
    {
        List<Coroutine> loops = new List<Coroutine>();

        foreach (Agent agt in Board.Agents.Values)
            loops.Add(Board.StartCoroutine(agt.Loop()));

        if (!ReferenceEquals(Board.Cat, null) && Board.Children.Count > 0)
            Board.StartCoroutine(CheckGameState(loops));
    }

    // Check for win states.
    private IEnumerator CheckGameState(List<Coroutine> routines)
    {
        while (true)
        {
            yield return new WaitForEndOfFrame();

            foreach (ChildAgent child in Board.Children.Values)
            {
                if (child.Path.Count == 0 && Board.Cat.Path.Count == 0)
                    continue;

                Location childLoc = child.Location;
                Location catLoc = Board.Cat.Location;
                Location lastChildLoc = child.Path[child.Path.Count - 1];
                Location lastCatLoc = Board.Cat.Path[Board.Cat.Path.Count - 1];

                if ((childLoc == catLoc
                        || (lastChildLoc == catLoc && lastCatLoc == childLoc))
                            || Board.Foods.Count == 0)
                {
                    foreach (Coroutine loop in routines)
                        Board.StopCoroutine(loop);

                    if (Board.Foods.Count > 0)
                    {
                        Debug.Log("Children Win At Location " + catLoc.ToString());
                        Board.Destroy(Board.AgentPieces['0']);
                    }
                    else
                    {
                        List<char> idList = Board.Children.Keys.ToList();
                        foreach (char id in idList)
                            Board.Destroy(Board.AgentPieces[id]);

                        Debug.Log("Cat Wins!");
                    }

                    yield break;
                }
            }
        }
    }

    // Remove food object at location
    public void RemoveObject(Location loc)
    {
        Board.Foods.Remove(loc);
        GameObject.Destroy(Board.ObjectPieces[loc]);
        Board.ObjectPieces.Remove(loc);
    }

    // public void DisplayObject(Location loc)
    // {
    //     GameObject go = Board.ObjectPieces[loc];
    //     Util.SetAlpha(go, 1f);
    // }
}