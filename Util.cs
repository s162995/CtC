using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class Util
{
    private static readonly System.Random rnd = new System.Random();

    public static int RandomNum(int max)
    {
        return rnd.Next(max);
    }

    public static T RandomElement<T>(List<T> list)
    {
        int r = rnd.Next(list.Count);
        return list[r];
    }

    public static double EuclideanDistance(Location from, Location to)
    {
        return Math.Sqrt(Math.Pow((from.Col - to.Col), 2) 
            + Math.Pow((from.Row - to.Row), 2));
    }

    public static int L1Distance(Location from, Location to)
    {
        return Math.Abs(from.Col - to.Col) 
            + Math.Abs(from.Row - to.Row);
    }

    public static void SetAlpha(GameObject go, float a)
    {
        SpriteRenderer rndr = go.GetComponent<SpriteRenderer>();
        Color c = rndr.color;
        c.a = a;
        rndr.color = c;
    }

    public static void SetColor(GameObject go, Color c)
    {
        SpriteRenderer rndr = go.GetComponent<SpriteRenderer>();
        rndr.color = c;
    }

    public static Room NearestRoom(Dictionary<char, Room> rooms, Location loc)
    {
        if (rooms.Count == 0)
            throw new NullReferenceException();

        if (rooms.Count == 1)
            return rooms.First().Value;

        List<Room> sortedRooms = rooms.Values.OrderBy(x => x.DistanceTo(loc)).ToList();

        return sortedRooms[0];
    }

    public static Subject NearestAgent(List<Subject> subjects, Location loc)
    {
        if (subjects.Count == 0)
            return new Subject();

        if (subjects.Count == 1)
            return subjects[0];

        List<Subject> sortedSubjects = subjects.OrderBy(x => L1Distance(x.Location, loc)).ToList();

        return sortedSubjects[0];
    }

    public static Location NearestLocation (ICollection<Location> locs, Location loc)
    {
        if (locs.Count == 1)
            return locs.First();

        List<Location> sortedLocs = locs.OrderBy(x => L1Distance(x, loc)).ToList();

        return sortedLocs[0];
    }

    public static Location FarthestLocation(ICollection<Location> locs, Location loc)
    {
        if (locs.Count == 1)
            return locs.First();

        List<Location> sortedLocs = locs.OrderBy(x => L1Distance(x, loc)).ToList();
        sortedLocs.Reverse();

        return sortedLocs[0];
    }

    public static Room CloneRoom(char id)
    {
        return Manager.Board.Rooms[id].Clone();
    }

    public static Dictionary<char, Room> CloneAllRooms()
    {
        return Manager.Board.Rooms.ToDictionary(x => x.Key, x => x.Value.Clone());
    }

    public static void DisableRenderer(float delay, SpriteRenderer renderer)
    {
        Manager.Board.StartCoroutine(DisableRendererUtil(delay, renderer));
    }

    public static IEnumerator DisableRendererUtil(float delay, SpriteRenderer renderer)
    {
        if (renderer != null)
            renderer.enabled = true;

        yield return new WaitForSeconds(delay);

        if (renderer != null)
            renderer.enabled = false;

        yield break;
    }

    public static void FlashRenderer(float freq, int repetitions, SpriteRenderer renderer)
    {
        Manager.Board.StartCoroutine(FlashRendererUtil(freq, repetitions, renderer));
    }

    public static IEnumerator FlashRendererUtil(float delay, int repetitions, SpriteRenderer renderer)
    {
        for (int i = 0; i < repetitions; i++)
        {
            if (renderer != null)
                renderer.enabled = false;

            yield return new WaitForSeconds(delay);

            if (renderer != null)
                renderer.enabled = true;

            yield return new WaitForSeconds(delay);
        }

        yield break;
    }

    public static bool IsAgent(char id)
    {
        return char.IsNumber(id);
    }

    public static bool IsCat(char id)
    {
        return id == '0';
    }

    public static bool IsChild(char id)
    {
        if (char.IsNumber(id) && id != '0')
            return true;

        return false;
    }

    public static Vector2 GetAgentDirection(char id)
    {
        return Manager.Board.Agents[id].Direction;
    }

    public static bool IsMoving(char id)
    {
        return Manager.Board.Agents[id].IsMoving;
    }

    public static bool IsRoom(char id)
    {
        return char.IsLower(id);
    }

    public static Room GetRoom(Location loc)
    {
        foreach(Room r in Manager.Board.Rooms.Values)
        {
            if (r.UnexploredTiles.Contains(loc))
                return r;
        }

        Debug.Log("Error: Room ID not found!");
        return null;
    }

    public static int GetAgentSpeed(char id)
    {
        return Manager.Board.Agents[id].ExecTime;
    }

    public static bool AgentAt(char id, Location loc)
    {
        if (!Manager.Board.Agents.ContainsKey(id))
            return false;

        return Manager.Board.Agents[id].Location.Equals(loc);
    }

    public static bool ObstacleAt(Location loc)
    {
        return Manager.Board.Obstacles.ContainsKey(loc);
    }

    public static bool FoodAt(Location loc)
    {
        return Manager.Board.Foods.ContainsKey(loc);
    }

    public static bool InRoom(char id, Location loc)
    {
        if (!Manager.Board.Rooms.ContainsKey(id))
            return false;

        return Manager.Board.Rooms[id].UnexploredTiles.Contains(loc);
    }

    public static bool InRoom(Location loc)
    {
        foreach (Room room in Manager.Board.Rooms.Values)
        {
            if (room.UnexploredTiles.Contains(loc))
                return true;
        }

        return false;
    }

    public static bool IsHigherRank(char id1, char id2)
    {
        return (int)Char.GetNumericValue(id1) > (int)Char.GetNumericValue(id2);
    }

    public static HashSet<Location> GetDynamicObstacles(char id, Location loc, Beliefs b)
    {
        HashSet<Location> obstacles = new HashSet<Location>();

        foreach (KeyValuePair<char, Subject> kvp in b.Sees.Agents)
        {
            if (kvp.Key == '0' || kvp.Value.Location == loc)
                continue;

            if (IsCat(id) || 
                (IsChild(id) 
                && !IsRegrouping(kvp.Key)
                && (!b.ToM[kvp.Key].Sees.Agents.ContainsKey(id)
                    || IsHigherRank(kvp.Key, id) 
                    || IsRecovering(kvp.Key)
                    || IsUrgent(kvp.Key))))
            {
                obstacles.Add(kvp.Value.Location);
            }
        }

        return obstacles;
    }

    public static bool IsUrgent(char id)
    {
        return Manager.Board.Children[id].IsUrgent;
    }

    public static bool IsRecovering(char id)
    {
        return Manager.Board.Children[id].IsRecovering;
    }

    public static bool IsRegrouping(char id)
    {
        return Manager.Board.Children[id].IsRegrouping;
    }

    public static Dictionary<TKey, TValue> Merge<TKey, TValue>(Dictionary<TKey, TValue> dict1, Dictionary<TKey, TValue> dict2)
    {
        Dictionary<TKey, TValue> newDict = new Dictionary<TKey, TValue>(dict1);

        List<TKey> keys = new List<TKey>(dict2.Keys);
        foreach (TKey key in keys)
        {
            newDict[key] = dict2[key];
        }

        return newDict;
    }
}