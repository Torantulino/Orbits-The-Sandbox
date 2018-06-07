using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

public class PhysicsObject : MonoBehaviour
{
    //public float mass;
    private float density;
    private float radius;
    private float volume;
    private float speed;
    public bool massLocked;
    public bool densityLocked;
    public bool radiusLocked;
    public Rigidbody rb;

    private float G = 667.408f;

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
    private UIManager UiManager;
    private CamController mainCamController;
    private ObjectCamCtrlr previewCamCtrlr;
    private LineRenderer lineRenderer;


    public float Density
    {
        get { return density; }
    }
    public float Radius
    {
        get { return radius; }
    }
    public float Volume
    {
        get { return volume; }
    }
    public float Speed
    {
        get { return speed; }
    }

    void Awake()
    {
        //Set properties
        radius = transform.localScale.x;
        calculateVolume(radius);
        calculateDensity(rb.mass, volume);
        forceMultiplier = 10;
        densityLocked = true;
        massLocked = false;
        radiusLocked = false;
    }

    // Use this for initialization
    void Start ()
	{
        //Aquire references
        UiManager = FindObjectOfType<UIManager>();
        if(UiManager == null)
            Debug.Log("UiManager not found!");
	    mainCamController = Camera.main.GetComponent<CamController>();
        if(mainCamController == null)
            Debug.Log("Main Cam Controller fot Found.");
	    previewCamCtrlr = FindObjectOfType<ObjectCamCtrlr>();
        if(previewCamCtrlr == null)
            Debug.Log("Preview Cam Controller not found!");
	    lineRenderer = GetComponent<LineRenderer>();

        //Add to list
        mainCamController.PhysicsObjects.Add(this);

	    //Apply Random Spin around local Y axis
	    Vector3 spinVector = transform.up * Random.Range(0.1f, 2.0f);
	    rb.angularVelocity = spinVector;



        trailRenderer = GetComponentInChildren<TrailRenderer>();
        

	    if (UiManager.spawnWithOrbit)
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

    // Simulate
    void FixedUpdate ()
    {
        //Gravitate all Physics objects other than this one
        foreach (PhysicsObject physicsObject in physicsObjects)
        {
            if (physicsObject != this)
            Gravitate(physicsObject);
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

    void calculateVolume(float rad)
    {
        volume = (4 / 3f) * Mathf.PI * Mathf.Pow(rad, 3);
    }

    void calculateVolume(float m, float den)
    {
        volume = m / den;
    }

    void calculateRadius(float vol)
    {
        radius = Mathf.Pow(3 * (vol / (4 * Mathf.PI)), (1/3f));
        transform.localScale = new Vector3(radius, radius, radius);
    }

    void calculateDensity(float m, float vol)
    {
        density = m / vol;
    }

    void calculateMass(float den, float vol)
    {
        rb.mass = den * vol;
    }

    public void setRadius(float newRad)
    {
        //Set radius
        radius = newRad;
        //Calculate Volume
        calculateVolume(newRad);
        if (massLocked)
        {
            //Calculate Density
            calculateDensity(rb.mass, volume);
        }
        else if (densityLocked)
        {
            //Calculate Mass
            calculateMass(density, volume);
        }
        transform.localScale = new Vector3(radius, radius, radius);
    }

    public void setDensity(float newDen)
    {
        //Set density
        density = newDen;
        if (massLocked)
        {
            //Calculate Volume
            calculateVolume(rb.mass, newDen);
            //Update Radius
            calculateRadius(volume);
        }
        else if (radiusLocked)
        {
            //Calculate Mass
            calculateMass(newDen, volume);
        }
    }

    public void setMass(float newMass)
    {
        //Set Mass
        rb.mass = newMass;
        if (radiusLocked)
        {
            //Calculate Density
            calculateDensity(newMass, volume);
        }
        else if (densityLocked)
        {
            //Calculate Volume
            calculateVolume(newMass, density);
            //Update Radius
            calculateRadius(volume);
        }
    }

    public void setVolume(float newVol)
    {
        //Calculate Radius
        calculateRadius(newVol);
        if (massLocked)
        {
            //Calculate Density
            calculateDensity(rb.mass, newVol);
        }
        else if (densityLocked)
        {
            //Calculate Mass
            calculateMass(density, newVol);
        }
    }

    public void setSpeed(float newSpeed)
    {
        
    }
    void OnCollisionEnter(Collision collision)
    {
        PhysicsObject theirPhysObj = collision.transform.GetComponent<PhysicsObject>();
        if (theirPhysObj.rb.mass > rb.mass)
        {
            //If Smaller, Destroy
            Destroy(transform.gameObject);
        }
        else if (theirPhysObj.rb.mass < rb.mass)
        {
            //If Bigger, Absorb
            setMass(rb.mass + theirPhysObj.rb.mass);
        }
    }

    void OnMouseDown()
    {
        dragtime = 0.0f;

        //Get mouse position on screen
        dragStart = gameObject.transform.position;
        
    }

    void OnMouseDrag()
    {
        dragtime += Time.unscaledDeltaTime;

        //Differentite click from drag
        if (dragtime > 0.3f)
        {
            // Launch Object
            if (UiManager.manipMode == 0)
            {
                //Get mouse position on screen
                Vector3 screenPosition = Input.mousePosition;
                screenPosition.z = Camera.main.transform.position.y - transform.position.y;
                //Translate to world position
                dragCurrent = Camera.main.ScreenToWorldPoint(screenPosition);
                dragCurrent = dragStart - dragCurrent;
                Vector3[] positions = new Vector3[2];
                lineRenderer.SetPosition(0, gameObject.transform.position);
                lineRenderer.SetPosition(1, dragCurrent);
            }
            // Drag Object
            else if (UiManager.manipMode == 1)
            {
                if (mainCamController.target != this)
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
    }

    void OnMouseUp()
    {
        if (dragtime < 0.3f)
        {
            //Send selected object to Ui Manager
            UiManager.SetSelectedObject(this);

            //Forus target camera
            mainCamController.SetCamTarget(this);
            previewCamCtrlr.SetCamTarget(this);
        }
        else if (UiManager.manipMode == 0)
        {
            rb.AddForce(forceMultiplier * dragCurrent * rb.mass);
            lineRenderer.SetPosition(0, Vector3.zero);
            lineRenderer.SetPosition(1, Vector3.zero);
        }
    }

    void OnDestroy()
    {
        mainCamController.PhysicsObjects.Remove(this);
    }
}
