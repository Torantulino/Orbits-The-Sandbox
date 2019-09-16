using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using Cinemachine;
using UnityEngine.UI;

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
    //private float G = 1.61803398875f;

    public Vector3 velocity = Vector3.zero;
    private Vector3 a = Vector3.zero;
    private Vector3 F = Vector3.zero;

    public static List<PhysicsObject> physicsObjects;
    public bool isTrailRenderer;

    public bool spawnWithOrbit;

    private float forceMultiplier;
    private float dragtime;
    private Vector3 dragStart;
    private Vector3 dragCurrent;
    private Vector3 dragStop;
    private TrailRenderer trailRenderer;
    private UIManager UiManager;
    private OrbitControls mainCamController;
    private CinemachineVirtualCamera previewCamCtrlr;
    private LineRenderer lineRenderer;
    private PhysicsEngine physicsEngine;
    private bool spawnee = false;
    private ContextMenu contextMenu;
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

    public int ID;

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
        physicsEngine = FindObjectOfType<PhysicsEngine>();
    }

    // Use this for initialization
    void Start()
    {
        //Aquire references
        UiManager = FindObjectOfType<UIManager>();
        if (UiManager == null)
            Debug.Log("UiManager not found by " + this.name + "!");

        mainCamController = Camera.main.GetComponent<OrbitControls>();
        if (mainCamController == null)
            Debug.Log("Main Cam Controller fot found by " + this.name + "!");

        previewCamCtrlr = FindObjectOfType<CinemachineVirtualCamera>();
        if (previewCamCtrlr == null)
            Debug.Log("Preview Cam Controller not found by " + this.name + "!");

        lineRenderer = GetComponent<LineRenderer>();
        if (lineRenderer == null)
            Debug.Log("Line renderer not found on object " + this.name + "!");

        trailRenderer = GetComponentInChildren<TrailRenderer>();
        if (trailRenderer == null)
            Debug.Log("Trail renderer not found on object " + this.name + "!");

        contextMenu = GameObject.FindObjectOfType<ContextMenu>();
        if (contextMenu == null)
            Debug.Log("Context Menu not found!");

        //Apply Random Spin around local Y axis
        Vector3 spinVector = transform.up * Random.Range(0.1f, 2.0f) / rb.mass;
        rb.angularVelocity = spinVector;

        //Clear Bugged Trail
        trailRenderer.Clear();

        // Set uniquie name
        name = name.Replace("(Clone)", "");
        if(name.StartsWith("["))
            name = name.TrimStart('[','0','1','2','3','4','5','6','7','8','9',']', ' ');
        bool idFound = false;

        // Loop until free name is found
        for (int i = 0; !idFound; i++)
        {
            if(!physicsEngine.objectIDs.Contains(i))
            {
                ID = i;
                idFound = true;
            }
        }
        // Set name
        name = "[" + ID + "] " + name;
        physicsEngine.objectIDs.Add(ID);

        // Add to entities list
        UiManager.AddToEntitiesPanel(this.gameObject);

        PhysicsObject strongestObj = null;
        if (spawnWithOrbit)
        {
            float strongestForce = 0.0f;
            strongestObj = null;
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
                requiredV = requiredV.normalized * (vMag + (UiManager.orbVMultiplier * vMag / 5));
                rb.velocity = requiredV + strongestObj.rb.velocity;

            }
        }
        if (strongestObj != null)
        {
            //Symetrical Spawning
            if (UiManager.spawnSymetry && !spawnee)
            {
                int div = UiManager.symDivs;
                float twoPi = 2 * Mathf.PI;
                //Loop starts at 1 to account for this object
                for (int i = 1; i < div; i++)
                {
                    //Calculate angle
                    float theta = (twoPi / div) * i;
                    //Spawn new object
                    GameObject spawnedObj = Instantiate(this.gameObject);
                    //- Rotate object about object of largest gravitational influence -
                    //Translate coordinate system so that OLGI is at center
                    Vector2 pos = new Vector2(rb.position.x - strongestObj.rb.position.x,
                        rb.position.z - strongestObj.rb.position.z);
                    //Perform Rotation
                    Vector2 newPos = new Vector2(pos.x * Mathf.Cos(theta) - pos.y * Mathf.Sin(theta),
                        pos.y * Mathf.Cos(theta) + pos.x * Mathf.Sin(theta));
                    //Translate coordinate system back to its original state
                    newPos = new Vector2(newPos.x + strongestObj.rb.position.x, newPos.y + strongestObj.rb.position.z);
                    //Apply position
                    spawnedObj.transform.position = new Vector3(newPos.x, strongestObj.rb.position.y, newPos.y);
                    //Set spawnee flag to prevent spawning loop
                    spawnedObj.GetComponent<PhysicsObject>().spawnee = true;
                    //Reset velocity
                    spawnedObj.GetComponent<PhysicsObject>().rb.velocity = Vector3.zero;
                }
            }
        }
    }



    void OnEnable()
    {
        if (physicsObjects == null)
            physicsObjects = new List<PhysicsObject>();

        physicsObjects.Add(this);

        physicsEngine.AddObject(this);

    }

    void OnDisable()
    {
        physicsEngine.RemoveObject(this);
        physicsObjects.Remove(this);
    }


    void Update()
    {
        //gameObject.transform.localScale = new Vector3(radius, radius, radius);

        //Set trail renderer thickness to scale with camera distance
        if (trailRenderer != null)
        {
            trailRenderer.widthMultiplier = mainCamController._Distance / 500.0f;
        }
        else
        {
            Debug.Log(this.name + " has no trail renderer!");
        }
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
        radius = Mathf.Pow(3 * (vol / (4 * Mathf.PI)), (1 / 3f));
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
            // // Launch Object
            // if (UiManager.manipMode == 0)
            // {
            //     //Get mouse position on screen
            //     Vector3 screenPosition = Input.mousePosition;
            //     screenPosition.z = Camera.main.transform.position.y - transform.position.y;
            //     //Translate to world position
            //     dragCurrent = Camera.main.ScreenToWorldPoint(screenPosition);
            //     dragCurrent = dragStart - dragCurrent;
            //     Vector3[] positions = new Vector3[2];
            //     lineRenderer.SetPosition(0, gameObject.transform.position);
            //     lineRenderer.SetPosition(1, dragCurrent);
            // }
            // // Drag Object
            // else if (UiManager.manipMode == 1)
            // {
            //     if (mainCamController.FocalObject != this)
            //     {
            //         //Get mouse position on screen
            //         Vector3 screenPosition = Input.mousePosition;
            //         screenPosition.z = Camera.main.transform.position.y - transform.position.y;
            //         //Translate to world position
            //         Vector3 worldPosition = Camera.main.ScreenToWorldPoint(screenPosition);
            //         //Move object
            //         transform.position = worldPosition;
            //     }
            // }
        }
    }

    void OnMouseUp()
    {

        if (dragtime < 0.3f)
        {
            //Send selected object to Ui Manager
            UiManager.SetSelectedObject(this);
        }
        // else if (UiManager.manipMode == 0)
        // {
        //     rb.AddForce(forceMultiplier * dragCurrent * rb.mass);
        //     lineRenderer.SetPosition(0, Vector3.zero);
        //     lineRenderer.SetPosition(1, Vector3.zero);
        // }
    }


    // Called every frame while the mouse is over the GUIElement or Collider.
    void OnMouseOver()
    {
        //Right click
        if (Input.GetMouseButtonDown(1))
        {
            contextMenu.SetTarget(this.gameObject);
        }
    }
    void OnDestroy()
    {
        UiManager.RemoveFromEntitiesPanel(this.gameObject);
        physicsEngine.objectIDs.Remove(ID);
    }
}
