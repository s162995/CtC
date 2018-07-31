using System.Collections.Generic;

public class Intention
{
    public string Name { get; private set; }
    public char ID { get; set; }

    public Intention(string name)
    {
        Name = name;
    }
}