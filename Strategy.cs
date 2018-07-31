using System.Linq;
using System.Collections;
using System.Collections.Generic;

public abstract class Strategy 
{
    public HashSet<Node> Explored { get; private set; }

    public Strategy()
    {
        Explored = new HashSet<Node>();
    }

    public abstract Node DequeueLeaf();

    public abstract void AddToFrontier(Node n);

    public abstract bool InFrontier(Node n);

    public abstract int FrontierSize();

    public abstract bool FrontierEmpty();

    public abstract List<Location> ExtractPlan();

    // The BFS has a normal queue as the frontier.
    public class BreadthFirst : Strategy
    {
        public Queue Frontier { get; private set; }

        public BreadthFirst() : base()
        {
            Frontier = new Queue();
        }

        public override void AddToFrontier(Node n)
        {
            Frontier.Enqueue(n);
        }

        public override Node DequeueLeaf()
        {
            return (Node)Frontier.Dequeue();
        }

        public override bool FrontierEmpty()
        {
            return Frontier.Count == 0;
        }

        public override int FrontierSize()
        {
            return Frontier.Count;
        }

        public override bool InFrontier(Node n)
        {
            return Frontier.Contains(n);
        }

        public override List<Location> ExtractPlan()
        {
            List<Location> locs = new List<Location>();

            foreach (Node node in Explored)
                locs.Add(node.CurLocation);

            return locs;
        }

        public override string ToString()
        {
            return "BFS";
        }
    }

    // A Best-First search uses a priority queue to evaluate nodes.
    public class BestFirst : Strategy
    {
        public PriorityQueue<Node> Frontier { get; private set; }

        private Node leaf;

        public BestFirst(Evaluation e) : base()
        {
            Frontier = new PriorityQueue<Node>(e);
        }

        public override void AddToFrontier(Node n)
        {
            Frontier.Enqueue(n);
        }

        public override Node DequeueLeaf()
        {
            leaf = Frontier.Dequeue();
            return leaf;
        }

        public override bool FrontierEmpty()
        {
            return Frontier.Count == 0;
        }

        public override int FrontierSize()
        {
            return Frontier.Count;
        }

        public override bool InFrontier(Node n)
        {
            return Frontier.Contains(n);
        }

        public override List<Location> ExtractPlan()
        {
            List<Location> plan = new List<Location>();

            while (!leaf.IsInitialState())
            {
                plan.Add(leaf.CurLocation);
                leaf = leaf.Parent;
            }
            plan.Reverse();

            return plan;
        }

        public override string ToString()
        {
            return "Best-First";
        }
    }
}