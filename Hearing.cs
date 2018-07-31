using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Hearing
{
    // Hearing is determined by checking for sound at the agents current
    // location.
    public static AudioPercept Perceive(char agtID, Location loc)
    {
        AudioPercept p = new AudioPercept();

        foreach (Agent a in Manager.Board.Agents.Values)
        {
            if (a.Noise == null || a.ID == agtID)
                continue;

            if (a.Noise.Contains(Manager.Board.Agents[agtID].Location))
                p.Agents.Add(a.ID, new Subject(a.ID, a.Location));
        }

        return p;
    }
}