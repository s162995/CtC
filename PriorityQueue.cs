using System.Collections.Generic;

public class PriorityQueue<T>
{
    private List<T> elements;
    private IComparer<T> comparer;

    public PriorityQueue()
    {
        elements = new List<T>();
    }

    public PriorityQueue(IComparer<T> comparer)
    {
        elements = new List<T>();
        this.comparer = comparer;
    }

    public int Count
    {
        get { return elements.Count; }
    }

    public void Enqueue(T n)
    {
        elements.Add(n);
    }

    public T Dequeue()
    {
        elements.Sort(comparer);
        T n = elements[0];
        elements.Remove(n);

        return n;
    }

    public bool Contains(T n)
    {
        return elements.Contains(n);
    }
}