using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System;

public class Room {

    public List<Location> UnexploredTiles { get; set; }
    public List<Location> Entrances { get; set; }
    public char ID { get; private set; }

    public Room()
    {
        UnexploredTiles = new List<Location>();
        Entrances = new List<Location>();
    }

    public Room(char symbol) : this()
    {
        ID = symbol;
    }

    public Location RandomTile()
    {
        return Util.RandomElement(UnexploredTiles);
    }

    public double DistanceTo(Location loc)
    {
        return Util.L1Distance(loc, NearestEntrance(loc));
    }

    // Calculate the nearest entrance to the room.
    public Location NearestEntrance(Location loc)
    {
        int bestIndex = 0;

        for (int i = 0; i < Entrances.Count; i++)
        {
            if (Util.L1Distance(loc, Entrances[i])
                < Util.L1Distance(loc, Entrances[bestIndex]))
            {
                bestIndex = i;
            }
        }

        return Entrances[bestIndex];
    }

    public Location NearestTile(Location loc)
    {
        int bestIndex = 0;

        for (int i = 0; i < UnexploredTiles.Count; i++)
        {
            if (Util.L1Distance(loc, UnexploredTiles[i])
                < Util.L1Distance(loc, UnexploredTiles[bestIndex]))
            {
                bestIndex = i;
            }
        }

        return UnexploredTiles[bestIndex];
    }

    public Location FarthestTile(Location loc)
    {
        int bestIndex = 0;

        for (int i = 0; i < UnexploredTiles.Count; i++)
        {
            if (Util.L1Distance(loc, UnexploredTiles[i])
                > Util.L1Distance(loc, UnexploredTiles[bestIndex]))
            {
                bestIndex = i;
            }
        }

        return UnexploredTiles[bestIndex];
    }

    // Removes passed location from unexplored tiles.
    public void ExploreTiles(HashSet<Location> locs)
    {
        foreach(Location loc in locs)
        {
            if (UnexploredTiles.Contains(loc))
                UnexploredTiles.Remove(loc);
        }
    }

    public Location MiddleTile()
    {
        int i = UnexploredTiles.Count / 2;

        return UnexploredTiles[i];
    }

    public bool IsExplored()
    {
        return UnexploredTiles.Count == 0;
    }

    public Room Clone()
    {
        Room r = new Room(ID);
        r.UnexploredTiles = new List<Location>(UnexploredTiles);
        r.Entrances = new List<Location>(Entrances);

        return r;
    }

    public override string ToString()
    {
        return ID.ToString();
    }

    public override bool Equals(object obj)
    {
        if (ReferenceEquals(null, obj))
            return false;

        if (ReferenceEquals(this, obj))
            return true;

        if (obj.GetType() != this.GetType())
            return false;

        Room other = obj as Room;

        return !System.Object.ReferenceEquals(null, UnexploredTiles)
        && ID == other.ID;
    }

    public static bool operator ==(Room r1, Room r2)
    {
        return r1.Equals(r2);
    }

    public static bool operator !=(Room r1, Room r2)
    {
        return !(r1 == r2);
    }

    public override int GetHashCode()
    {
        int hash = 13;
        hash = (hash * 7) + ID.GetHashCode();
        return hash;
    }
}
