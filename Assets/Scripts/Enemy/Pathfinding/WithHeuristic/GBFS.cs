using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class GBFS : MonoBehaviour
{
    public static List<PF_Node> GreedyBFS(PF_Node start, PF_Node end)
    {
        var frontier = new PriorityQueue<PF_Node>();
        frontier.Enqueue(start, 0);
        var cameFrom = new Dictionary<PF_Node, PF_Node>();
        cameFrom[start] = null;

        while (!frontier.IsEmpty)
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
                float priority = Heuristic(end, next);
                frontier.Enqueue(next, priority);
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

    public static float Heuristic(PF_Node to, PF_Node from)
    {
        return Vector3.Distance(to.transform.position, from.transform.position);
    }
}
