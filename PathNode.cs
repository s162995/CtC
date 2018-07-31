using UnityEngine;

public class PathNode : Node
{
    protected Location dest;

    public PathNode(PathNode parent, Location dest) : base(parent)
    {
        G = (parent == null) ? 0 : (parent.G + 1);
        this.dest = dest;
    }

    // The heuristic of the Path node is the manhattan-distance
    // from the current location to the target location.
    public override double H
    {
        get 
        {
            return CurLocation.DistanceTo(dest);
        }
    }

    public override bool IsGoalState()
    {
        return CurLocation.Equals(dest);
    }

    public override Node ChildNode(Location loc)
    {
        PathNode n = new PathNode(this, dest)
        {
            CurLocation = loc,
            Obstacles = Obstacles
        };

        return n;
    }

    public override bool Equals(object obj)
    {
        if (ReferenceEquals(null, obj))
            return false;

        if (ReferenceEquals(this, obj))
            return true;

        if (obj.GetType() != this.GetType())
            return false;

        PathNode other = (PathNode)obj;

        return !System.Object.ReferenceEquals(null, CurLocation)
        && CurLocation.Equals(other.CurLocation);
    }

    public override int GetHashCode()
    {
        int hash = 13;
        hash = (hash * 7) + (!System.Object.ReferenceEquals(null, CurLocation) ? CurLocation.GetHashCode() : 0);
        return hash;
    }
}