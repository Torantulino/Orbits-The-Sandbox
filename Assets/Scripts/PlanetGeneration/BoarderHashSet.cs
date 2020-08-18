using System.Collections.Generic;
using UnityEngine;

public class BoarderHashSet : HashSet<TriangleBoarder>
{
    public void Seperate(List<int> _originalVertices, List<int> _addedVertices)
    {
        foreach(TriangleBoarder boarder in this)
        {
            for(int i = 0; i < 2; i++)
            {
                boarder.InnerVertices[i] = _addedVertices[_originalVertices.IndexOf(boarder.OuterVertices[i])];
            }
        }
    }

    public List<int> RemoveDublicates()
    {
        List<int> vertices = new List<int>();
        foreach (TriangleBoarder boarder in this)
        {
            foreach (int vertexIndex in boarder.OuterVertices)
            {
                if (!vertices.Contains(vertexIndex))
                {
                    vertices.Add(vertexIndex);
                }
            }
        }
        return vertices;
    }

    public Dictionary<int, Vector3> GetInwardDirections(List<Vector3> vertexPositions)
    {
        Dictionary<int,Vector3> inwardDirections = new Dictionary<int, Vector3>();
        Dictionary<int,int> numItems = new Dictionary<int, int>();

        foreach(TriangleBoarder boarder in this)
        {
            Vector3 innerVertexPosition = vertexPositions[boarder.InwardDirectionVertex];
            Vector3 boarderPosA   = vertexPositions[boarder.InnerVertices[0]];
            Vector3 boarderPosB   = vertexPositions[boarder.InnerVertices[1]];
            Vector3 boarderCenter = Vector3.Lerp(boarderPosA, boarderPosB, 0.5f);
            Vector3 innerVector = (innerVertexPosition - boarderCenter).normalized;

            for(int i = 0; i < 2; i++)
            {
                int boarderVertex = boarder.InnerVertices[i];
                if (inwardDirections.ContainsKey(boarderVertex))
                {
                    inwardDirections[boarderVertex] += innerVector;
                    numItems[boarderVertex]++;
                }
                else
                {
                    inwardDirections.Add(boarderVertex, innerVector);
                    numItems.Add(boarderVertex, 1);
                }
            }
        }

        foreach(KeyValuePair<int, int> kvp in numItems)
        {
            int vertexIndex               = kvp.Key;
            int contributionsToThisVertex = kvp.Value;
            inwardDirections[vertexIndex] = (inwardDirections[vertexIndex] / contributionsToThisVertex).normalized;
        }

        return inwardDirections;
    }   
}
