using System.Collections.Generic;
using UnityEngine;

public class PF_WNode : MonoBehaviour
{
    [SerializeField] List<PF_WNode> neighbors = new();
    [SerializeField] public float cost;
    [SerializeField] private int x, y;
    private Renderer rend;
    public List<PF_WNode> Neighbors => neighbors;
    private LineOfSight los;
    private List<PF_WNode> onRange;

    public int X => x;
    public int Y => y;

    private void Awake()
    {
        rend = GetComponent<Renderer>();
        if(TryGetComponent(out LineOfSight los))
        {
            this.los = los;
        }
    }

    public void SetIndexes(int x, int y)
    {
        this.x = x;
        this.y = y;
    }
    public void SetNeighbors(List<PF_WNode> neighbors)
    {
        this.neighbors = neighbors;
    }

    public void SetColor(Color color)
    {
        rend.material.color = color;
    }
}
