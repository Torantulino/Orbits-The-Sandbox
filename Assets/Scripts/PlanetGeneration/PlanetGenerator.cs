using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

public class PlanetGenerator : MonoBehaviour
{
    [Header("Base Settings:")]
    public float Radius;
    public int IcosphereSubdivisions;
    public Material PlanetMaterial;
    public bool SmoothNormals;
    public bool Rotate;
    public float TurnSpeed;
    public Dictionary<int, Color> Colors = new Dictionary<int, Color>()
    {
        {-2, Color.cyan},         //Shore
        {-1, Color.gray},         //Cliffs
        {0, Color.blue},          //Ocean
        {1, Color.magenta},       //Land1
        {2, Color.white}, //Land2
    };

    [Header("Oceans:")]
    public bool DrawShore;
    public float MinShoreWidth;
    public float MaxShoreWidth;

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
    private MeshCollider meshCollider;
    private Mesh planetMesh;

    private List<MeshTriangle> MeshTriangles = new List<MeshTriangle>();
    private List<Vector3> Vertices = new List<Vector3>();

    private int current_layer = 1;
    private Dictionary<int, TriangleHashSet> layers = new Dictionary<int, TriangleHashSet>();

    private TriangleHashSet oceans;
    //private TriangleHashSet continents;
    private TriangleHashSet continentsSides;
    private TriangleHashSet mountains;

    private float brushSize = 0.1f;
    private int noLayers = 3;
    private float layerHeight = 0.05f;
    private UserInterface userInterface;
    public void Start()
    {
        userInterface = FindObjectOfType<UserInterface>();
        Physics.queriesHitTriggers = true;
        StartGeneration();

        for (int i = 0; i < noLayers; i++)
            layers[i] = new TriangleHashSet();
        continentsSides = new TriangleHashSet();
        layers[0].UnionWith(MeshTriangles);
    }

    public Color FindColor(int _layer)
    {
        for (int i = 0; i < Colors.Count; i++)
        {
            if (Colors.ContainsKey(_layer))
                return Colors[_layer];
        }

        return Color.magenta;
    }

    public void SetBrushSize(float _value)
    {
        brushSize = _value;
    }

    public void StartGeneration()
    {
        planetGameObject = this.gameObject;
        planetGameObject.transform.parent = transform;

        if (meshRenderer == null)
        {
            meshRenderer = planetGameObject.AddComponent<MeshRenderer>();
            meshRenderer.material = PlanetMaterial;
        }

        if (meshFilter == null)
            meshFilter = planetGameObject.AddComponent<MeshFilter>();

        if (meshCollider == null)
            meshCollider = planetGameObject.AddComponent<MeshCollider>();

        meshCollider.sharedMesh = meshFilter.mesh;
        meshCollider.convex = true;
        meshCollider.isTrigger = true;

        planetMesh = new Mesh();
        GenerateIcosphere();
        CalculateNeighbors();

        GenerateMesh();
    }


    //todo: change to OnMouseDown event
    void OnMouseOver()
    {
        // Left Click
        if (Input.GetMouseButtonDown(0))
        {
            RaycastHit hit;

            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out hit))
            {
                Vector3 pointOnSphere = (transform.InverseTransformPoint(hit.point)).normalized;
                DrawLand(pointOnSphere);
            }
        }
    }

    /// <summary>
    /// Update is called every frame, if the MonoBehaviour is enabled.
    /// </summary>
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            current_layer = Mathf.Max(0, current_layer - 1);
            userInterface.SetSelector(current_layer);
        }

        if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            current_layer = Mathf.Min(2, current_layer + 1);
            userInterface.SetSelector(current_layer);
        }

        if (Rotate)
        {
            transform.Rotate(Vector3.up, TurnSpeed * Time.deltaTime);
        }
    }

    public void Save(string _filename, float _radius, float _mass)
    {
        // Create prefab
        GameObject prefab = GameObject.Instantiate(Resources.Load<GameObject>("Prefabs/Objects/PlanetGenerator/PlanetGenSource"));
        Mesh mesh = meshFilter.mesh;
        MeshFilter pMFilter = prefab.GetComponentInChildren<MeshFilter>();
        MeshRenderer pMRenderer = prefab.GetComponentInChildren<MeshRenderer>();
        PhysicsObject physicsObject = prefab.GetComponent<PhysicsObject>();
        SphereCollider sphereCollider = prefab.GetComponent<SphereCollider>();

        // Create folder
        string guid = AssetDatabase.CreateFolder("Assets/Resources/Prefabs/Objects/PlanetGenerator", _filename);
        AssetDatabase.SaveAssets();
        string newFolderPath = AssetDatabase.GUIDToAssetPath(guid);

        // Export Mesh
        try
        {
            AssetDatabase.CreateAsset(mesh, "Assets/Resources/Models/PlanetGenerator/" + _filename + ".asset");
        }
        catch (UnityEngine.UnityException e)
        {
            Debug.LogError("<b><color=red>This planet is an exact duplicate of another previously exported disk!</color></b> Please ensure it is unique in either colour or mesh!");
            return;
        }
        AssetDatabase.SaveAssets();

        // Load resources for prefab creation
        Mesh pMesh = Resources.Load<Mesh>("Models/PlanetGenerator/" + _filename);
        Material pMat = Resources.Load<Material>("Materials/PlanetMaterial");

        // Assign to prefab
        pMFilter.mesh = pMesh;
        pMRenderer.material = pMat;

        // Assign properties
        prefab.transform.localScale = Vector3.one * _radius;
        physicsObject.rb.mass = _mass;
        // defaults
        physicsObject.isStar = false;
        physicsObject.isTrailRenderer = true;
        physicsObject.spawnWithOrbit = true;
        sphereCollider.radius = 1.0f;

        // Save prefab
        PrefabUtility.SaveAsPrefabAsset(prefab, newFolderPath + "/" + _filename + ".prefab");
    }

    public void Load(string _filename)
    {
    }


    private void DrawLand(Vector3 _pointOnSphere)
    {
        if (!layers.ContainsKey(current_layer))
            layers[current_layer] = new TriangleHashSet();

        TriangleHashSet addedLandmass = GetTriangles(_pointOnSphere, brushSize, MeshTriangles);

        // If any of these triangles are currently in other layers, remove the former from the latter.
        for (int i = 0; i < layers.Count; i++)
        {
            foreach (MeshTriangle newTriangle in addedLandmass)
            {
                if (layers[i].Contains(newTriangle))
                    layers[i].Remove(newTriangle);
            }
        }

        layers[current_layer].UnionWith(addedLandmass);
        layers[current_layer].ApplyColor(Colors[current_layer]);

        continentsSides.UnionWith(SetHeight(layers[current_layer], current_layer * layerHeight));
        continentsSides.ApplyColor(Colors[-1]);


        foreach (MeshTriangle triangle in layers[current_layer])
        {
            Vector3[] currentVerts = new Vector3[3];
            for (int i = 0; i < triangle.VertexIndices.Count; i++)
            {
                currentVerts[i] = Vertices[triangle.VertexIndices[i]];
            }
            AddBumpyness(currentVerts);
            for (int i = 0; i < triangle.VertexIndices.Count; i++)
            {
                Vertices[triangle.VertexIndices[i]] = currentVerts[i];
            }
        }

        GenerateMesh();
    }

    public void ApplyColours()
    {
        layers[0].ApplyColor(Colors[0]);
        layers[1].ApplyColor(Colors[1]);
        layers[2].ApplyColor(Colors[2]);
        continentsSides.ApplyColor(Colors[-1]);
    }

    public void SetColour(int _layer, Color _colour)
    {
        if (Colors.ContainsKey(_layer))
            Colors[_layer] = _colour;
        else
            Debug.LogError("Color " + _layer + " not found!");
    }

    private void AddContinents()
    {
        layers[1] = new TriangleHashSet();

        for (int i = 0; i < MaxAmountOfContinents; i++)
        {
            float continentSize = Random.Range(ContinentsMinSize, ContinentsMaxSize);
            TriangleHashSet addedLandmass = GetTriangles(Random.onUnitSphere, continentSize, MeshTriangles);

            layers[1].UnionWith(addedLandmass);
        }
        layers[1].ApplyColor(Colors[1]);

        continentsSides = Extrude(layers[1], Random.Range(MinLandExtrusionHeight, MaxLandExtrusionHeight));
        continentsSides.ApplyColor(Colors[1]);

        foreach (MeshTriangle triangle in layers[1])
        {
            Vector3[] currentVerts = new Vector3[3];
            for (int i = 0; i < triangle.VertexIndices.Count; i++)
            {
                currentVerts[i] = Vertices[triangle.VertexIndices[i]];
            }
            AddBumpyness(currentVerts);
            for (int i = 0; i < triangle.VertexIndices.Count; i++)
            {
                Vertices[triangle.VertexIndices[i]] = currentVerts[i];
            }
        }
    }


    private void UpdateOceans()
    {
        oceans = new TriangleHashSet();

        foreach (MeshTriangle triangle in MeshTriangles)
        {
            if (!layers[1].Contains(triangle))
                oceans.Add(triangle);
        }

        TriangleHashSet ocean = new TriangleHashSet(oceans);
        ocean.ApplyColor(Colors[0]);
        if (DrawShore)
        {
            TriangleHashSet shore;
            shore = SetHeight(ocean, Random.Range(MinShoreWidth, MaxShoreWidth));
            shore.ApplyColor(Colors[-2]);

            shore = SetHeight(ocean, -0.02f);
            shore.ApplyColor(Colors[0]);

            shore = SetHeight(ocean, 0.02f);
            shore.ApplyColor(Colors[0]);
        }
    }

    private void AddOceans()
    {
        oceans = new TriangleHashSet();

        foreach (MeshTriangle triangle in MeshTriangles)
        {
            if (!layers[1].Contains(triangle))
                oceans.Add(triangle);
        }

        TriangleHashSet ocean = new TriangleHashSet(oceans);
        ocean.ApplyColor(Colors[0]);
        if (DrawShore)
        {
            TriangleHashSet shore;
            shore = Inset(ocean, Random.Range(MinShoreWidth, MaxShoreWidth));
            shore.ApplyColor(Colors[-2]);

            shore = Extrude(ocean, -0.02f);
            shore.ApplyColor(Colors[0]);

            shore = Inset(ocean, 0.02f);
            shore.ApplyColor(Colors[0]);
        }
    }

    private void AddMountains()
    {
        TriangleHashSet sides;

        for (int i = 0; i < MaxAmountOfMountains; i++)
        {
            mountains = GetTriangles(Random.onUnitSphere, MountainBaseSize, layers[1]);
            mountains.ApplyColor(Colors[-1]);
            layers[1].UnionWith(mountains);
            sides = Extrude(mountains, Random.Range(MinMountainHeight, MaxMountainHeight));
            sides.ApplyColor(Colors[-1]);

            mountains.ApplyColor(Colors[2]);
            mountains = GetTriangles(Random.onUnitSphere, MountainBaseSize * -.33f, layers[1]);
            layers[1].UnionWith(mountains);
            sides = Extrude(mountains, Random.Range(MinMountainHeight, MaxMountainHeight));
            mountains.ApplyColor(Colors[2]);

            mountains = GetTriangles(Random.onUnitSphere, MountainBaseSize * -.66f, layers[1]);
            layers[1].UnionWith(mountains);
            sides = Extrude(mountains, Random.Range(MinMountainHeight, MaxMountainHeight));
            mountains.ApplyColor(Colors[2]);
        }
    }

    public void GenerateIcosphere()
    {
        this.transform.localScale = Vector3.one * Radius;
        MeshTriangles = new List<MeshTriangle>();
        Vertices = new List<Vector3>();

        float t = (1.0f + Mathf.Sqrt(5.0f)) / 2.0f;

        Vertices.Add(new Vector3(-1, t, 0).normalized);
        Vertices.Add(new Vector3(1, t, 0).normalized);
        Vertices.Add(new Vector3(-1, -t, 0).normalized);
        Vertices.Add(new Vector3(1, -t, 0).normalized);
        Vertices.Add(new Vector3(0, -1, t).normalized);
        Vertices.Add(new Vector3(0, 1, t).normalized);
        Vertices.Add(new Vector3(0, -1, -t).normalized);
        Vertices.Add(new Vector3(0, 1, -t).normalized);
        Vertices.Add(new Vector3(t, 0, -1).normalized);
        Vertices.Add(new Vector3(t, 0, 1).normalized);
        Vertices.Add(new Vector3(-t, 0, -1).normalized);
        Vertices.Add(new Vector3(-t, 0, 1).normalized);

        MeshTriangles.Add(new MeshTriangle(0, 11, 5));
        MeshTriangles.Add(new MeshTriangle(0, 5, 1));
        MeshTriangles.Add(new MeshTriangle(0, 1, 7));
        MeshTriangles.Add(new MeshTriangle(0, 7, 10));
        MeshTriangles.Add(new MeshTriangle(0, 10, 11));
        MeshTriangles.Add(new MeshTriangle(1, 5, 9));
        MeshTriangles.Add(new MeshTriangle(5, 11, 4));
        MeshTriangles.Add(new MeshTriangle(11, 10, 2));
        MeshTriangles.Add(new MeshTriangle(10, 7, 6));
        MeshTriangles.Add(new MeshTriangle(7, 1, 8));
        MeshTriangles.Add(new MeshTriangle(3, 9, 4));
        MeshTriangles.Add(new MeshTriangle(3, 4, 2));
        MeshTriangles.Add(new MeshTriangle(3, 2, 6));
        MeshTriangles.Add(new MeshTriangle(3, 6, 8));
        MeshTriangles.Add(new MeshTriangle(3, 8, 9));
        MeshTriangles.Add(new MeshTriangle(4, 9, 5));
        MeshTriangles.Add(new MeshTriangle(2, 4, 11));
        MeshTriangles.Add(new MeshTriangle(6, 2, 10));
        MeshTriangles.Add(new MeshTriangle(8, 6, 7));
        MeshTriangles.Add(new MeshTriangle(9, 8, 1));

        Subdivide();
    }

    public void Subdivide()
    {
        var midPointCache = new Dictionary<int, int>();

        for (int i = 0; i < IcosphereSubdivisions; i++)
        {
            var newPolys = new List<MeshTriangle>();
            foreach (var poly in MeshTriangles)
            {
                int a = poly.VertexIndices[0];
                int b = poly.VertexIndices[1];
                int c = poly.VertexIndices[2];

                int ab = GetMidPointIndex(midPointCache, a, b);
                int bc = GetMidPointIndex(midPointCache, b, c);
                int ca = GetMidPointIndex(midPointCache, c, a);

                newPolys.Add(new MeshTriangle(a, ab, ca));
                newPolys.Add(new MeshTriangle(b, bc, ab));
                newPolys.Add(new MeshTriangle(c, ca, bc));
                newPolys.Add(new MeshTriangle(ab, bc, ca));
            }

            MeshTriangles = newPolys;
        }
    }

    public int GetMidPointIndex(Dictionary<int, int> cache, int indexA, int indexB)
    {
        int smallerIndex = Mathf.Min(indexA, indexB);
        int greaterIndex = Mathf.Max(indexA, indexB);
        int key = (smallerIndex << 16) + greaterIndex;

        // If a midpoint is already defined, just return it.

        int ret;
        if (cache.TryGetValue(key, out ret))
            return ret;

        // If we're here, it's because a midpoint for these two
        // vertices hasn't been created yet. Let's do that now!

        Vector3 p1 = Vertices[indexA];
        Vector3 p2 = Vertices[indexB];
        Vector3 middle = Vector3.Lerp(p1, p2, 0.5f).normalized;

        ret = Vertices.Count;
        Vertices.Add(middle);

        // Add our new midpoint to the cache so we don't have
        // to do this again. =)

        cache.Add(key, ret);
        return ret;
    }

    public void CalculateNeighbors()
    {
        foreach (MeshTriangle poly in MeshTriangles)
        {
            foreach (MeshTriangle other_poly in MeshTriangles)
            {
                if (poly == other_poly)
                    continue;

                if (poly.IsNeighbouring(other_poly))
                    poly.Neighbours.Add(other_poly);
            }
        }
    }

    public List<int> CloneVertices(List<int> old_verts)
    {
        List<int> new_verts = new List<int>();
        foreach (int old_vert in old_verts)
        {
            Vector3 cloned_vert = Vertices[old_vert];
            new_verts.Add(Vertices.Count);
            Vertices.Add(cloned_vert);
        }
        return new_verts;
    }

    public TriangleHashSet StitchPolys(TriangleHashSet polys, out BoarderHashSet stitchedEdge)
    {
        TriangleHashSet stichedPolys = new TriangleHashSet();

        stichedPolys.IterationIndex = Vertices.Count;

        stitchedEdge = polys.CreateBoarderHashSet();
        var originalVerts = stitchedEdge.RemoveDublicates();
        var newVerts = CloneVertices(originalVerts);

        stitchedEdge.Seperate(originalVerts, newVerts);

        foreach (TriangleBoarder edge in stitchedEdge)
        {
            // Create new polys along the stitched edge. These
            // will connect the original poly to its former
            // neighbor.

            var stitch_poly1 = new MeshTriangle(edge.OuterVertices[0],
                                           edge.OuterVertices[1],
                                           edge.InnerVertices[0]);
            var stitch_poly2 = new MeshTriangle(edge.OuterVertices[1],
                                           edge.InnerVertices[1],
                                           edge.InnerVertices[0]);
            // Add the new stitched faces as neighbors to
            // the original Polys.
            edge.InnerTriangle.UpdateNeighbour(edge.OuterTriangle, stitch_poly2);
            edge.OuterTriangle.UpdateNeighbour(edge.InnerTriangle, stitch_poly1);

            MeshTriangles.Add(stitch_poly1);
            MeshTriangles.Add(stitch_poly2);

            stichedPolys.Add(stitch_poly1);
            stichedPolys.Add(stitch_poly2);
        }

        //Swap to the new vertices on the inner polys.
        foreach (MeshTriangle poly in polys)
        {
            for (int i = 0; i < 3; i++)
            {
                int vert_id = poly.VertexIndices[i];
                if (!originalVerts.Contains(vert_id))
                    continue;
                int vert_index = originalVerts.IndexOf(vert_id);
                poly.VertexIndices[i] = newVerts[vert_index];
            }
        }

        return stichedPolys;
    }

    // Returns created 'cliffs'
    public TriangleHashSet SetHeight(TriangleHashSet polys, float height)
    {
        BoarderHashSet stitchedEdge;
        TriangleHashSet stitchedPolys = StitchPolys(polys, out stitchedEdge);
        List<int> verts = polys.RemoveDublicates();

        foreach (int vert in verts)
        {
            Vector3 v = Vertices[vert];
            v = v.normalized * (1.0f + height);
            Vertices[vert] = v;
        }

        return stitchedPolys;

    }
    public TriangleHashSet Extrude(TriangleHashSet polys, float height)
    {
        BoarderHashSet stitchedEdge;
        TriangleHashSet stitchedPolys = StitchPolys(polys, out stitchedEdge);
        List<int> verts = polys.RemoveDublicates();

        // Take each vertex in this list of polys, and push it
        // away from the center of the Planet by the height
        // parameter.

        foreach (int vert in verts)
        {
            Vector3 v = Vertices[vert];
            v = v.normalized * (v.magnitude + height);
            Vertices[vert] = v;
        }

        return stitchedPolys;
    }

    public TriangleHashSet Inset(TriangleHashSet polys, float insetDistance)
    {
        BoarderHashSet stitchedEdge;
        TriangleHashSet stitchedPolys = StitchPolys(polys, out stitchedEdge);

        Dictionary<int, Vector3> inwardDirections = stitchedEdge.GetInwardDirections(Vertices);

        // Push each vertex inwards, then correct
        // it's height so that it's as far from the center of
        // the planet as it was before.

        foreach (KeyValuePair<int, Vector3> kvp in inwardDirections)
        {
            int vertIndex = kvp.Key;
            Vector3 inwardDirection = kvp.Value;

            Vector3 vertex = Vertices[vertIndex];
            float originalHeight = vertex.magnitude;

            vertex += inwardDirection * insetDistance;
            vertex = vertex.normalized * originalHeight;
            Vertices[vertIndex] = vertex;
        }

        return stitchedPolys;
    }

    public TriangleHashSet GetTriangles(Vector3 center, float radius, IEnumerable<MeshTriangle> source)
    {
        TriangleHashSet newSet = new TriangleHashSet();

        foreach (MeshTriangle p in source)
        {
            foreach (int vertexIndex in p.VertexIndices)
            {
                // This loop expands the search 'vertically' to ensure distant layers can be drawn over one another.
                bool found = false;
                for (int i = 0; i < noLayers; i++)
                {
                    float distanceToSphere = Vector3.Distance(center * (1.0f + (layerHeight * i)), Vertices[vertexIndex]);

                    if (distanceToSphere <= radius)
                    {
                        newSet.Add(p);
                        found = true;
                    }
                    if (found)
                        break;
                }
            }
        }

        return newSet;
    }

    public void GenerateMesh()
    {
        int vertexCount = MeshTriangles.Count * 3;

        int[] indices = new int[vertexCount];

        Vector3[] vertices = new Vector3[vertexCount];
        Vector3[] normals = new Vector3[vertexCount];
        Color32[] colors = new Color32[vertexCount];
        Vector2[] uvs = new Vector2[vertexCount];

        for (int i = 0; i < MeshTriangles.Count; i++)
        {
            var poly = MeshTriangles[i];

            indices[i * 3 + 0] = i * 3 + 0;
            indices[i * 3 + 1] = i * 3 + 1;
            indices[i * 3 + 2] = i * 3 + 2;

            vertices[i * 3 + 0] = Vertices[poly.VertexIndices[0]];
            vertices[i * 3 + 1] = Vertices[poly.VertexIndices[1]];
            vertices[i * 3 + 2] = Vertices[poly.VertexIndices[2]];

            uvs[i * 3 + 0] = poly.UVs[0];
            uvs[i * 3 + 1] = poly.UVs[1];
            uvs[i * 3 + 2] = poly.UVs[2];

            colors[i * 3 + 0] = poly.Color;
            colors[i * 3 + 1] = poly.Color;
            colors[i * 3 + 2] = poly.Color;

            if (SmoothNormals)
            {
                normals[i * 3 + 0] = Vertices[poly.VertexIndices[0]].normalized;
                normals[i * 3 + 1] = Vertices[poly.VertexIndices[1]].normalized;
                normals[i * 3 + 2] = Vertices[poly.VertexIndices[2]].normalized;
            }
            else
            {
                Vector3 ab = Vertices[poly.VertexIndices[1]] - Vertices[poly.VertexIndices[0]];
                Vector3 ac = Vertices[poly.VertexIndices[2]] - Vertices[poly.VertexIndices[0]];

                Vector3 normal = Vector3.Cross(ab, ac).normalized;

                normals[i * 3 + 0] = normal;
                normals[i * 3 + 1] = normal;
                normals[i * 3 + 2] = normal;
            }
        }

        planetMesh.vertices = vertices;
        planetMesh.normals = normals;
        planetMesh.colors32 = colors;
        planetMesh.uv = uvs;

        planetMesh.SetTriangles(indices, 0);

        meshFilter.mesh = planetMesh;
        meshCollider.sharedMesh = planetMesh;
    }

    Vector3[] AddBumpyness(Vector3[] verts)
    {
        Dictionary<Vector3, List<int>> dictionary = new Dictionary<Vector3, List<int>>();
        for (int x = 0; x < verts.Length; x++)
        {
            if (!dictionary.ContainsKey(verts[x]))
            {
                dictionary.Add(verts[x], new List<int>());
            }
            dictionary[verts[x]].Add(x);
        }
        foreach (KeyValuePair<Vector3, List<int>> pair in dictionary)
        {
            Vector3 newPos = pair.Key * Random.Range(MinBumpFactor, MaxBumpFactor);
            foreach (int i in pair.Value)
            {
                verts[i] = newPos;
            }
        }
        return verts;
    }
}
