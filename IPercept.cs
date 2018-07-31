using System.Collections.Generic;

public interface IPercept
{
    HashSet<Location> Locations { get; set; }
}

public class AudioPercept : IPercept
{
    public HashSet<Location> Locations { get; set; }
    public Dictionary<char, Subject> Agents { get; private set; }

    public AudioPercept()
    {
        Agents = new Dictionary<char, Subject>();
    }
}

public class VisionPercept : IPercept
{
    public HashSet<Location> Locations { get; set; }
    public Dictionary<char, Subject> Agents { get; private set; }
    public Dictionary<Location, Obstacle> Obstacles { get; private set; }
    public Dictionary<Location, Food> Foods { get; private set; }

    public VisionPercept()
    {
        Agents = new Dictionary<char, Subject>();
        Obstacles = new Dictionary<Location, Obstacle>();
        Foods = new Dictionary<Location, Food>();
    }
}