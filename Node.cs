using System;
using System.Collections.Generic;
using System.Linq;

public enum Action
{
    None,
    Left,
    Right,
    Up,
    Down
};

public abstract class Node
{

    public Node Parent { get; private set; }
    public static HashSet<Location> Walls { get; set; }
    public static HashSet<Location> Pits { get; set; }
    public Location CurLocation { get; set; }
    public HashSet<Location> Obstacles { get; set; }
    public int G { get; protected set; }
    public virtual double H { get; set; }

    //private static System.Random rnd;

    public Node(Node parent)
    {
        Parent = parent;
        CurLocation = new Location();
        Obstacles = new HashSet<Location>();
        //rnd = new System.Random();    
    }

    // Node expansion method
    public List<Node> GetExpandedNodes()
    {
        List<Node> expandedNodes = new List<Node>();
        Location newLocation = null;

        foreach (Action a in Enum.GetValues(typeof(Action)))
        {
            switch (a)
            {
                case Action.Up:
                    newLocation = new Location(CurLocation.Row - 1, CurLocation.Col);

                    break;
                case Action.Down:
                    newLocation = new Location(CurLocation.Row + 1, CurLocation.Col);

                    break;
                case Action.Left:
                    newLocation = new Location(CurLocation.Row, CurLocation.Col - 1);

                    break;
                case Action.Right:
                    newLocation = new Location(CurLocation.Row, CurLocation.Col + 1);

                    break;
                case Action.None:
                    newLocation = new Location(CurLocation);

                    break;
            }

            if (!Walls.Contains(newLocation) && !Obstacles.Contains(newLocation))
            {
                expandedNodes.Add(ChildNode(newLocation));
            }
        }

        //expandedNodes.OrderBy(item => rnd.Next());

        return expandedNodes;
    }

    // If node has no parent then it is the initial state.
    public bool IsInitialState()
    {
        return Parent == null;
    }

    // Checks if the current state is the goal state.
    public abstract bool IsGoalState();
    // Creates a child node.
    public abstract Node ChildNode(Location loc);
}