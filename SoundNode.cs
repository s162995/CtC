public class SoundNode : Node
{
    private int depth;
    private Location origin;

    // A BFS search to generate sound.
    public SoundNode(SoundNode parent, Location origin, int depth) : base(parent)
    {
        this.origin = origin;
        this.depth = depth;

        if (ReferenceEquals(parent, null))
            CurLocation = origin;
    }

    public override bool IsGoalState()
    {
        return origin.DistanceTo(CurLocation) == depth;
    }

    public override Node ChildNode(Location loc)
    {
        SoundNode n = new SoundNode(this, origin, depth);
        n.CurLocation = loc;

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

        SoundNode n = (SoundNode)obj;

        return !System.Object.ReferenceEquals(null, CurLocation)
        && CurLocation.Equals(n.CurLocation);
    }

    public override int GetHashCode()
    {
        int hash = 13;
        hash = (hash * 7) + (!System.Object.ReferenceEquals(null, CurLocation) ? CurLocation.GetHashCode() : 0);
        return hash;
    }
}