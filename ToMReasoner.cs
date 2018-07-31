using System.Linq;
using System.Collections.Generic;
using UnityEngine;

public class ToMReasoner
{
    // The recursive higher-order observation method as described in the thesis. 
    // It has a height limiter to specify the desired max order of observation.
    public void UpdateToM(Subject s, Beliefs b, HashSet<Location> fov, int height)
    {
        if (height == 0 || b.Sees.Agents.Count == 0)
            return;

        foreach (KeyValuePair<char, Subject> kvp in b.Sees.Agents)
        {
            HashSet<Location> intersectFov = FOV.GetSharedFov(kvp.Value, s);

            VisionPercept vp = Sight.Perceive(kvp.Key, intersectFov);
            AudioPercept ap = Hearing.Perceive(kvp.Key, kvp.Value.Location);

            if (!b.ToM.ContainsKey(kvp.Key))
                b.ToM.Add(kvp.Key, new Beliefs());

            b.ToM[kvp.Key].Update(vp);
            b.ToM[kvp.Key].Update(ap);

            UpdateToM(kvp.Value, b.ToM[kvp.Key], intersectFov, height - 1);
        }
    }
}