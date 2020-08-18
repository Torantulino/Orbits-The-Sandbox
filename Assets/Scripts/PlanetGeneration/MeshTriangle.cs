using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeshTriangle 
{
    public List<int>            VertexIndices;
    public List<Vector2>        UVs;
    public List<MeshTriangle>   Neighbours;
    public Color Color;

    public MeshTriangle(int _vertexIndexA, int _vertexIndexB, int _vertexIndexC)
    {
        VertexIndices = new List<int>() {_vertexIndexA,_vertexIndexB,_vertexIndexC};
        UVs = new List<Vector2>{Vector2.zero,Vector2.zero,Vector2.zero};
        Neighbours = new List<MeshTriangle>();
    }

    public bool IsNeighbouring(MeshTriangle _other)
    {
        int sharedVertices = 0;
        foreach(int index in VertexIndices)
        {
            if(_other.VertexIndices.Contains(index))
            {
                sharedVertices++;
            }
        }
        return sharedVertices > 1;
    }

    public void UpdateNeighbour(MeshTriangle _initialNeighbour, MeshTriangle _newNeighbour)
    {
        for(int i = 0; i < Neighbours.Count; i++)
        {
            if(_initialNeighbour == Neighbours[i])
            {
                Neighbours[i] = _newNeighbour;
                return;
            }
        }
    }
}
