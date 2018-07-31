using System.Collections.Generic;

public class Beliefs
{
    public Dictionary<char, Subject> Agents { get; private set; }
    public Dictionary<Location, Obstacle> Obstacles { get; private set; }
    public Dictionary<Location, Food> Foods { get; private set; }
    public Dictionary<char, Room> Rooms { get; set; }
    public VisionPercept Sees { get; set; }
    public AudioPercept Hears { get; set; }
    public Dictionary<char, Beliefs> ToM { get; set; }

    public Beliefs()
    {
        Agents = new Dictionary<char, Subject>();
        Obstacles = new Dictionary<Location, Obstacle>();
        Foods = new Dictionary<Location, Food>();
        Rooms = new Dictionary<char, Room>();
        Sees = new VisionPercept();
        Hears = new AudioPercept();
        ToM = new Dictionary<char, Beliefs>();
    }

    // This method receives a vision percept which updates the
    // agent's beliefs about its surroundings.
    public void Update(VisionPercept vp)
    {
        Sees = vp;

        List<char> aIDs = new List<char>(vp.Agents.Keys);
        foreach (char id in aIDs)
            Agents[id] = vp.Agents[id];

        List<Location> fLocs = new List<Location>(vp.Foods.Keys);
        foreach (Location loc in fLocs)
            Foods[loc] = vp.Foods[loc];

        List<Location> oLocs = new List<Location>(vp.Obstacles.Keys);
        foreach (Location loc in oLocs)
            Obstacles[loc] = vp.Obstacles[loc];

        List<char> rIDs = new List<char>(Rooms.Keys);
        foreach (char id in rIDs)
        {
            Rooms[id].ExploreTiles(vp.Locations);

            if (Rooms[id].IsExplored())
                Rooms.Remove(id);
        }
    }

    // Updates the beliefs according to what the agent hears.
    public void Update(AudioPercept ap)
    {
        Hears = ap;
    }
}