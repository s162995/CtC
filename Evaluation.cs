using System.Collections.Generic;

public abstract class Evaluation : IComparer<Node>
{
    public abstract double F(Node n);

    public int Compare(Node n1, Node n2)
    {
        return Comparer<double>.Default.Compare(this.F(n1), this.F(n2));
    }

    // A* node evaluation consideres cost of both path traveled and goal heuristic.
    public class AStar : Evaluation
    {
        public AStar(Node init) { }

        public override double F(Node n)
        {
            return n.G + n.H;
        }
    }

    // The Greedy node only evaluates nodes wrt. goal heuristic.
    public class Greedy : Evaluation
    {
        public Greedy(Node init) { }

        public override double F(Node n)
        {
            return n.H;
        }
    }
}