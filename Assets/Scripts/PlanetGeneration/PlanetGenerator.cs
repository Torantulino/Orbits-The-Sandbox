using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlanetGenerator : MonoBehaviour
{
    [Header("Base Settings")]
    public float Radius;
    public int IcosphereSubdivisions;
    public Material PlanetMaterial;
    public bool SmoothNormals;
    public bool Rotate;
    public float TurnSpeed;
    public List<ColorSetting> Colours = new List<ColorSetting>();

    [Header("Ocean:")]
    public bool DrawShore;
    public float minShoreWidth;
    public float maxShoreWidth;

    [Header("Continents:")]
    public int MaxAmountOfContinents;
    public float ContinentsMinSize;
    public float ContinentsMaxSize;
    public float MinLandExtrusionHeight;
    public float MaxLandExtrusionHeight;

    [Header("Mountains:")]
    public float MaxAmountOfMountains;
    public float MountainBaseSize;
    public float MinMountainHeight;
    public float MaxMountainHeight;

    [Header("Bumpiness:")]
    public float MinBumpFactor;
    public float MaxBumpFactor;

    private GameObject planetGameObject;
    private MeshRenderer meshRenderer;
    private MeshFilter meshFilter;

    public void GeneratePlanet()
    {
        if (!planetGameObject)
            planetGameObject = this.gameObject;

        if (!meshRenderer)
        {
            meshRenderer = gameObject.AddComponent<MeshRenderer>();
            meshRenderer.material = PlanetMaterial;
        }

        if (!meshFilter)
            meshFilter = gameObject.AddComponent<MeshFilter>();

        planetMesh = new Mesh();
        GenerateIcosphere();
        SetMesh();
    }

    private void GenerateIcosphere()
    {
        transform.localScale = Vector3.one * Radius;
        meshTriangles.Clear();
        vertices.Clear();

        // create 12 vertices of a icosahedron
        float t = (1.0f + Mathf.Sqrt(5.0f)) / 2.0f;

        vertices.Add(new Vector3(-1f, t, 0f).normalized);
        vertices.Add(new Vector3(1f, t, 0f).normalized);
        vertices.Add(new Vector3(-1f, -t, 0f).normalized);
        vertices.Add(new Vector3(1f, -t, 0f).normalized);

        vertices.Add(new Vector3(0f, -1f, t).normalized);
        vertices.Add(new Vector3(0f, 1f, t).normalized);
        vertices.Add(new Vector3(0f, -1f, -t).normalized);
        vertices.Add(new Vector3(0f, 1f, -t).normalized);

        vertices.Add(new Vector3(t, 0f, -1f).normalized);
        vertices.Add(new Vector3(t, 0f, 1f).normalized);
        vertices.Add(new Vector3(-t, 0f, -1f).normalized);
        vertices.Add(new Vector3(-t, 0f, 1f).normalized);


        // 5 faces around point 0
        meshTriangles.Add(new MeshTriangle(0, 11, 5));
        meshTriangles.Add(new MeshTriangle(0, 5, 1));
        meshTriangles.Add(new MeshTriangle(0, 1, 7));
        meshTriangles.Add(new MeshTriangle(0, 7, 10));
        meshTriangles.Add(new MeshTriangle(0, 10, 11));

        // 5 adjacent faces
        meshTriangles.Add(new MeshTriangle(1, 5, 9));
        meshTriangles.Add(new MeshTriangle(5, 11, 4));
        meshTriangles.Add(new MeshTriangle(11, 10, 2));
        meshTriangles.Add(new MeshTriangle(10, 7, 6));
        meshTriangles.Add(new MeshTriangle(7, 1, 8));

        // 5 faces around point 3
        meshTriangles.Add(new MeshTriangle(3, 9, 4));
        meshTriangles.Add(new MeshTriangle(3, 4, 2));
        meshTriangles.Add(new MeshTriangle(3, 2, 6));
        meshTriangles.Add(new MeshTriangle(3, 6, 8));
        meshTriangles.Add(new MeshTriangle(3, 8, 9));

        // 5 adjacent faces
        meshTriangles.Add(new MeshTriangle(4, 9, 5));
        meshTriangles.Add(new MeshTriangle(2, 4, 11));
        meshTriangles.Add(new MeshTriangle(6, 2, 10));
        meshTriangles.Add(new MeshTriangle(8, 6, 7));
        meshTriangles.Add(new MeshTriangle(9, 8, 1));

        SubdivideVertices();
    }

    private void SubdivideVertices()
    {
        Dictionary<int, int> middlePointVertices = new Dictionary<int, int>();

        // refine triangles
        for (int i = 0; i < IcosphereSubdivisions; i++)
        {
            List<MeshTriangle> new_faces = new List<MeshTriangle>();
            foreach (MeshTriangle tri in meshTriangles)
            {
                // replace triangle by 4 triangles
                int ab = MiddlePointIndex(middlePointVertices, tri.VertexIndices[0], tri.VertexIndices[1]);
                int bc = MiddlePointIndex(middlePointVertices, tri.VertexIndices[1], tri.VertexIndices[2]);
                int ca = MiddlePointIndex(middlePointVertices, tri.VertexIndices[2], tri.VertexIndices[0]);

                new_faces.Add(new MeshTriangle(tri.VertexIndices[0], ab, ca));
                new_faces.Add(new MeshTriangle(tri.VertexIndices[1], bc, ab));
                new_faces.Add(new MeshTriangle(tri.VertexIndices[2], ca, bc));
                new_faces.Add(new MeshTriangle(ab, bc, ca));
            }
            meshTriangles = new_faces;
        }
    }
}
