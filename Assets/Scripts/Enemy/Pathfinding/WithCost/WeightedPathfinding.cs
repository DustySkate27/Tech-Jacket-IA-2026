using System.Collections.Generic;
using UnityEngine;

public class WeightedPathfinding
{
    public static List<PF_WeightNode> BFS(PF_WeightNode start, PF_WeightNode end)
    {
        var frontier = new Queue<PF_WeightNode>();
        frontier.Enqueue(start);

        var cameFrom = new Dictionary<PF_WeightNode, (PF_WeightNode,float)>();
        cameFrom[start] = (null, float.PositiveInfinity);

        var priorityQueue = new PriorityQueue<PF_WeightNode>();
        priorityQueue.Enqueue(start, 0);

        while (frontier.Count > 0)
        {
            PF_WeightNode current = frontier.Dequeue();
            current.SetColor(Color.cyan);

            if (current == end)
            {
                break;
            }

            foreach (var next in current.Neighbors)
            {
                if (cameFrom.ContainsKey(next)) continue;
                frontier.Enqueue(next);
                cameFrom[next] = (current, current.cost);
            }
            
        }
        PF_WeightNode newCurrent = end;
        var path = new List<PF_WeightNode>();
        while (newCurrent != null)
        {
            path.Add(newCurrent);
            newCurrent = cameFrom[newCurrent];
        }
        path.Reverse();
        return path;
    }
}