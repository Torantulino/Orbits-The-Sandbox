using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InfiniteGrids : MonoBehaviour
{
  public Material GLMat;
    public int gridCount = 100; //number of grids to draw on each side of the look position (half size)
    public float gridSize = 1.0f; //spacing between gridlines
 
    Ray ray;
    float rayDist;
    Vector3 lookPosition;
    public Plane plane = new Plane( Vector3.up, Vector3.zero ); //world plane to draw the grid on
    Camera cam;

    public float gridBrightness = 1.0f; 
    Color highWhite;
    Color medWhite;
    Color lowWhite;
    private List<Grid> grids = new List<Grid>();

    struct Grid
    {
        public List<Vector3> _grid;
        public float _cellSize;
    }

    private Grid CreateGrid(float _cellSize)
    {
        Grid _Grid = new Grid();
        _Grid._cellSize = _cellSize;
        List<Vector3> _grid = new List<Vector3>();

        //Major x line
        _grid.Add(new Vector3( gridCount * _cellSize, 0, 0 ));
        _grid.Add(new Vector3( -gridCount * _cellSize, 0, 0 ));
        //Major z line
        _grid.Add(new Vector3( 0, 0, gridCount * _cellSize ));
        _grid.Add(new Vector3( 0, 0, -gridCount * _cellSize ));
 
        for (int i = 1; i < gridCount + 1; i++) {
            //positive x lines
            _grid.Add(new Vector3( i * _cellSize, 0, gridCount * _cellSize ));
            _grid.Add(new Vector3( i * _cellSize, 0, -gridCount * _cellSize ));
            //negative x lines
            _grid.Add(new Vector3( -i * _cellSize, 0, gridCount * _cellSize ));
            _grid.Add(new Vector3( -i * _cellSize, 0, -gridCount * _cellSize ));
            //positive z lines
            _grid.Add(new Vector3( gridCount * _cellSize, 0, i * _cellSize ));
            _grid.Add(new Vector3( -gridCount * _cellSize, 0, i * _cellSize ));
            //negative z lines
            _grid.Add(new Vector3( gridCount * _cellSize, 0, -i * _cellSize ));
            _grid.Add(new Vector3( -gridCount * _cellSize, 0, -i * _cellSize ));
        }

        _Grid._grid = _grid;
        return _Grid;
    }

    void Start () {
        highWhite = new Color(1.0f, 1.0f, 1.0f, 0.8f);
        medWhite = new Color(1.0f, 1.0f, 1.0f, 0.3f);
        lowWhite = new Color(1.0f, 1.0f, 1.0f, 0.05f);
        
        cam = GetComponent<Camera>();


        grids.Add(CreateGrid(1));
        grids.Add(CreateGrid(10));
        grids.Add(CreateGrid(100));
        //grids.Add(CreateGrid(1000));
    }
 
    void LateUpdate () {
        ray = cam.ScreenPointToRay( new Vector3( Screen.width / 2, Screen.height / 2, 0 ) );
        plane.Raycast( ray, out rayDist );
        lookPosition = ray.GetPoint( rayDist );
    }
    
    void OnPostRender () 
    {   
        int i = 0;
        foreach (Grid grid in grids)
        {
            if(i==0 && Vector3.Distance(cam.transform.position, lookPosition) > 400.0f)
            {
                i++;
                continue;
            }
            GL.PushMatrix();
            GLMat.SetPass(0);
            GL.Begin(GL.LINES);

            GL.Color(Color.white * Mathf.Min((grid._cellSize / Vector3.Distance(cam.transform.position, lookPosition) + 0.3f), 1.0f));
            foreach (Vector3 vertex in grid._grid)
            {
                GL.Vertex(vertex + lookPosition);
            }

            GL.End();
            GL.PopMatrix();
            i++;
        }
    }
    
    float Round ( float x ) {
        return Mathf.Round( x / gridSize ) * gridSize;
    }
}
