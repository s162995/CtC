using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Food
{
    public Location Location { get; set; }
    public int Health { get; set; }

    public Food(Location loc)
    {
        Location = loc;
        Health = 10;
    }

    public void TakeDamage()
    {
        Health--;
    }

    public double DistanceTo(Location loc)
    {
        return Util.L1Distance(loc, Location);
    }

    public override bool Equals(object obj)
    {

        if (ReferenceEquals(null, obj))
            return false;

        if (ReferenceEquals(this, obj))
            return true;

        if (obj.GetType() != this.GetType())
            return false;

        Food other = obj as Food;

        return !System.Object.ReferenceEquals(null, Location)
        && System.Object.ReferenceEquals(Location, other.Location);
    }

    public static bool operator ==(Food i1, Food i2)
    {
        return i1.Equals(i2);
    }

    public static bool operator !=(Food i1, Food i2)
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
