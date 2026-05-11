using UnityEngine;
using System.Collections.Generic;

public class PF_WeightedNPC : MonoBehaviour
{
    public PF_WeightNode start, end;
    public float speed;
    private List<PF_WeightNode> path = new();

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (path.Count > 0)
        {
            var dir = path[0].transform.position - transform.position;
            transform.position += dir.normalized * Time.deltaTime * speed;
            if(dir.magnitude < 0.3f)
                path.RemoveAt(0);
        }

        if (Input.GetKeyDown(KeyCode.P))
        {
            path = WeightedPathfinding.BFS(start, end);
            for (int i = 0; i < path.Count; i++)
            {
                path[i].SetColor(Color.Lerp(Color.red, Color.yellow, (float)i / path.Count));
            }
        }
    }
}
