using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FleeNode : Node
{
    private Location leave;
    private int maxDist;
    private Location origin;

    public FleeNode(FleeNode parent, Location origin, Location leave, int maxDist) : base(parent)
    {
        G = (parent == null) ? 0 : (parent.G + 1);
        this.maxDist = maxDist;
        this.origin = origin;
        this.leave = leave;
    }

    // The heuristic of the Flee node is calculated as the distance from the
    // location being fled from. The farther away, the lower the cost.
    public override double H
    {
        get
        {
            double d = int.MaxValue;

            return d -= Util.EuclideanDistance(CurLocation, leave);
        }
    }

    public override bool IsGoalState()
    {
        return origin.DistanceTo(CurLocation) >= maxDist;
    }

    public override Node ChildNode(Location loc)
    {
        FleeNode n = new FleeNode(this, origin, leave, maxDist)
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

        FleeNode other = (FleeNode)obj;

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
