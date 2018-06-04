using System.Collections.Generic;
using UnityEngine;

public class PhysicsObject : MonoBehaviour
{ 
    public Vector3 position;
    //public float mass;
    public float density;
    public float radius;
    public bool massLocked;
    public bool densityLocked;
    public bool radiusLocked;
    public float volume;
    private UIManager UiManager;
    public bool massChanged;
    public bool densityChanged;
    public bool radiusChanged;
    public Rigidbody rb;
    public bool isPathPlotter;

    private bool volumeChanged;
    private float oldRadius;
    private float oldMass;
    private float oldDensity;
    private float G = 667.0f;

    public Vector3 velocity = Vector3.zero;
    private Vector3 a = Vector3.zero;
    private Vector3 F = Vector3.zero;

    public static List<PhysicsObject> physicsObjects;
    public bool isTrailRenderer;

    private float forceMultiplier;
    private float dragtime;
    private Vector3 dragStart;
    private Vector3 dragCurrent;
    private Vector3 dragStop;
    private TrailRenderer trailRenderer;

    public int manipMode; //0 = Launch Mode, 1 = Move mode

    public bool spawnWithOrbit = true;


	// Use this for initialization
	void Start ()
	{
        //Apply Random Spin
	    rb.angularVelocity = new Vector3(0.0f, Random.Range(0.1f, 2.0f), 0.0f);
	    UiManager = FindObjectOfType<UIManager>();
	    forceMultiplier = 10;
        manipMode = 0;
	    radius = gameObject.transform.localScale.x;
	    density = 0.0001f;
	    densityLocked = true;
	    massLocked = false;
	    radiusLocked = false;
	    trailRenderer = GetComponentInChildren<TrailRenderer>();
        

	    if (spawnWithOrbit)
	    {
	        float strongestForce = 0.0f;
	        PhysicsObject strongestObj = null;
            // Sort PhysicsObjects by Mass
            physicsObjects.Sort((y, x) => x.rb.mass.CompareTo(y.rb.mass));
	        //Find Object with highest gravitational influence
	        foreach (PhysicsObject obj in physicsObjects)
	        {
	            //Obtain Direction Vector
	            Vector3 dir = rb.position - obj.rb.position;
	            //Obtain Distance, return if 0
	            float dist = dir.magnitude;
	            if (dist != 0)
	            {
	                //Calculate Magnitude of force
	                float magnitude = G * (rb.mass * obj.rb.mass) / Mathf.Pow(dist, 2);
	                //Calculate force
	                Vector3 force = dir.normalized * magnitude;
	                if (force.magnitude >= strongestForce)
	                {
	                    strongestObj = obj;
	                    strongestForce = force.magnitude;
	                }
	            }
	        }
	        //Attempt to achive stable orbit
	        if (strongestObj != null)
	        {
	            //Obtain Oblique vector along y plane
	            Vector3 dir = rb.position - strongestObj.rb.position;
	            float dist = dir.magnitude;
	            Vector3 requiredV = new Vector3(dir.z, dir.y, -dir.x);
	            float vMag = Mathf.Sqrt(G * strongestObj.rb.mass / dist);
	            requiredV = requiredV.normalized * vMag;
	            rb.velocity = requiredV + strongestObj.rb.velocity;

	        }
	    }
    }



    void OnEnable ()
    {
     if (physicsObjects == null)
            physicsObjects = new List<PhysicsObject>();

     physicsObjects.Add(this);

       
    }

    void OnDisable ()
    {
        physicsObjects.Remove(this);
    }


    void Update()
    {
        //gameObject.transform.localScale = new Vector3(radius, radius, radius);
        if (massChanged || densityChanged || radiusChanged || volumeChanged)
        {
            UpdateProperties();
        }

        if (isTrailRenderer)
        {
            //Set trail renderer thickness to scale with camera distance
            if (trailRenderer != null)
            {
                trailRenderer.widthMultiplier =
                    Vector3.Distance(Camera.main.transform.position, transform.position) / 250;
            }
            else
            {
                Debug.Log(this.name + " has no trail renderer!");
            }
        }
    }

    // Simulate
    void FixedUpdate ()
    {
        if (isPathPlotter)
        {
        }
        else
        {
            //Gravitate all Physics objects other than this one
            foreach (PhysicsObject physicsObject in physicsObjects)
            {
                if (physicsObject != this)
                    Gravitate(physicsObject);
            }
        }
        
        /*
        //Calculate Acceleration from Force and Mass
        a = F / rb.mass;
        //Calculate Velocity from acceleration and time
        velocity += a * Time.fixedDeltaTime;
        //Calculate New position from velocity and time
	    position += velocity * Time.fixedDeltaTime;
        //Move Object
	    gameObject.transform.position = position;
        */
	}

    void Gravitate(PhysicsObject subjectObj)
    {
        //Obtain Direction Vector
        Vector3 dir = rb.position - subjectObj.rb.position;
        //Obtain Distance, return if 0
        float dist = dir.magnitude;
        if(dist == 0)
            return;
        //Calculate Magnitude of force
        float magnitude = G * (rb.mass * subjectObj.rb.mass) / Mathf.Pow(dist, 2);
        //Calculate force
        Vector3 force = dir.normalized * magnitude;
        //Excert force on subject object
        subjectObj.rb.AddForce(force);
     }

    void ExcertForce(Vector3 force)
    {
        F += force;
    }

    void UpdateProperties()
    {
        if (massChanged)
        {
            if (massLocked)
            {
            }
            else if (!radiusLocked)
            {
                //Update Volume
                volume = rb.mass / density;
                //Update Radius
                radius = Mathf.Pow((3 * (volume / (4 * Mathf.PI))), 1/3f);
            }
            else if (!densityLocked)
            {
                density = rb.mass / volume;
            }
            massChanged = false;
        }
        if (densityChanged)
        {
            if (densityLocked)
                density = oldDensity;
            else if (!massLocked)
                rb.mass = density * volume;
            else if (!radiusLocked)
            {
                //Update Volume
                volume = rb.mass / density;
                //Update Radius
                radius = Mathf.Pow((3 * (volume / 4 * Mathf.PI)), 1/3f);
            }
            densityChanged = false;
        }
        if (radiusChanged)
        {
            if (radiusLocked)
            {
                radius = oldRadius;
            }
            else if (!massLocked)
            {
                //Update Volume
                volume = (4/3f) * Mathf.PI * Mathf.Pow(radius, 3f);
                //Update Mass
                rb.mass = density * volume;
            }
            else if (!densityLocked)
            {
                //Update Volume
                volume = (4/3f) * Mathf.PI * Mathf.Pow(radius, 3f);
                //Update Density
                density = rb.mass / volume;
            }
            radiusChanged = false;
        }
    }

    void OnMouseDown()
    {
        Debug.Log("Click");
        UiManager.SetSelectedObject(this);

        dragtime = 0.0f;

        //Get mouse position on screen
        dragStart = gameObject.transform.position;
        
    }

    void OnMouseDrag()
    {
        dragtime += Time.deltaTime;

        //Differentite click from drag
        if (dragtime > 0.3f)
        {
            // Launch Object
            if (manipMode == 0)
            {
                //Get mouse position on screen
                Vector3 screenPosition = Input.mousePosition;
                screenPosition.z = Camera.main.transform.position.y - transform.position.y;
                //Translate to world position
                dragCurrent = Camera.main.ScreenToWorldPoint(screenPosition);
                dragCurrent = dragStart - dragCurrent;
                Debug.DrawRay(gameObject.transform.position, dragCurrent, Color.white);
                Debug.Log("Drag:" + dragCurrent);
            }
            // Drag Object
            else if (manipMode == 1)
            {
                //Get mouse position on screen
                Vector3 screenPosition = Input.mousePosition;
                screenPosition.z = Camera.main.transform.position.y - transform.position.y;
                //Translate to world position
                Vector3 worldPosition = Camera.main.ScreenToWorldPoint(screenPosition);
                //Move object
                transform.position = worldPosition;
            }
        }
    }

    void OnMouseUp()
    {
        if (manipMode == 0)
        {
            rb.AddForce(forceMultiplier * dragCurrent * rb.mass);
        }
    }

}
