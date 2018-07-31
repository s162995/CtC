using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class CatAgent : Agent
{
    public CatAgent(char id, Location loc) : base(id, loc)
    {
        planList = new List<Plan>()
        {
            new CatPlan.Flee(),
            new CatPlan.Wait(),
            new CatPlan.Search(),
            new CatPlan.Eat(),
        };

        goalList = new List<IGoal>()
        {
            new CatGoal.Flee(),
            new CatGoal.Wait(),
            new CatGoal.Eat(),
            new CatGoal.Search()
        };
    }

    protected override Intention Deliberate(Beliefs b, Intention i)
    {
        foreach (IGoal g in goalList)
        {
            if (g.HasReasonFor(new Subject(this), b, i))
                return g.ExtractIntention(b, new Subject(this));
        }

        return new Intention("none");
    }

    protected override Queue<Location> Plan(Beliefs b, Intention i)
    {
        Queue<Location> plan = new Queue<Location>();
        foreach (Plan p in planList)
        {
            if (p.IsPlanFor(i))
            {
                plan = p.ExtractPlan(new Subject(this), i, b, Path);
                break;
            }
        }

        Debug.Log("(" + ID + ")" + " " + i.Name + " " + i.ID);

        if (b.Rooms.Count == 0)
            b.Rooms = Util.CloneAllRooms();

        float t = (1f / (float)Manager.Board.simulationSpeed);

        // Set the eating or waiting timer.
        if (i.Name == "eat")
            Manager.Board.StartCoroutine(Count(t *= Manager.Board.eatTime));
        else if (i.Name == "wait")
            Manager.Board.StartCoroutine(Count(t *= Manager.Board.waitTime));

        // When fleeing forget the location of food objects seen.
        if (i.Name == "flee")
        {
            b.Foods.Clear();
            ExecTime = (int)Speed.VeryFast;
        }
        else
            ExecTime = (int)Speed.Medium;

        return plan;
    }

    protected override bool Succeeded(Intention i, Beliefs b)
    {
        // If eating and counter is done, remove food object from level.
        if (i.Name == "eat")
        {
            if (counter < 0.1f)
            {
                b.Foods.Remove(Location);
                Manager.Instance.RemoveObject(Location);

                return true;
            }
        }

        if (i.Name == "wait")
        {
            if (counter < 0.1f)
                return true;
        }

        // Searching succeedes if room has been explored.
        if (i.Name == "search" && !b.Rooms.ContainsKey(i.ID))
            return true;

        return false;
    }

    protected override bool Reconsider(Intention i, Beliefs b)
    {
        // If not currently fleeing from the nearest child, reconsider intention.
        if ((i.Name == "flee")
            && (b.Sees.Agents.Count > 0 || b.Hears.Agents.Count > 0))
        {
            List<Subject> perceived = Util.Merge(b.Hears.Agents, b.Sees.Agents).Values.ToList();

            if (i.ID != Util.NearestAgent(perceived.ToList(), Location).ID)
                return true;
        }

        // If hears or sees a child agent and not currently fleeing or waiting,
        // reconsider current intention.
        if ((b.Sees.Agents.Count > 0 || b.Hears.Agents.Count > 0)
            && !(i.Name == "wait" || i.Name == "flee"))
        {
            return true;
        }

        // If not waiting or fleeing and sees food and the current intention
        // is not to eat, reconsider intention.
        if (!(i.Name == "wait" || i.Name == "flee")
            && i.ID != 'F'
            && b.Sees.Foods.Count > 0)
        {
            return true;
        }

        // If current intention is nothing and all rooms are searched, but food
        // remains, re-fill rooms and reconsider intention.
        if ((i.Name == "none")
            && b.Rooms.Count == 0
            && Manager.Board.Foods.Count > 0)
        {
            b.Rooms = Manager.Board.Rooms.ToDictionary(x => x.Key, x => x.Value.Clone());

            return true;
        }

        // If waiting and hears a child or sees a child in close proximity,
        // reconsider current intention.
        if (i.Name == "wait")
        {
            Subject nearest = Util.NearestAgent(b.Sees.Agents.Values.ToList(), Location);

            if (((b.Sees.Agents.Count > 0 && Util.L1Distance(Location, nearest.Location) < 3)
            || b.Hears.Agents.Count > 0))
            {
                return true;
            }
        }

        return false;
    }

    protected override bool Sound(Queue<Location> plan, Intention i, Beliefs b)
    {
        // If exploring and the entire path is within FOV, this area of the room
        // is already searched so re-plan.
        if (i.Name == "explore")
        {
            List<Location> intersect = plan.Intersect(b.Sees.Locations).ToList();
            if (intersect.Count == plan.Count)
                return false;
        }

        // Re-plan if an obstacle is discovered which is located on the current
        // planned path.
        foreach (Location loc in plan)
        {
            if (b.Obstacles.ContainsKey(loc))
                return false;
        }

        return true;
    }
}