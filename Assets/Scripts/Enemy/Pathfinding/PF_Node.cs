using System.Collections.Generic;
using UnityEngine;

public class PF_Node : MonoBehaviour
{
    [SerializeField] List<PF_Node> neighbors = new();
    [SerializeField] private int x, y;
    private Renderer rend;
    public List<PF_Node> Neighbors => neighbors;

    public int X => x;
    public int Y => y;

    private void Awake()
    {
        rend = GetComponent<Renderer>();
    }

    public void SetIndexes(int x, int y)
    {
        this.x = x;
        this.y = y;
    }
    public void SetNeighbors(List<PF_Node> neighbors)
    {
        this.neighbors = neighbors;
    }

    public void SetColor(Color color)
    {
        rend.material.color = color;
    }
}
