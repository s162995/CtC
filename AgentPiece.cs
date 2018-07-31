using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum Feature
{
    Sound,
    Vision,
    Plan,
    Path,
    Obstacle,
    Food
}

public class AgentPiece : MonoBehaviour
{
    public GameObject visionPrefab;
    public GameObject soundPrefab;
    public GameObject planPrefab;
    public GameObject pathPrefab;
    public GameObject obstaclePrefab;
    public GameObject foodPrefab;
    public SpriteRenderer clothsRnderer { get; set; }
    public SpriteRenderer rnderer { get; set; }

    private List<GameObject> visionTiles;
    private List<GameObject> soundtiles;
    private List<GameObject> planTiles;
    private List<GameObject> obstacleTiles;
    private List<GameObject> foodTiles;

    private void Awake()
    {
        rnderer = GetComponent<SpriteRenderer>();
        clothsRnderer = transform.GetChild(0).GetComponent<SpriteRenderer>();

        Color color = new Color(255f, 255f, 255f, 0.65f);
        visionTiles = CreateFeatureList(visionPrefab, 21, color);
        soundtiles = CreateFeatureList(soundPrefab, 100, clothsRnderer.color);
        planTiles = CreateFeatureList(planPrefab, 100, clothsRnderer.color);
        obstacleTiles = CreateFeatureList(obstaclePrefab, 21, Color.white);
        foodTiles = CreateFeatureList(foodPrefab, 21, Color.white);
    }

    private List<GameObject> CreateFeatureList(GameObject prefab, int size, Color c)
    {
        List<GameObject> list = new List<GameObject>();
        for (int i = 0; i < size; i++)
        {
            GameObject obj = (GameObject)Instantiate(prefab);
            obj.SetActive(false);
            obj.GetComponent<SpriteRenderer>().color = c;
            obj.transform.SetParent(this.transform);
            list.Add(obj);
        }

        return list;
    }

    // Translates the agent piece and updates the sprite orientation.
    public void Move(Vector2 dir)
    {
        if (dir == Vector2.left)
        {
            rnderer.flipX = true;
            clothsRnderer.flipX = true;
        }
        else if (dir == Vector2.right)
        {
            rnderer.flipX = false;
            clothsRnderer.flipX = false;
        }

        transform.Translate(dir);
    }

    // Displays the different features of the agent (e.g. sound, vision, plan).
    public void DisplayFeature(IEnumerable<Location> locs, Feature f)
    {
        List<GameObject> tiles = new List<GameObject>();

        switch (f)
        {
            case Feature.Vision:
                tiles = visionTiles;
                break;
            case Feature.Sound:
                tiles = soundtiles;
                foreach (GameObject go in tiles)
                {
                    float inverseSimSpeed = 1f / Manager.Board.simulationSpeed;
                    SpriteRenderer r = go.GetComponent<SpriteRenderer>();
                    Util.DisableRenderer(inverseSimSpeed * 2f, r);
                }
                break;
            case Feature.Plan:
                tiles = planTiles;
                break;
            case Feature.Obstacle:
                tiles = obstacleTiles;
                break;
            case Feature.Food:
                tiles = foodTiles;
                break;
            default:
                return;
        }

        foreach (GameObject go in tiles)
            go.SetActive(false);

        int i = 0;
        foreach (Location loc in locs)
        {
            GameObject go = tiles[i];
            go.transform.position = loc.ToVector();
            go.SetActive(true);

            i++;
        }
    }
}