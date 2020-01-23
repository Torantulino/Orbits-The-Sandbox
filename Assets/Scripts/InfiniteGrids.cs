using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InfiniteGrids : MonoBehaviour
{
    public Material GLMat;
    public int gridCount = 100; //number of grids to draw on each side of the look position (half size)

    Ray ray;
    float rayDist;
    Vector3 origin;
    public Plane plane = new Plane(Vector3.up, Vector3.zero); //world plane to draw the grid on
    Camera cam;

    private SortedList<float, Grid> grids = new SortedList<float, Grid>();  // <Focus Point, grid>

    private OrbitControls orbitControls;

    public bool render = true;

    //Dictionary<char, GradientAlphaKey[]> alphaKeys = new Dictionary<char, GradientAlphaKey[]>();

    struct Grid
    {
        public List<Vector3> _grid;
        public float _cellSize;
        public AnimationCurve _alphaCurve;
    }

    private Grid CreateGrid(float _cellSize)
    {
        //float _focalPoint = _cellSize / 10.0f;
        float _focalPoint = _cellSize;

        Grid _Grid = new Grid();
        _Grid._cellSize = _cellSize;
        List<Vector3> _grid = new List<Vector3>();

        // Setup alpha keys (time = distance, value = alpha)
        {
            //ALPHA
            Keyframe[] alphaKeys = new Keyframe[3];
            //Start
            alphaKeys[0].time = _focalPoint / 2.0f;
            alphaKeys[0].value = 0.0f;
            //Focal Point
            alphaKeys[1].time = _focalPoint * 2.0f;
            alphaKeys[1].value = 0.4f;
            //End 
            alphaKeys[2].time = _focalPoint * 100.0f;
            alphaKeys[2].value = 0.0f;
            // Set
            AnimationCurve curve = new AnimationCurve();

            for (int i = 0; i < 3; i++)
                curve.AddKey(alphaKeys[i]);

            //Set
            _Grid._alphaCurve = curve;
        }

        // Create Grid Structure
        //Major x line
        _grid.Add(new Vector3(gridCount * _cellSize, 0, 0));
        _grid.Add(new Vector3(-gridCount * _cellSize, 0, 0));
        //Major z line
        _grid.Add(new Vector3(0, 0, gridCount * _cellSize));
        _grid.Add(new Vector3(0, 0, -gridCount * _cellSize));

        for (float i = 1.0f; i < gridCount + 1; i++)
        {
            //positive x lines
            _grid.Add(new Vector3(i * _cellSize, 0.0f, gridCount * _cellSize));
            _grid.Add(new Vector3(i * _cellSize, 0.0f, -gridCount * _cellSize));
            //negative x lines
            _grid.Add(new Vector3(-i * _cellSize, 0.0f, gridCount * _cellSize));
            _grid.Add(new Vector3(-i * _cellSize, 0.0f, -gridCount * _cellSize));
            //positive z lines
            _grid.Add(new Vector3(gridCount * _cellSize, 0.0f, i * _cellSize));
            _grid.Add(new Vector3(-gridCount * _cellSize, 0.0f, i * _cellSize));
            //negative z lines
            _grid.Add(new Vector3(gridCount * _cellSize, 0.0f, -i * _cellSize));
            _grid.Add(new Vector3(-gridCount * _cellSize, 0.0f, -i * _cellSize));
        }

        _Grid._grid = _grid;
        return _Grid;
    }

    void Start()
    {

        cam = GetComponent<Camera>();
        orbitControls = GetComponent<OrbitControls>();

        // Create and add grids
        grids.Add(0.0f, CreateGrid(1.0f));
        grids.Add(1.0f, CreateGrid(10.0f));
        grids.Add(10.0f, CreateGrid(100.0f));
        grids.Add(100.0f, CreateGrid(1000.0f));
    }

    void LateUpdate()
    {

    }

    void OnRenderObject()
    {
        if (Camera.current.tag == "MainCamera")
        {
            try
            {
                // Calculate new plane
                origin = orbitControls.FocalObject.position; //This essentially should be the same thing as below. The plane is used elsewhere in the game so is necessary.
                plane = new Plane(Vector3.up, origin);

                // Get grid origin position
                ray = cam.ScreenPointToRay(new Vector3(Screen.width / 2.0f, Screen.height / 2.0f, 0.0f));
                plane.Raycast(ray, out rayDist);
                origin = ray.GetPoint(rayDist);


                // Get distance from origin to camera
                float cameraDistance = Vector3.Distance(cam.transform.position, origin);

                // Render
                if (render)
                {
                    int i = 0;
                    foreach (KeyValuePair<float, Grid> pair in grids)
                    {
                        // Get grid from pair
                        Grid grid = pair.Value;

                        // Don't render grid 0 when cam is further than 400.0f
                        if (i == 0 && cameraDistance > 400.0f)
                        {
                            i++;
                            continue;
                        }

                        GL.PushMatrix();
                        GLMat.SetPass(0);
                        GL.Begin(GL.LINES);

                        // Set line colour
                        GL.Color(new Color(Color.white.r, Color.white.g, Color.white.b, grid._alphaCurve.Evaluate(orbitControls._TargetDistance)));

                        // Render grid vertices
                        foreach (Vector3 vertex in grid._grid)
                        {
                            GL.Vertex(vertex + origin);
                        }

                        GL.End();
                        GL.PopMatrix();
                        i++;
                    }
                }
            }
            catch
            {

            }
        }
    }
}
