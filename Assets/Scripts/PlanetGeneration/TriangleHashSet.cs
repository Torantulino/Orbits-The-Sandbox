    
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TriangleHashSet : HashSet<MeshTriangle>
{
    public TriangleHashSet() {}
    public TriangleHashSet(TriangleHashSet source) : base(source) {}
    public int IterationIndex = -1;

    public BoarderHashSet CreateBoarderHashSet()
    {
        BoarderHashSet boarderSet = new BoarderHashSet();
        foreach (MeshTriangle triangle in this)
        {
            foreach (MeshTriangle neighbor in triangle.Neighbours)
            {
                if (this.Contains(neighbor))
                {
                    continue;
                }
                TriangleBoarder boarder = new TriangleBoarder(triangle, neighbor);
                boarderSet.Add(boarder);
            }
        }
        return boarderSet;
    }

    public List<int> RemoveDublicates()
    {
        List<int> vertices = new List<int>();
        foreach (MeshTriangle triangle in this)
        {
            foreach (int vertexIndex in triangle.VertexIndices)
            {
                if (!vertices.Contains(vertexIndex))
                {
                    vertices.Add(vertexIndex);
                }      
            }
        }
        return vertices;
    }

    public void ApplyColor(Color _color)
    {
        foreach (MeshTriangle triangle in this)
            triangle.Color = _color;
    }
}

