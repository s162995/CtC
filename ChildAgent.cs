﻿using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ChildAgent : Agent
{
    public bool IsRecovering { get; set; }
    public bool IsUrgent { get; set; }
    public bool IsRegrouping { get; set; }

    private ToMReasoner tom;

    public ChildAgent(char id, Location loc) : base(id, loc)
    {
        IsRecovering = false;
        IsUrgent = false;
        IsRegrouping = false;
        tom = new ToMReasoner();

        planList = new List<Plan>()
        {
            new ChildPlan.GoTo(),
            new ChildPlan.Track(),
            new ChildPlan.Recover(),
            new ChildPlan.Halt(),
            new ChildPlan.Regroup(),
            new ChildPlan.Capture(),
            new ChildPlan.Ambush(),
            new ChildPlan.Search(),
            new ChildPlan.Assist(),
            new ChildPlan.Flank()
        };

        goalList = new List<IGoal>()
        {
            new ChildGoal.Recover(),
            new ChildGoal.Capture(),
            new ChildGoal.Halt(),
            new ChildGoal.Flank(),
            new ChildGoal.Assist(),
            new ChildGoal.Ambush(),
            new ChildGoal.GoTo(),
            new ChildGoal.Track(),
            new ChildGoal.Search(),
            new ChildGoal.Regroup()
        };
    }

    protected override bool Execute(Queue<Location> plan)
    {
        if (base.Execute(plan))
        {
            // Create sound generated by agent.
            MakeSound();

            return true;
        }

        return false;
    }

    protected override Beliefs BRF(Beliefs b, List<IPercept> p)
    {
        b = base.BRF(b, p);

        // The recursive higher-order observation reasoner updates the agent's
        // higher-order beliefs.
        tom.UpdateToM(this, b, b.Sees.Locations, 3);

        return b;
    }

    protected override Intention Deliberate(Beliefs b, Intention i)
    {
        Intention intent = new Intention("none");

        foreach (IGoal g in goalList)
        {
            if (g.HasReasonFor(new Subject(this), b, i))
            {
                intent = g.ExtractIntention(b, new Subject(this));

                break;
            }
        }

        return intent;
    }

    protected override Queue<Location> Plan(Beliefs b, Intention i)
    {
        Queue<Location> plan = new Queue<Location>();
        foreach (Plan p in planList)
        {
            if (p.IsPlanFor(i))
            {
                plan = p.ExtractPlan(new Subject(this), i, b, new List<Location>(Path));
                break;
            }
        }

        Debug.Log("(" + ID + ")" + " " + i.Name + " " + i.ID);

        float t = 1f / (float)Manager.Board.simulationSpeed;

        // Initiate countdown timer for recover or ambush.
        if (i.Name == "recover")
            Manager.Board.StartCoroutine(Count(t *= 50));
        else if (i.Name == "ambush")
            Manager.Board.StartCoroutine(Count(t *= 300));

        // Re-fill unexplored rooms if current intention is to capture, assist
        // or track and set execution speed to fast.
        if (i.Name == "capture" || i.Name == "assist" || i.Name == "track")
        {
            b.Rooms = Util.CloneAllRooms();
            ExecTime = (int)Speed.Fast;
        }
        else
            ExecTime = (int)Speed.Medium;

        if (i.Name == "recover")
            IsRecovering = true;
        else
            IsRecovering = false;

        if (i.Name == "assist" || i.Name == "flank" || i.Name == "track" 
            || i.Name == "ambush" || i.Name == "capture")
            IsUrgent = true;
        else
            IsUrgent = false;

        if (i.Name == "regroup")
            IsRegrouping = true;
        else
            IsRegrouping = false;

        return plan;
    }

    protected override bool Succeeded(Intention i, Beliefs b)
    {
        if (i.Name == "recover")
        {
            if (counter < 0.1f)
                return true;
        }

        // Halt is successful if cat is no longer visible.
        if (i.Name == "halt")
        {
            if (!b.Sees.Agents.ContainsKey('0'))
                return true;
        }

        // Track is successful if the cat is spotted.
        if (i.Name == "track")
        {
            if (b.Sees.Agents.ContainsKey('0'))
                return true;
        }

        // Flank is successful if the other child sees this child agent.
        if (i.Name == "flank")
        {
            if (b.ToM[i.ID].Sees.Agents.ContainsKey(ID))
                return true;

            return false;
        }

        // Assist is successful if the cat is spotted.
        if (i.Name == "assist")
        {
            if (b.Sees.Agents.ContainsKey('0'))
                return true;
        }

        // Ambush is successful if the cat is no longer visible an the 
        // waiting timer is done. Re-explore room when done.
        if (i.Name == "ambush")
        {
            if (!b.Sees.Agents.ContainsKey('0') && counter < 0.1f)
            {
                b.Rooms[i.ID] = Util.CloneRoom(i.ID);

                return true;
            }
        }

        // If the cat is spotted or the room is not in the unexplored rooms set,
        // the intention has succeeded.
        if (i.Name == "search")
        {
            if (!b.Rooms.ContainsKey(i.ID) || b.Sees.Agents.ContainsKey('0'))
            {
                return true;
            }
        }

        // If agent hears another agent or sees another agent, regroup is successful.
        if (i.Name == "regroup")
        {
            if (b.Hears.Agents.Count > 0)
            {
                b.Rooms = Util.CloneAllRooms();
                return true;

            }

            if (b.Sees.Agents.Count > 0)
            {
                foreach (char aID in b.Sees.Agents.Keys)
                {
                    if (Util.IsRegrouping(aID) && !Util.IsMoving(aID))
                        continue;

                    b.Rooms = Util.CloneAllRooms();
                    return true;
                }
            }
        }

        return false;
    }

    protected override bool Reconsider(Intention i, Beliefs b)
    {
        // Never reconsider the recover intention (it is a punishment)
        if (i.Name == "recover")
            return false;

        // If currently capturing and no longer sees cat, reconsider intention.
        if (i.Name == "capture")
        {
            if (!b.Sees.Agents.ContainsKey('0'))
                return true;

            return false;
        }

        // If the agent has reason for the listed intention and they are not
        // the current intention, then reconsider the current intention.
        foreach (IGoal g in goalList)
        {
            if (g.HasReasonFor(new Subject(this), b, i))
            {
                string gName = g.ExtractIntention(b, new Subject(this)).Name;
                if (i.Name != gName
                    && (gName == "recover"
                    || gName == "capture"
                    || gName == "halt"
                    || gName == "flank"
                    || gName == "assist"
                    || gName == "ambush"))
                    return true;
            }
        }

        return false;
    }

    protected override bool Sound(Queue<Location> plan, Intention i, Beliefs b)
    {
        // If currently capturing and the cat has moved, re-plan the path to the
        // cat agent.
        if (i.Name == "capture"
            && plan.ToList()[plan.Count - 1] != b.Agents[i.ID].Location)
        {
            return false;
        }

        // If searching and in rooms and the entire planned path is visible in
        // in the agent's FOV, then re-plan the search (or exploration).
        if (i.Name == "search" && Util.InRoom(Location))
        {
            List<Location> intersect = plan.Intersect(b.Sees.Locations).ToList();
            if (intersect.Count == plan.Count)
                return false;
        }

        // If not tracking or recovering and there are dynamic obstacles in the
        // planned path of the agent, then re-plan.
        if (!((i.Name == "track") || IsRecovering))
        {
            foreach (Location loc in plan)
            {
                if (Util.GetDynamicObstacles(ID, Location, b).Contains(loc))
                    return false;
            }
        }

        // Re-plan if there are obstacles in the planned path.
        foreach (Location loc in plan)
        {
            if (b.Obstacles.ContainsKey(loc))
            {
                if (i.Name == "track" || i.Name == "search")
                    i = Deliberate(b, i);

                return false;
            }
        }

        return true;
    }
}