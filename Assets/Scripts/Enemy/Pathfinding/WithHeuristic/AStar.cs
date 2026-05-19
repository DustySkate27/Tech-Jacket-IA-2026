using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AStar : MonoBehaviour
{
    public static List<PF_WNode> AStarSearch(PF_WNode start, PF_WNode end)
    {
        var cameFrom = new Dictionary<PF_WNode, (PF_WNode, float)>();
        cameFrom[start] = (null, 0);

        var priorityQueue = new PriorityQueue<PF_WNode>();
        priorityQueue.Enqueue(start, 0);

        while (!priorityQueue.IsEmpty)
        {
            PF_WNode current = priorityQueue.Dequeue();
            current.SetColor(Color.cyan);

            if (current == end)
            {
                break;
            }

            foreach (var next in current.Neighbors)
            {
                float newCost = cameFrom[current].Item2 + next.cost;
                if (cameFrom.ContainsKey(next) && newCost >= cameFrom[next].Item2) continue;
                priorityQueue.Enqueue(next, newCost + Heuristic(end, next));
                cameFrom[next] = (current, newCost);
            }

        }
        PF_WNode newCurrent = end;
        var path = new List<PF_WNode>();

        if (!cameFrom.ContainsKey(end))
        {
            Debug.LogWarning("No se encontró camino al destino.");
        }
        else
        {
            while (newCurrent != null)
            {
                path.Add(newCurrent);
                newCurrent = cameFrom[newCurrent].Item1;
            }
            path.Reverse();
        }
        return path;
    }

    public static float Heuristic(PF_WNode to, PF_WNode from)
    {
        return Vector3.Distance(to.transform.position, from.transform.position);
    }
}
