using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeshTriangle : MonoBehaviour
{
    public List<int> VertexIndices;
    public List<Vector2> UVs;
    public List<MeshTriangle> Neighbours;
    public Color Colour;

    // Creates a Mesh triangle with the given vertices
    public MeshTriangle(int _vertexIndexA, int _vertexIndexB, int _vertexIndexC)
    {
        VertexIndices = new List<int>() { _vertexIndexA, _vertexIndexB, _vertexIndexC };
        UVs = new List<Vector2> { Vector2.zero, Vector2.zero, Vector2.zero };
        Neighbours = new List<MeshTriangle>();
    }

    // Checks if the given triangle is a neighbour to this one.
    // A neighbour is a triangle which shares a side with this one, not just a single vertex.
    public bool IsNeighbouring(MeshTriangle _other)
    {
        int sharedVertices = 0;
        foreach (int index in VertexIndices)
        {
            if (_other.VertexIndices.Contains(index))
            {
                sharedVertices++;
            }
        }
        return sharedVertices > 1;
    }

    // Replaces the specified old neighbour with the specifed new one.
    public void UpdateNeighbour(MeshTriangle _initialNeighbour, MeshTriangle _newNeighbour)
    {
        for (int i = 0; i < Neighbours.Count; i++)
        {
            if (_initialNeighbour == Neighbours[i])
            {
                Neighbours[i] = _newNeighbour;
                return;
            }
        }
    }
}
