using System.Collections.Generic;

public static class Sight
{
    // Stores and returns the objects within the agents FOV in a percept.
    public static VisionPercept Perceive(char agtID, HashSet<Location> locs)
    {
        VisionPercept p = new VisionPercept
        {
            Locations = new HashSet<Location>(locs)
        };

        foreach (Location loc in locs)
        {
            foreach (char id in Manager.Board.Agents.Keys)
            {
                if (id == agtID)
                    continue;

                if (Util.AgentAt(id, loc))
                    p.Agents[id] = new Subject(id, loc, Manager.Board.Agents[id].Direction);
            }

            if (Util.ObstacleAt(loc))
                p.Obstacles.Add(loc, new Obstacle(loc));

            if (Util.FoodAt(loc))
                p.Foods.Add(loc, new Food(loc));
        }

        return p;
    }
}