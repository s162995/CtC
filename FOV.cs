using System.Collections.Generic;
using UnityEngine;

public class FOV
{

    public FOV() { }

    // Method to get the fov shared by (intersection of) two agents.
    public static HashSet<Location> GetSharedFov(Subject s1, Subject s2)
    {
        HashSet<Location> fov1 = new HashSet<Location>();
        HashSet<Location> fov2 = new HashSet<Location>();
        fov1 = GetFov(s1.Direction, s1.Location);
        fov2 = GetFov(s2.Direction, s2.Location);
        fov1.IntersectWith(fov2);

        return fov1;
    }

    public static HashSet<Location> GetFov(Vector2 dir, Location loc)
    {
        if (dir == Vector2.right)
            return CalculateFov(loc, 0, 1);

        if (dir == Vector2.left)
            return CalculateFov(loc, 0, -1);

        if (dir == Vector2.up)
            return CalculateFov(loc, -1, 0);

        if (dir == Vector2.down)
            return CalculateFov(loc, 1, 0);

        return new HashSet<Location>();
    }

    // Calculates the FOV of an agent as explained in the thesis.
    private static HashSet<Location> CalculateFov(Location agtLoc, int rowOffset, int colOffset)
    {
        HashSet<Location> locs = new HashSet<Location>();
        Location loc = null;
        int newCol = 0;
        int newRow = 0;
        int j;

        for (int i = -1; i <= 1; i++)
        {
            j = 0;

            if (rowOffset == 0)
            {
                newRow = agtLoc.Row + i;
                newCol = agtLoc.Col;
            }
            else if (colOffset == 0)
            {
                newCol = agtLoc.Col + i;
                newRow = agtLoc.Row;
            }

            loc = new Location(newRow, newCol);

            while (!Node.Walls.Contains(loc) && j <= 7)
            {
                locs.Add(loc);

                loc = new Location(newRow, newCol);
                newCol += colOffset;
                newRow += rowOffset;
                j++;

                if (i == 0 && (j - 1) == 1 && Node.Walls.Contains(loc))
                    return new HashSet<Location>() { new Location(agtLoc) };
            }
        }

        return locs;
    }
}