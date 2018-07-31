using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Obstacle {

    public Location Location { get; set; }

    public Obstacle(Location loc)
    {
        this.Location = loc;
    }

    public double DistanceTo(Location loc)
    {
        return Util.L1Distance(loc, this.Location);
    }

    public override bool Equals(object obj)
    {

        if (ReferenceEquals(null, obj))
            return false;

        if (ReferenceEquals(this, obj))
            return true;

        if (obj.GetType() != this.GetType())
            return false;

        Obstacle other = obj as Obstacle;

        return !System.Object.ReferenceEquals(null, Location)
        && System.Object.ReferenceEquals(Location, other.Location);
    }

    public static bool operator ==(Obstacle i1, Obstacle i2)
    {
        return i1.Equals(i2);
    }

    public static bool operator !=(Obstacle i1, Obstacle i2)
    {
        return !(i1 == i2);
    }

    public override int GetHashCode()
    {
        int hash = 13;
        hash = (hash * 7) + (!System.Object.ReferenceEquals(null, Location) ? Location.GetHashCode() : 0);
        return hash;
    }
}