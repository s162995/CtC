using UnityEngine;

public class Location
{
    public int Row { get; set; }
    public int Col { get; set; }

    public Location() { }

    public Location(int row, int col)
    {
        Row = row;
        Col = col;
    }

    public Location(Location loc)
    {
        Row = loc.Row;
        Col = loc.Col;
    }

    public Vector2 ToVector()
    {
        return new Vector2(Col, -Row);
    }

    public override string ToString()
    {
        return "(" + Row + ", " + Col + ")";
    }

    public double DistanceTo(Location loc)
    {
        return Util.L1Distance(this, loc);
    }

    public override bool Equals(object obj)
    {

        if (ReferenceEquals(null, obj))
            return false;

        if (ReferenceEquals(this, obj))
            return true;

        if (obj.GetType() != this.GetType())
            return false;

        Location other = obj as Location;

        return Col == other.Col
            && Row == other.Row;
    }

    public static bool operator ==(Location l1, Location l2)
    {
        return l1.Equals(l2);
    }

    public static bool operator !=(Location l1, Location l2)
    {
        return !(l1 == l2);
    }

    public override int GetHashCode()
    {
        int hash = 13;
        hash = (hash * 7) + Row.GetHashCode();
        hash = (hash * 7) + Col.GetHashCode();
        return hash;
    }
}