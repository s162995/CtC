using System.Collections.Generic;
using System.Collections;
using System.Linq;
using UnityEngine;

public enum Speed
{
    Slow = 16,
    Medium = 8,
    Fast = 6,
    VeryFast = 2
}

public abstract class Agent : Subject
{
    public List<Location> Noise { get; protected set; }
    public List<Location> Path { get; protected set; }
    public AgentPiece Piece { get; set; }
    public int ExecTime { get; protected set; }
    public bool IsMoving { get; protected set; }

    protected float counter;
    protected bool canExecute;
    protected List<Plan> planList;
    protected List<IGoal> goalList;

    public Agent(char id, Location loc) : base(id, loc)
    {
        Noise = new List<Location>();
        Path = new List<Location>();
        IsMoving = true;
        counter = 0f;
        canExecute = true;
        planList = new List<Plan>();
        goalList = new List<IGoal>();
    }

    // The BDI control loop described in the thesis
    public IEnumerator Loop()
    {
        ExecTime = (int)Speed.Medium;
        Manager.Board.StartCoroutine(ExecTimer());

        Beliefs b = new Beliefs();
        b.Rooms = Util.CloneAllRooms();

        Intention i = new Intention("none");

        while (true)
        {
            List<IPercept> p = Perceive();
            b = BRF(b, p);
            i = Deliberate(b, i);
            Queue<Location> plan = Plan(b, i);

            while (!Succeeded(i, b) && plan.Count > 0)
            {
                if (canExecute)
                    Execute(plan);

                p = Perceive();
                b = BRF(b, p);

                if (Reconsider(i, b))
                {
                    i = Deliberate(b, i);
                    plan = Plan(b, i);
                }

                if (!Sound(plan, i, b))
                    plan = Plan(b, i);

                yield return null;
            }

            yield return null;
        }
    }

    // The agent generates audio and vision percept from the locations
    // within the agent's field of view.
    protected List<IPercept> Perceive()
    {
        HashSet<Location> fov = FOV.GetFov(Direction, Location);

        VisionPercept p = Sight.Perceive(ID, fov);
        AudioPercept p2 = Hearing.Perceive(ID, Location);

        Piece.DisplayFeature(fov.ToList(), Feature.Vision);
        Piece.DisplayFeature(fov.Intersect(Manager.Board.Obstacles.Keys).ToList(), Feature.Obstacle);
        Piece.DisplayFeature(fov.Intersect(Manager.Board.Foods.Keys).ToList(), Feature.Food);

        return new List<IPercept>() { p, p2 };
    }

    // The percepts are passed to the belief-revision function, which updates
    // the agent's beliefs.
    protected virtual Beliefs BRF(Beliefs b, List<IPercept> p)
    {
        VisionPercept vp = new VisionPercept();
        AudioPercept ap = new AudioPercept();

        foreach (IPercept ip in p)
        {
            if (ip is VisionPercept)
                vp = (VisionPercept)ip;
            else
                ap = (AudioPercept)ip;
        }

        b.Update(vp);
        b.Update(ap);

        return b;
    }

    // The deliberation process of the BDI loop. Determines the current
    // intentions.
    protected abstract Intention Deliberate(Beliefs b, Intention i);

    // Generates a plan according to the current intention.
    protected abstract Queue<Location> Plan(Beliefs b, Intention i);

    // The current plan is passed to the execution function which
    // executes the head of the plan.
    protected virtual bool Execute(Queue<Location> plan)
    {
        canExecute = false;

        if (!plan.Peek().Equals(Location))
        {
            Location loc = plan.Dequeue() as Location;
            Direction = loc.ToVector() - Location.ToVector();
            Path.Add(Location);
            Location = new Location(loc);

            Piece.Move(Direction);
            Piece.DisplayFeature(plan, Feature.Plan);

            IsMoving = true;

            return true;
        }

        Piece.DisplayFeature(new Queue<Location>(), Feature.Plan);

        IsMoving = false;

        return false;
    }

    // Checks whether the current intention has been succeeded.
    protected abstract bool Succeeded(Intention i, Beliefs b);
    // Checks whether the current intention should be dropped.
    protected abstract bool Reconsider(Intention i, Beliefs b);
    // Checks whether the current plan is sound.
    protected abstract bool Sound(Queue<Location> plan, Intention i, Beliefs b);

    public IEnumerator Count(float seconds)
    {
        counter = seconds;
        while (counter > 0)
        {
            yield return new WaitForSeconds(1);
            counter--;
        }

        counter = 0;
    }

    // Determines the speed at which the agent executes actions.
    private IEnumerator ExecTimer()
    {
        while (true)
        {
            float time = 1f / (float)Manager.Board.simulationSpeed * ExecTime;
            yield return new WaitForSeconds(time);
            canExecute = true;
        }
    }

    // Sound is created as a BFS search.
    protected void MakeSound()
    {
        int depth = 0;

        switch (ExecTime)
        {
            case (int)Speed.Medium:
                depth = 2;
                break;
            case (int)Speed.Fast:
                depth = 3;
                break;
        }

        SoundNode n = new SoundNode(null, Location, depth);
        Noise = Planner.Search(n).ToList();

        if (Noise != null)
            Noise.Remove(Location);

        Piece.DisplayFeature(Noise, Feature.Sound);
    }

    public override bool Equals(object obj)
    {
        if (ReferenceEquals(null, obj))
            return false;

        if (ReferenceEquals(this, obj))
            return true;

        if (obj.GetType() != this.GetType())
            return false;

        Agent other = obj as Agent;

        return ID == other.ID;
    }

    public static bool operator ==(Agent a1, Agent a2)
    {
        return a1.Equals(a2);
    }

    public static bool operator !=(Agent a1, Agent a2)
    {
        return !(a1 == a2);
    }

    public override int GetHashCode()
    {
        int hash = 13;
        hash = (hash * 7) + ID.GetHashCode();
        return hash;
    }
}