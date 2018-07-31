using System.Collections.Generic;
using System.Linq;

public interface IGoal
{
    bool HasReasonFor(Subject s, Beliefs b, Intention i);
    Intention ExtractIntention(Beliefs b, Subject s);
}

// The goals available to the child agents.
public abstract class ChildGoal
{
    // If in the same location as another agent and is lower rank, then recover.
    public class Recover : IGoal
    {
        public bool HasReasonFor(Subject s, Beliefs b, Intention i)
        {
            if (i.Name != "recover")
            {
                foreach (Subject subj in b.Sees.Agents.Values)
                {
                    if (subj.Location == s.Location && Util.IsHigherRank(subj.ID, s.ID))
                        return true;
                }
            }

            return false;
        }

        public Intention ExtractIntention(Beliefs b, Subject s)
        {
            return new Intention("recover");
        }
    }

    // If the agent sees the cat and the cat is believed to hear the child
    // or the cat is running, try and capture the cat.
    public class Capture : IGoal
    {
        public bool HasReasonFor(Subject s, Beliefs b, Intention i)
        {
            if (!new Recover().HasReasonFor(s, b, i))
            {
                if (b.Sees.Agents.ContainsKey('0'))
                {
                    if (b.ToM['0'].Hears.Agents.ContainsKey(s.ID)
                        || Util.GetAgentSpeed('0') == (int)Speed.VeryFast)
                    {
                        return true;
                    }

                    // If lower rank, Misdirect the cat if another agent is seen 
                    // and sees this agent and they both see the cat.
                    if (b.Sees.Agents.Count > 1)
                    {
                        foreach (Subject subj in b.Sees.Agents.Values)
                        {
                            if (Util.IsChild(subj.ID)
                                && b.ToM[subj.ID].Sees.Agents.ContainsKey(s.ID)
                                && ((b.ToM[subj.ID].Sees.Agents.ContainsKey('0')
                                    && Util.IsHigherRank(subj.ID, s.ID)) 
                                        || !b.ToM[subj.ID].Sees.Agents.ContainsKey('0')))
                            {
                                return true;
                            }
                        }
                    }
                }
            }

            return false;
        }

        public Intention ExtractIntention(Beliefs b, Subject s)
        {
            return new Intention("capture");
        }
    }

    // Halt if the cat is spotted and believed to have spotted the child
    // or if in close proximity to the cat.
    public class Halt : IGoal
    {
        public Intention ExtractIntention(Beliefs b, Subject s)
        {
            return new Intention("halt");
        }

        public bool HasReasonFor(Subject s, Beliefs b, Intention i)
        {
            if (!new Capture().HasReasonFor(s, b, i))
            {
                if (b.Sees.Agents.ContainsKey('0'))
                {
                    if (b.ToM['0'].Sees.Agents.ContainsKey(s.ID))
                    {
                        if (Util.GetAgentSpeed('0') != (int)Speed.VeryFast)
                            return true;
                    }
                    else
                    {
                        if (Util.L1Distance(s.Location, b.Agents['0'].Location) <= 2)
                            return true;
                    }
                }
            }

            return false;
        }
    }

    // Track the cat if the current intention is to capture the cat
    // but it is no longer in sight.
    public class Track : IGoal
    {
        public Intention ExtractIntention(Beliefs b, Subject s)
        {
            return new Intention("track");
        }

        public bool HasReasonFor(Subject s, Beliefs b, Intention i)
        {
            if (!new Recover().HasReasonFor(s, b, i))
            {
                if (!b.Sees.Agents.ContainsKey('0') && i.Name == "capture")
                    return true;
            }

            return false;
        }
    }

    // Flank the cat if another agent has seen the cat but not this child agent
    // and the cat is believed to not see this child agent.
    public class Flank : IGoal
    {
        public Intention ExtractIntention(Beliefs b, Subject s)
        {
            Intention i = new Intention("flank");

            foreach (KeyValuePair<char, Subject> kvp in b.Sees.Agents)
            {
                if (Util.IsChild(kvp.Key) && b.ToM[kvp.Key].Sees.Agents.ContainsKey('0')
                    && !b.ToM[kvp.Key].Sees.Agents.ContainsKey(s.ID))
                {
                    i.ID = kvp.Key;

                    break;
                }
            }

            return i;
        }

        public bool HasReasonFor(Subject s, Beliefs b, Intention i)
        {
            if (!new Halt().HasReasonFor(s, b, i))
            {
                if (b.Sees.Agents.ContainsKey('0'))
                {
                    if (b.Sees.Agents.Count > 1
                        && (b.Sees.Agents.ContainsKey('0')
                            && !b.ToM['0'].Sees.Agents.ContainsKey(s.ID)
                            && Util.GetAgentSpeed('0') != (int)Speed.VeryFast))
                    {
                        foreach (Subject subj in b.Sees.Agents.Values)
                        {
                            if (Util.IsChild(subj.ID)
                                && b.ToM[subj.ID].Sees.Agents.ContainsKey('0')
                                && !b.ToM[subj.ID].Sees.Agents.ContainsKey(s.ID))
                            {
                                return true;
                            }
                        }
                    }

                }
            }

            return false;
        }
    }

    // Assist a child agent if it is not moving, not recovering, not regrouping,
    // and this agent does not see the cat.
    public class Assist : IGoal
    {
        public Intention ExtractIntention(Beliefs b, Subject s)
        {
            Intention i = new Intention("assist");

            List<Subject> subjects = new List<Subject>();

            foreach (Subject subj in b.Sees.Agents.Values)
            {
                if (!Util.IsRegrouping(subj.ID) && !Util.IsMoving(subj.ID) && !Util.IsRecovering(subj.ID))
                    subjects.Add(subj);
            }

            i.ID = Util.NearestAgent(subjects, s.Location).ID;

            return i;
        }

        public bool HasReasonFor(Subject s, Beliefs b, Intention i)
        {
            if (!new Track().HasReasonFor(s, b, i))
            {
                if (!b.Sees.Agents.ContainsKey('0') && i.Name != "ambush")
                {
                    if (b.Sees.Agents.Count > 0)
                    {
                        foreach (Subject subj in b.Sees.Agents.Values)
                        {
                            if (!b.ToM[subj.ID].Sees.Agents.ContainsKey(s.ID)
                                && !Util.IsRegrouping(subj.ID)
                                && !Util.IsMoving(subj.ID)
                                && !Util.IsRecovering(subj.ID))
                                return true;
                        }
                    }
                }
            }

            return false;
        }
    }

    // If not currently halting or flanking, ambush the cat if spotted 
    // and the cat does not see this child.
    public class Ambush : IGoal
    {
        public bool HasReasonFor(Subject s, Beliefs b, Intention i)
        {
            if (!new Flank().HasReasonFor(s, b, i))
            {
                if (b.Sees.Agents.ContainsKey('0'))
                {
                    if (Util.InRoom(b.Agents['0'].Location)
                        && !b.ToM['0'].Sees.Agents.ContainsKey(s.ID) 
                        && i.Name != "halt"
                        && i.Name != "flank")
                        return true;
                }
            }

            return false;
        }

        public Intention ExtractIntention(Beliefs b, Subject s)
        {
            Intention i = new Intention("ambush");

            Room room = Util.GetRoom(b.Agents['0'].Location);
            i.ID = room.ID;

            return i;
        }
    }

    // If the child does not see the cat, search for it.
    public class Search : IGoal
    {
        public bool HasReasonFor(Subject s, Beliefs b, Intention i)
        {
            if (!new Assist().HasReasonFor(s, b, i))
            {
                if (!b.Sees.Agents.ContainsKey('0') && b.Rooms.Count > 0)
                    return true;
            }

            return false;
        }

        public Intention ExtractIntention(Beliefs b, Subject s)
        {
            Intention i = new Intention("search");

            Room room = Util.NearestRoom(b.Rooms, s.Location);
            i.ID = room.ID;

            return i;
        }
    }

    // Go to destination.
    public class GoTo : IGoal
    {
        public bool HasReasonFor(Subject s, Beliefs b, Intention i)
        {
            if (!new Ambush().HasReasonFor(s, b, i))
            {
                if (b.Sees.Agents.ContainsKey('0'))
                {
                    return true;
                }
            }

            return false;
        }

        public Intention ExtractIntention(Beliefs b, Subject s)
        {
            Intention i = new Intention("goto");

            if (b.Sees.Agents.ContainsKey('0'))
                i.ID = '0';

            return i;
        }
    }

    // If done searching and no other agent is visible, regroup.
    public class Regroup : IGoal
    {
        public bool HasReasonFor(Subject s, Beliefs b, Intention i)
        {
            if (!new Search().HasReasonFor(s, b, i))
            {
                if (!b.Sees.Agents.ContainsKey('0') && b.Rooms.Count == 0)
                    return true;
            }

            return false;
        }

        public Intention ExtractIntention(Beliefs b, Subject s)
        {
            return new Intention("regroup");
        }
    }
}

// The goals available to the cat agent.
public abstract class CatGoal
{
    // If a child is heard or if a child is spotted within close proximity
    // to the cat itself, then it will flee.
    public class Flee : IGoal
    {
        public Intention ExtractIntention(Beliefs b, Subject s)
        {
            return new Intention("flee");
        }

        public bool HasReasonFor(Subject s, Beliefs b, Intention i)
        {
            List<Subject> perceived = Util.Merge(b.Hears.Agents, b.Sees.Agents).Values.ToList();
            char agtID = Util.NearestAgent(perceived, s.Location).ID;

            if (i.Name == "wait" || b.Hears.Agents.Count > 0 || 
                (b.Sees.Agents.Count > 0 && 
                    Util.L1Distance(s.Location, b.Agents[agtID].Location) <= 3))
            {
                return true;
            }

            return false;
        }
    }

    // If the cat spots a child not within close proximity it will first wait.
    public class Wait : IGoal
    {
        public Intention ExtractIntention(Beliefs b, Subject s)
        {
            return new Intention("wait");
        }

        public bool HasReasonFor(Subject s, Beliefs b, Intention i)
        {
            if (!new Flee().HasReasonFor(s, b, i))
            {
                if (b.Sees.Agents.Count > 0)
                {
                    List<Subject> perceived = Util.Merge(b.Hears.Agents, b.Sees.Agents).Values.ToList();
                    char agtID = Util.NearestAgent(perceived, s.Location).ID;

                    if (i.Name != "wait" && Util.L1Distance(s.Location, b.Agents[agtID].Location) > 3)
                        return true;
                }
            }

            return false;
        }
    }

    // If the cat does not see any child agents and sees food, it will 
    // try to eat the food.
    public class Eat : IGoal
    {
        public Intention ExtractIntention(Beliefs b, Subject s)
        {
            return new Intention("eat");
        }

        public bool HasReasonFor(Subject s, Beliefs b, Intention i)
        {
            if (!new Flee().HasReasonFor(s, b, i))
            {
                if (b.Sees.Agents.Count == 0 && b.Sees.Foods.Count > 0)
                {
                    return true;
                }
            }

            return false;
        }
    }

    // Search for food while no children are spotted.
    public class Search : IGoal
    {
        public Intention ExtractIntention(Beliefs b, Subject s)
        {
            return new Intention("search");
        }

        public bool HasReasonFor(Subject s, Beliefs b, Intention i)
        {
            if (!new Eat().HasReasonFor(s, b, i))
            {
                if (b.Sees.Agents.Count == 0 && b.Rooms.Count > 0)
                {
                    return true;
                }

            }

            return false;
        }
    }
}