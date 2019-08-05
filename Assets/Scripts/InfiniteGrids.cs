using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InfiniteGrids : MonoBehaviour
{
  public Material GLMat;
    public int gridCount = 5; //number of grids to draw on each side of the look position (half size)
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

 
    void Start () {
        highWhite = new Color(1.0f, 1.0f, 1.0f, 0.8f * gridBrightness);
        medWhite = new Color(1.0f, 1.0f, 1.0f, 0.3f * gridBrightness);
        lowWhite = new Color(1.0f, 1.0f, 1.0f, 0.05f * gridBrightness);
        
        cam = GetComponent<Camera>();
    }
 
    void LateUpdate () {
        ray = cam.ScreenPointToRay( new Vector3( Screen.width / 2, Screen.height / 2, 0 ) );
        plane.Raycast( ray, out rayDist );
        lookPosition = ray.GetPoint( rayDist );
    }
 
    void OnPostRender () {

        GL.PushMatrix();
        GLMat.SetPass( 0 );
        GL.Begin( GL.LINES );
 
        Vector3 rounedPos = lookPosition;
 
        //Actual look position
        // GL.Color( Color.black );
        // GL.Vertex( lookPosition );
        // GL.Vertex( lookPosition + Vector3.up );
 
        GL.Color(highWhite);
 
        //Major x line
        GL.Vertex( rounedPos + new Vector3( gridCount * gridSize, 0, 0 ) );
        GL.Vertex( rounedPos + new Vector3( -gridCount * gridSize, 0, 0 ) );
        //Major z line
        GL.Vertex( rounedPos + new Vector3( 0, 0, gridCount * gridSize ) );
        GL.Vertex( rounedPos + new Vector3( 0, 0, -gridCount * gridSize ) );
 
 
        for (int i = 1; i < gridCount + 1; i++) {
            if(i%50 ==0)
                GL.Color(highWhite);
            else if(i%10 == 0)
                GL.Color(medWhite);
            else
                GL.Color(lowWhite);

            //positive x lines
            GL.Vertex( rounedPos + new Vector3( i * gridSize, 0, gridCount * gridSize ) );
            GL.Vertex( rounedPos + new Vector3( i * gridSize, 0, -gridCount * gridSize ) );
            //negative x lines
            GL.Vertex( rounedPos + new Vector3( -i * gridSize, 0, gridCount * gridSize ) );
            GL.Vertex( rounedPos + new Vector3( -i * gridSize, 0, -gridCount * gridSize ) );
            //positive z lines
            GL.Vertex( rounedPos + new Vector3( gridCount * gridSize, 0, i * gridSize ) );
            GL.Vertex( rounedPos + new Vector3( -gridCount * gridSize, 0, i * gridSize ) );
            //negative z lines
            GL.Vertex( rounedPos + new Vector3( gridCount * gridSize, 0, -i * gridSize ) );
            GL.Vertex( rounedPos + new Vector3( -gridCount * gridSize, 0, -i * gridSize ) );
        }
 
        GL.End();
        GL.PopMatrix();
    }
 
    float Round ( float x ) {
        return Mathf.Round( x / gridSize ) * gridSize;
    }
}
