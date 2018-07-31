using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlankNode : PathNode
{
    protected Location avoid;

    public FlankNode(FlankNode parent, Location dest, Location avoid) : base(parent, dest)
    {
        G = (parent == null) ? 0 : (parent.G + 1);
        this.avoid = avoid;
        this.dest = dest;
    }

    // The Flank node heuristic takes both proximity to location to be avoided
    // and proximity to goal location into account.
    public override double H
    {
        get
        {
            double d = 100f;

            d -= (Util.EuclideanDistance(CurLocation, avoid) * 6f);
            d += base.H;

            return d;
        }
    }

    public override bool IsGoalState()
    {
        return CurLocation.Equals(dest);
    }

    public override Node ChildNode(Location loc)
    {
        FlankNode n = new FlankNode(this, dest, avoid)
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

        FlankNode other = (FlankNode)obj;

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