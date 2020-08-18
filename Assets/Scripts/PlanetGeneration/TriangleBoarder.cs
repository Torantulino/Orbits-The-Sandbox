using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// An Edge is a boundary between two Polygons. We're going to be working with loops of Edges, so
// each Edge will have a Polygon that's inside the loop and a Polygon that's outside the loop.
// We also want to Split apart the inner and outer Polygons so that they no longer share the same
// vertices. This means the Edge will need to keep track of what the outer Polygon's vertices are
// along its border with the inner Polygon, and what the inner Polygon's vertices are for that
// same border.
public class TriangleBoarder 
{
    public MeshTriangle InnerTriangle;//The Poly that's inside the Edge. The one we'll be extruding or insetting.
    public MeshTriangle OuterTriangle;//The Poly that's outside the Edge. We'll be leaving this one alone.

    public List<int> InnerVertices; //The vertices along this edge, according to the Inner poly.
    public List<int> OuterVertices; //The vertices along this edge, according to the Outer poly.

    public int InwardDirectionVertex;
    //The third vertex of the inner polygon. That is, the one
    //that doesn't touch this edge.

    public TriangleBoarder(MeshTriangle _innerTriangle, MeshTriangle _outerTriangle)
    {
        InnerTriangle = _innerTriangle;
        OuterTriangle = _outerTriangle;

        InnerVertices = new List<int>(2);
        OuterVertices = new List<int>(2);

        // Examine all three of the inner poly's vertices. Add the vertices that it shares with the
        // outer poly to the m_InnerVerts list. We also make a note of which vertex wasn't on the edge
        // and store it for later in m_InwardDirectionVertex.
        for(int i = 0; i < InnerTriangle.VertexIndices.Count; i++)
        {
            if(OuterTriangle.VertexIndices.Contains(InnerTriangle.VertexIndices[i]))
            {
                InnerVertices.Add(InnerTriangle.VertexIndices[i]);
            }
            else
            {
                InwardDirectionVertex = InnerTriangle.VertexIndices[i];
            }
        }

         // Calculate the 'inward direction', a vector that goes from the midpoint of the edge, to the third vertex on
        // the inner poly (the vertex that isn't part of the edge). This will come in handy later if we want to push
        // vertices directly away from the edge.

        // For consistency, we want the 'winding order' of the edge to be the same as that of the inner
        // polygon. So the vertices in the edge are stored in the same order that you would encounter them if
        // you were walking clockwise around the polygon. That means the pair of edge vertices will be:
        // [1st inner poly vertex, 2nd inner poly vertex] or
        // [2nd inner poly vertex, 3rd inner poly vertex] or
        // [3rd inner poly vertex, 1st inner poly vertex]
        //
        // The formula above will give us [1st inner poly vertex, 3rd inner poly vertex] though, so
        // we check for that situation and reverse the vertices.

        if(InnerVertices[0] == InnerTriangle.VertexIndices[0] && InnerVertices[1] == InnerTriangle.VertexIndices[2])
        {
            int temp = InnerVertices[0];
            InnerVertices[0] = InnerVertices[1];
            InnerVertices[1] = temp;
        }

        // No manipulations have happened yet, so the outer and inner Polygons still share the same vertices.
        // We can instantiate m_OuterVerts as a copy of m_InnerVerts.

        OuterVertices = new List<int>(InnerVertices);
    }
}
