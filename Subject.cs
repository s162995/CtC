using UnityEngine;

public class Subject
{
    public char ID { get; set; }
    public Location Location { get; set; }
    public Vector2 Direction { get; set; }

    public Subject() {}

    public Subject(char id)
    {
        ID = id;
    }

    public Subject(Location loc)
    {
        Location = loc;
        Direction = Vector2.zero;
    }

    public Subject(char id, Location loc) : this(id)
    {
        Location = loc;
        Direction = Vector2.zero;
    }

    public Subject(char id, Location loc, Vector2 dir) : this(id, loc)
    {
        Direction = dir;
    }

    public Subject(Subject s)
    {
        ID = s.ID;
        Location = s.Location;
        Direction = s.Direction;
    }

    public override bool Equals(object obj)
    {

        if (ReferenceEquals(null, obj))
            return false;

        if (ReferenceEquals(this, obj))
            return true;

        if (obj.GetType() != this.GetType())
            return false;

        Subject other = obj as Subject;

        return !System.Object.ReferenceEquals(null, Location)
        && ID == other.ID;
        //&& System.Object.ReferenceEquals(Location, other.Location)
        //&& System.Object.ReferenceEquals(Direction, other.Direction);
    }

    public static bool operator ==(Subject i1, Subject i2)
    {
        return i1.Equals(i2);
    }

    public static bool operator !=(Subject i1, Subject i2)
    {
        return !(i1 == i2);
    }

    public override int GetHashCode()
    {
        int hash = 13;
        hash = (hash * 7) + ID.GetHashCode();
        //hash = (hash * 7) + (!System.Object.ReferenceEquals(null, Location) ? Location.GetHashCode() : 0);
        //hash = (hash * 7) + (!System.Object.ReferenceEquals(null, Direction) ? Direction.GetHashCode() : 0);
        return hash;
    }
}