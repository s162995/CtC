using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public abstract class Plan
{
    protected string name;

    public bool IsPlanFor(Intention g)
    {
        return this.name == g.Name;
    }

    public abstract Queue<Location> ExtractPlan(Subject s, Intention i, Beliefs b, List<Location> path);
}

public abstract class ChildPlan
{
    // Flank is planned with a Best-First search with Flank nodes.
    public class Flank : Plan
    {
        public Flank()
        {
            name = "flank";
        }

        public override Queue<Location> ExtractPlan(Subject s, Intention i, Beliefs b, List<Location> path)
        {
            Queue<Location> plan = new Queue<Location>();

            FlankNode fNode = new FlankNode(null, b.Agents['0'].Location, b.Agents[i.ID].Location)
            {
                CurLocation = new Location(s.Location),
                Obstacles = new HashSet<Location>(b.Obstacles.Keys)
            };

            fNode.Obstacles.UnionWith(Util.GetDynamicObstacles(s.ID, s.Location, b));

            plan = Planner.Search(fNode);

            return plan;
        }
    }

    // Assist is planned as a flank to a random destination within the FOV of the assisted
    // agent that is not visible to this agent.
    public class Assist : Plan
    {
        public Assist()
        {
            name = "assist";
        }

        public override Queue<Location> ExtractPlan(Subject s, Intention i, Beliefs b, List<Location> path)
        {
            Queue<Location> plan = new Queue<Location>();
            Location dest = new Location();

            HashSet<Location> fov = FOV.GetFov(b.Agents[i.ID].Direction, b.Agents[i.ID].Location);

            if(fov.Except(FOV.GetSharedFov(s, b.Agents[i.ID])).Count() > 0)
                dest = Util.RandomElement(fov.Except(FOV.GetSharedFov(s, b.Agents[i.ID])).ToList(), s.Location);
            else
                dest = Util.RandomElement(fov.ToList(), s.Location);

            FlankNode fNode = new FlankNode(null, dest, b.Agents[i.ID].Location)
            {
                CurLocation = new Location(s.Location),
                Obstacles = new HashSet<Location>(b.Obstacles.Keys)
            };

            fNode.Obstacles.UnionWith(Util.GetDynamicObstacles(s.ID, s.Location, b));

            plan = Planner.Search(fNode);

            return plan;
        }
    }

    // Track is planned as a flee from the cats location away from the childs location.
    // The agent then goes to the destination of this search.
    public class Track : Plan
    {
        public Track()
        {
            name = "track";
        }

        public override Queue<Location> ExtractPlan(Subject s, Intention i, Beliefs b, List<Location> path)
        {
            List<Location> actions = new List<Location>();

            path = new Flee().ExtractPlan(s, i, b, path).ToList();

            i.ID = '0';

            actions.AddRange(new GoTo().ExtractPlan(s, i, b, path));

            return new Queue<Location>(actions);
        }
    }

    // Search is planned as a GoTo action. If the agent is in a room, the go action will have
    // the farthest room tile as the destination, otherwise the destination will be the room 
    // entrance.
    public class Search : Plan
    {
        public Search()
        {
            name = "search";
        }

        public override Queue<Location> ExtractPlan(Subject s, Intention i, Beliefs b, List<Location> path)
        {
            List<Location> actions = new List<Location>();

            if (!Util.InRoom(i.ID, s.Location))
            {
                actions.AddRange(new GoTo().ExtractPlan(s, i, b, path));
                if (actions.Count > 0) s.Location = new Location(actions[actions.Count - 1]);
            }

            actions.AddRange(new Explore().ExtractPlan(s, i, b, path));

            return new Queue<Location>(actions);
        }
    }

    // Ambush is planned as a Leave room, GoTo entrance, Wait at entrance if in room.
    // Otherwise, it is GoTo entrance, Wait or simply Wait if already at entrance.
    public class Ambush : Plan
    {
        public Ambush()
        {
            name = "ambush";
        }

        public override Queue<Location> ExtractPlan(Subject s, Intention i, Beliefs b, List<Location> path)
        {
            List<Location> actions = new List<Location>();

            if (Util.InRoom(i.ID, s.Location))
            {
                actions.AddRange(new Leave().ExtractPlan(new Subject(s), i, b, path));
                s.Location = new Location(actions[actions.Count - 1]);
            }

            actions.AddRange(new GoTo().ExtractPlan(new Subject(s), i, b, path));
            if (actions.Count > 0) s.Location = new Location(actions[actions.Count - 1]);

            actions.AddRange(new Wait().ExtractPlan(new Subject(s), i, b, path));

            return new Queue<Location>(actions);
        }
    }

    // Capture is planned as a GoTo cat destination, Grab if not already at cat location.
    // Otherwise, it is simply a Grab action.
    public class Capture : Plan
    {
        public Capture()
        {
            name = "capture";
        }

        public override Queue<Location> ExtractPlan(Subject s, Intention i, Beliefs b, List<Location> path)
        {
            List<Location> actions = new List<Location>();
            i.ID = '0';

            if (!Util.AgentAt('0', s.Location))
            {
                actions.AddRange(new GoTo().ExtractPlan(new Subject(s), i, b, path));
                if (actions.Count > 0) s.Location = new Location(actions[actions.Count - 1]);
            }

            actions.AddRange(new Grab().ExtractPlan(new Subject(s), i, b, path));

            return new Queue<Location>(actions);
        }
    }

    // Regroup is planned as a GoTo rendezvous location, Halt is not already at rendezvous.
    public class Regroup : Plan
    {
        public Regroup()
        {
            name = "regroup";
        }

        public override Queue<Location> ExtractPlan(Subject s, Intention i, Beliefs b, List<Location> path)
        {
            List<Location> actions = new List<Location>();
            i.ID = 'R';

            if (!s.Location.Equals(Manager.Board.Rendezvous))
            {
                actions.AddRange(new GoTo().ExtractPlan(new Subject(s), i, b, path));
                s.Location = new Location(actions[actions.Count - 1]);
            }

            actions.AddRange(new Halt().ExtractPlan(new Subject(s), i, b, path));

            return new Queue<Location>(actions);
        }
    }

    // Goto is a Best-First search with Path nodes.
    public class GoTo : Plan
    {
        public GoTo()
        {
            name = "goto";
        }

        public override Queue<Location> ExtractPlan(Subject s, Intention i, Beliefs b, List<Location> path)
        {
            Queue<Location> plan = new Queue<Location>();
            Location dest = new Location();

            if (i.ID == 'R')
                dest = Manager.Board.Rendezvous;
            else if (i.ID == '0')
                if (i.Name == "track")
                    dest = path[path.Count - 1];
                else
                    dest = b.Agents['0'].Location;
            else
                dest = b.Rooms[i.ID].NearestEntrance(s.Location);

            PathNode pNode = new PathNode(null, dest)
            {
                CurLocation = new Location(s.Location),
                Obstacles = new HashSet<Location>(b.Obstacles.Keys)
            };
            
            pNode.Obstacles.UnionWith(Util.GetDynamicObstacles(s.ID, s.Location, b));

            plan = Planner.Search(pNode);

            if (char.IsLower(i.ID))
            {
                Queue<Location> actions = new Queue<Location>();

                foreach (Location loc in plan.ToList())
                {
                    if (!Util.InRoom(plan.Peek()) || b.Rooms[i.ID].Entrances.Contains(plan.Peek()))
                        actions.Enqueue(plan.Dequeue());
                }

                return actions;
            }

            return plan;
        }
    }

    // Explore is a GoTo to the farthest room location.
    public class Explore : Plan
    {
        public Explore()
        {
            name = "explore";
        }

        public override Queue<Location> ExtractPlan(Subject s, Intention i, Beliefs b, List<Location> path)
        {
            Queue<Location> plan = new Queue<Location>();
            Location dest = new Location();

            if (!b.Rooms.ContainsKey(i.ID))
                return plan;

            dest = b.Rooms[i.ID].FarthestTile(s.Location);

            PathNode pNode = new PathNode(null, dest)
            {
                CurLocation = new Location(s.Location),
                Obstacles = new HashSet<Location>(b.Obstacles.Keys)
            };

            pNode.Obstacles.UnionWith(Util.GetDynamicObstacles(s.ID, s.Location, b));

            plan = Planner.Search(pNode);

            return plan;
        }
    }

    // Leave is a backtrack of the path traveled to the first location not within the room.
    // A GoTo is then planned to this location.
    public class Leave : Plan
    {
        public Leave()
        {
            name = "leave";
        }

        public override Queue<Location> ExtractPlan(Subject s, Intention i, Beliefs b, List<Location> path)
        {
            Queue<Location> plan = new Queue<Location>();

            for (int j = path.Count - 1; j >= 0; j--)
            {
                if (!Util.InRoom(path[j]))
                {
                    PathNode pNode = new PathNode(null, path[j])
                    {
                        CurLocation = new Location(s.Location),
                        Obstacles = new HashSet<Location>(b.Obstacles.Keys)
                    };

                    pNode.Obstacles.UnionWith(Util.GetDynamicObstacles(s.ID, s.Location, b));

                    plan = Planner.Search(pNode);

                    break;
                }
            }

            return plan;
        }
    }

    // For the child agents the flee node is performed as a flee which prioritizes a specified direction.
    public class Flee : Plan
    {
        public Flee()
        {
                name = "flee";
        }

        public override Queue<Location> ExtractPlan(Subject s, Intention i, Beliefs b, List<Location> path)
        {
            Queue<Location> plan = new Queue<Location>();
            Location dest = new Location();
            dest = s.Location;

            PursueNode fNode = new PursueNode(null, b.Agents['0'].Location, dest, 10, b.Agents['0'].Direction, s.Direction)
            {
                CurLocation = new Location(s.Location),
                Obstacles = new HashSet<Location>(b.Obstacles.Keys)
            };

            fNode.Obstacles.UnionWith(Util.GetDynamicObstacles(s.ID, s.Location, b));

            plan = Planner.Search(fNode);

            return plan;
        }
    }

    // Grab is planned as a stationary action.
    public class Grab : Plan
    {
        public Grab()
        {
                name = "grab";
        }

        public override Queue<Location> ExtractPlan(Subject s, Intention i, Beliefs b, List<Location> path)
        {
            Queue<Location> plan = new Queue<Location>();

            i.ID = '0';
            plan.Enqueue(s.Location);

            return plan;
        }
    }

    // Recover is planned as a stationary action.
    public class Recover : Plan
    {
        public Recover()
        {
                name = "recover";
        }

        public override Queue<Location> ExtractPlan(Subject s, Intention i, Beliefs b, List<Location> path)
        {
            Queue<Location> plan = new Queue<Location>();

            plan.Enqueue(s.Location);

            return plan;
        }
    }

    // Wait is planned as a stationary action.
    public class Wait : Plan
    {
        public Wait()
        {
            name = "wait";
        }

        public override Queue<Location> ExtractPlan(Subject s, Intention i, Beliefs b, List<Location> path)
        {
            Queue<Location> plan = new Queue<Location>();

            plan.Enqueue(s.Location);

            return plan;
        }
    }

    // Halt is planned as a stationary action.
    public class Halt : Plan
    {
        public Halt()
        {
                name = "halt";
        }

        public override Queue<Location> ExtractPlan(Subject s, Intention i, Beliefs b, List<Location> path)
        {
            Queue<Location> plan = new Queue<Location>();

            plan.Enqueue(s.Location);

            return plan;
        }
    }
}

// The plans available to the cat.
public abstract class CatPlan
{
    // Eat is planned as a GoTo food location, Consume food object. If already at food location then
    // simply Consume.
    public class Eat : Plan
    {
        public Eat()
        {
            name = "eat";
        }

        public override Queue<Location> ExtractPlan(Subject s, Intention i, Beliefs b, List<Location> path)
        {
            List<Location> actions = new List<Location>();
            i.ID = 'F';

            if (!Util.FoodAt(s.Location))
            {
                actions.AddRange(new GoTo().ExtractPlan(s, i, b, path));
                s.Location = new Location(actions[actions.Count - 1]);
            }

            actions.AddRange(new Consume().ExtractPlan(s, i, b, path));

            return new Queue<Location>(actions);
        }
    }

    public class Search : Plan
    {
        public Search()
        {
            name = "search";
        }

        public override Queue<Location> ExtractPlan(Subject s, Intention i, Beliefs b, List<Location> path)
        {
            List<Location> actions = new List<Location>();
            Room room = Util.NearestRoom(b.Rooms, s.Location);
            i.ID = room.ID;

            if (!Util.InRoom(room.ID, s.Location))
            {
                actions.AddRange(new GoTo().ExtractPlan(s, i, b, path));
                s.Location = new Location(actions[actions.Count - 1]);
            }

            actions.AddRange(new Explore().ExtractPlan(s, i, b, path));

            return new Queue<Location>(actions);
        }
    }

    public class GoTo : Plan
    {
        public GoTo()
        {
                name = "goto";
        }

        public override Queue<Location> ExtractPlan(Subject s, Intention i, Beliefs b, List<Location> path)
        {
            Queue<Location> plan = new Queue<Location>();
            Location dest = new Location();

            if (char.IsLower(i.ID))
                dest = b.Rooms[i.ID].NearestEntrance(s.Location);
            else if (i.ID == 'F')
                dest = Util.NearestLocation(b.Foods.Keys, s.Location);

            PathNode pNode = new PathNode(null, dest)
            {
                CurLocation = new Location(s.Location),
                Obstacles = new HashSet<Location>(b.Obstacles.Keys)
            };

            plan = Planner.Search(pNode);

            return plan;
        }
    }

    public class Explore : Plan
    {
        public Explore()
        {
            name = "explore";
        }

        public override Queue<Location> ExtractPlan(Subject s, Intention i, Beliefs b, List<Location> path)
        {
            Queue<Location> plan = new Queue<Location>();
            Location dest = new Location();

            dest = b.Rooms[i.ID].FarthestTile(s.Location);

            PathNode pNode = new PathNode(null, dest)
            {
                CurLocation = new Location(s.Location),
                Obstacles = new HashSet<Location>(b.Obstacles.Keys)
            };

            pNode.Obstacles.UnionWith(Util.GetDynamicObstacles(s.ID, s.Location, b));

            plan = Planner.Search(pNode);

            return plan;
        }
    }

    // Consume is planned as a stationary action.
    public class Consume : Plan
    {
        public Consume()
        {
                name = "consume";
        }

        public override Queue<Location> ExtractPlan(Subject s, Intention i, Beliefs b, List<Location> path)
        {
            Queue<Location> plan = new Queue<Location>();

            plan.Enqueue(s.Location);

            return plan;
        }
    }

    // The cat flees from the location of the nearest child for a specified max path length.
    // Flee is planned as a Greedy Best-first search using Flee nodes.
    public class Flee : Plan
    {
        public Flee()
        {
                name = "flee";
        }

        public override Queue<Location> ExtractPlan(Subject s, Intention i, Beliefs b, List<Location> path)
        {
            Queue<Location> plan = new Queue<Location>();
            Subject subj = new Subject();

            if (b.Sees.Agents.Count > 0 || b.Hears.Agents.Count > 0)
            {
                subj = Util.NearestAgent(Util.Merge(b.Hears.Agents, b.Sees.Agents).Values.ToList(), s.Location);
                i.ID = subj.ID;
            }
            else
                subj.Location = s.Location;

            FleeNode fNode = new FleeNode(null, s.Location, subj.Location, 10)
            {
                CurLocation = new Location(s.Location),
                Obstacles = new HashSet<Location>(b.Obstacles.Keys)
            };

            plan = Planner.Search(fNode);

            return plan;
        }
    }

    public class Wait : Plan
    {
        public Wait()
        {
                name = "wait";
        }

        public override Queue<Location> ExtractPlan(Subject s, Intention i, Beliefs b, List<Location> path)
        {
            Queue<Location> plan = new Queue<Location>();

            plan.Enqueue(s.Location);

            return plan;
        }
    }
}