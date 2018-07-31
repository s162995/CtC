using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PursueNode : Node
{
    private Location leave;
    private int maxDist;
    private Location origin;
    private Vector2 targetDir;
    private Vector2 dir;

    public PursueNode(PursueNode parent, Location origin, Location leave, int maxDist, Vector2 targetDir, Vector2 dir) : base(parent)
    {
        G = (parent == null) ? 0 : (parent.G + 1);
        this.maxDist = maxDist;
        this.origin = origin;
        this.leave = leave;
        this.targetDir = targetDir;
        this.dir = dir;
    }

    // The Pursue node is a Flee node which prefers a specified direction.
    public override double H
    {
        get
        {
            double d = int.MaxValue;

            if (dir == targetDir)
                d -= 10f;
            else if (dir == -targetDir)
                d -= 0f;
            else
                d -= 5f;

            return d -= Util.EuclideanDistance(CurLocation, leave);
        }
    }

    public override bool IsGoalState()
    {
        return origin.DistanceTo(CurLocation) >= maxDist;
    }

    public override Node ChildNode(Location loc)
    {
        Vector2 newDir = loc.ToVector() - CurLocation.ToVector();

        PursueNode n = new PursueNode(this, origin, leave, maxDist, targetDir, newDir)
        {
            CurLocation = loc,
            Obstacles = Obstacles,
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

        PursueNode other = (PursueNode)obj;

        return !System.Object.ReferenceEquals(null, CurLocation)
        && CurLocation.Equals(other.CurLocation)
        && dir.Equals(other.dir);
    }

    public override int GetHashCode()
    {
        int hash = 13;
        hash = (hash * 7) + (!System.Object.ReferenceEquals(null, CurLocation) ? CurLocation.GetHashCode() : 0);
        hash = (hash * 7) + (!System.Object.ReferenceEquals(null, dir) ? dir.GetHashCode() : 0);
        return hash;
    }
}
