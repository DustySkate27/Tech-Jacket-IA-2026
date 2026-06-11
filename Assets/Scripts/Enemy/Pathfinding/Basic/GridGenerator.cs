using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class GridGenerator : MonoBehaviour
{
    public PF_Node prefab;
    public PF_Node[] nodeGrid;
    public int width;
    public int height;
    public float distance;


    [ContextMenu("SetNodeGrid")]
    public void SetNodeGrid()
    {
        nodeGrid = new PF_Node[width * height];
        for (int h = 0; h < height; h++)
        {
            for (int w = 0; w < width; w++)
            {
                PF_Node newNode = Instantiate(prefab, transform.position
                    + new Vector3(w * distance, 0, h * distance), transform.rotation, transform);
                newNode.SetIndexes(w, h);
                nodeGrid[w + h * width] = newNode;
            }
        }
        for (int h = 0; h < height; h++)
        {
            for (int w = 0; w < width; w++)
            {
                List<PF_Node> neighs = new();
                if (w > 0) neighs.Add(nodeGrid[w - 1 + h * width]);
                if (w < width - 1) neighs.Add(nodeGrid[w + 1 + h * width]);
                if (h > 0) neighs.Add(nodeGrid[w + (h - 1) * width]);
                if (h < height - 1) neighs.Add(nodeGrid[w + (h + 1) * width]);
                nodeGrid[w + h * width].SetNeighbors(neighs);
            }
        }
    }
}
