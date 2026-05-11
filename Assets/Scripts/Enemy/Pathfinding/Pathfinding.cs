using System.Collections.Generic;
using UnityEngine;

public class Pathfinding
{
    public static List<PF_Node> BFS(PF_Node start, PF_Node end)
    {
        var frontier = new Queue<PF_Node>();
        frontier.Enqueue(start);
        var cameFrom = new Dictionary<PF_Node, PF_Node>();
        cameFrom[start] = null;

        while (frontier.Count > 0)
        {
            PF_Node current = frontier.Dequeue();
            current.SetColor(Color.cyan);

            if (current == end)
            {
                break;
            }

            foreach (var next in current.Neighbors)
            {
                if (cameFrom.ContainsKey(next)) continue;
                frontier.Enqueue(next);
                cameFrom[next] = current;
            }
            
        }
        PF_Node newCurrent = end;
        var path = new List<PF_Node>();
        while (newCurrent != null)
        {
            path.Add(newCurrent);
            newCurrent = cameFrom[newCurrent];
        }
        path.Reverse();
        return path;
    }
}