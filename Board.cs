using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class Board : MonoBehaviour
{
    private static Board instance;
    public static Board Instance { get { return instance; } }

    public float eatTime = 400f;
    public float waitTime = 400f;
    public static int MAX_ROW { get; private set; }
    public static int MAX_COL { get; private set; }
    public static double MAX_DISTANCE { get; private set; }
    [Range(1f, 100f)] public float simulationSpeed = 10f;
    [Range(1, 25)] public int foodAmount = 10;
    public GameObject tilePrefab;
    public GameObject foodPrefab;
    public GameObject rendezvousPrefab;
    public GameObject catPrefab;
    public GameObject[] childPrefabs;
    public GameObject[] objectPrefabs;
    public Dictionary<char, Room> Rooms { get; private set; }
    public Dictionary<char, Agent> Agents { get; private set; }
    public Dictionary<char, ChildAgent> Children { get; private set; }
    public Dictionary<Location, Obstacle> Obstacles { get; private set; }
    public Dictionary<Location, Food> Foods { get; private set; }
    public CatAgent Cat { get; private set; }
    public Location Rendezvous { get; private set; }
    public GameObject TileHolder { get; private set; }
    public GameObject ObjectHolder { get; private set; }
    public Dictionary<Location, GameObject> FloorTiles { get; private set; }
    public Dictionary<char, GameObject> AgentPieces { get; private set; }
    public Dictionary<Location, GameObject> ObjectPieces { get; private set; }

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(this.gameObject);
        }
        else
        {
            instance = this;
        }

        FloorTiles = new Dictionary<Location, GameObject>();
        Rooms = new Dictionary<char, Room>();
        Agents = new Dictionary<char, Agent>();
        Children = new Dictionary<char, ChildAgent>();
        Cat = null;
        Rendezvous = new Location();
        TileHolder = new GameObject();
        ObjectHolder = new GameObject();
        AgentPieces = new Dictionary<char, GameObject>();
        Obstacles = new Dictionary<Location, Obstacle>();
        Foods = new Dictionary<Location, Food>();
        ObjectPieces = new Dictionary<Location, GameObject>();
    }

    // Generates the level from a multi-dimensional array
    public void CreateBoard(char[,] map)
    {
        MAX_ROW = map.GetLength(0);
        MAX_COL = map.GetLength(1);

        Node.Walls = new HashSet<Location>();
        Node.Pits = new HashSet<Location>();

        for (int i = 0; i < MAX_ROW; i++)
        {
            for (int j = 0; j < MAX_COL; j++)
            {
                Location loc = new Location(i, j);
                char symbol = map[i, j];

                GameObject tile;
                tile = Instantiate(tilePrefab, loc.ToVector(), Quaternion.identity);
                tile.transform.SetParent(TileHolder.transform);

                TileProperties tileProperties = tile.GetComponent<TileProperties>();

                if (symbol == '+')
                {
                    Node.Walls.Add(loc);
                    tileProperties.Type = Item.Wall;
                }
                else if (symbol == '-')
                {
                    Node.Pits.Add(loc);
                    tileProperties.Type = Item.Pit;
                }
                else
                {
                    tileProperties.Type = Item.Floor;
                    FloorTiles.Add(loc, tile);
                }

                if (char.ToLower(symbol) >= 'a' && char.ToLower(symbol) <= 'z')
                {
                    char lwrSymbol = char.ToLower(symbol);

                    if (!Rooms.ContainsKey(lwrSymbol))
                        Rooms.Add(lwrSymbol, new Room(lwrSymbol));

                    Rooms[lwrSymbol].UnexploredTiles.Add(loc);

                    Util.SetColor(FloorTiles[loc], Color.gray);
                }
                else if (symbol == '0')
                    Cat = new CatAgent(symbol, loc);
                else if (symbol >= '1' && symbol <= '9')
                    Children.Add(symbol, new ChildAgent(symbol, loc));
                else if (symbol == '&')
                {
                    Obstacles[loc] = new Obstacle(loc);

                    GameObject prefab = objectPrefabs[0];
                    GameObject go;
                    go = Instantiate(prefab, loc.ToVector(), Quaternion.identity);
                    go.transform.SetParent(ObjectHolder.transform);

                    ObjectPieces.Add(loc, go);
                }

                if (symbol >= 'A' && symbol <= 'Z')
                {
                    Foods[loc] = new Food(loc);

                    GameObject go;
                    go = Instantiate(foodPrefab, loc.ToVector(), Quaternion.identity);
                    go.transform.SetParent(ObjectHolder.transform);

                    ObjectPieces.Add(loc, go);
                }
            }
        }

        foreach (Room r in Rooms.Values)
        {
            foreach (Location loc in r.UnexploredTiles)
            {
                HashSet<Location> neighbors = new HashSet<Location>()
                {
                    new Location(loc.Row + 1, loc.Col),
                    new Location(loc.Row - 1, loc.Col),
                    new Location(loc.Row, loc.Col + 1),
                    new Location(loc.Row, loc.Col - 1)
                };

                foreach (Location neighbor in neighbors)
                {
                    if (!Node.Walls.Contains(neighbor) && !Obstacles.ContainsKey(neighbor)
                        && !r.UnexploredTiles.Contains(neighbor))
                    {
                        r.Entrances.Add(loc);
                    }
                }
            }
        }
    }

    // Creates the agents of the game.
    public void CreateAgents()
    {
        GameObject piece = null;

        Rendezvous = Children.First().Value.Location;
        Instantiate(rendezvousPrefab, Rendezvous.ToVector(), Quaternion.identity);

        int i = 0;
        foreach(ChildAgent agt in Children.Values)
        {
            piece = Instantiate(childPrefabs[i], agt.Location.ToVector(), Quaternion.identity);
            agt.Piece = piece.GetComponent<AgentPiece>();

            AgentPieces.Add(agt.ID, piece);
            Agents.Add(agt.ID, agt);

            i++;
        }

        if(!ReferenceEquals(Cat, null))
        {
            piece = Instantiate(catPrefab, Cat.Location.ToVector(), Quaternion.identity);
            Cat.Piece = piece.GetComponent<AgentPiece>();
            Cat.Piece.clothsRnderer.color = Color.black;

            AgentPieces.Add(Cat.ID, piece);
            Agents.Add(Cat.ID, Cat);
        }
    }
}