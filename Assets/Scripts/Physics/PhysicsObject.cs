using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using Cinemachine;
using UnityEngine.UI;
using System.Linq;
using System;
using UnityEngine.EventSystems;

public class PhysicsObject : MonoBehaviour
{
    public bool Shattered = false;

    //public float mass;
    private float density;
    private float radius;
    private float volume;
    private float speed;
    public bool massLocked;
    public bool densityLocked;
    public bool radiusLocked;
    public Rigidbody rb;
    private bool isShard = false;

    public PhysicsObjectDefaults defaultSettings;

    public Vector3 velocity = Vector3.zero;
    private Vector3 a = Vector3.zero;
    private Vector3 F = Vector3.zero;

    public static Dictionary<int, PhysicsObject> physicsObjects;    //Key = ID
    public bool isTrailRenderer;

    public bool spawnWithOrbit;

    public TrailRenderer trailRenderer;

    private float forceMultiplier;
    private float dragtime;
    private Vector3 dragStart;
    private Vector3 dragCurrent;
    private Vector3 dragStop;

    private UIManager UiManager;
    private OrbitControls mainCamController;
    private CinemachineVirtualCamera previewCamCtrlr;
    private LineRenderer lineRenderer;
    private PhysicsEngine physicsEngine;
    private bool spawnee = false;
    private PhysicsObject biggestGravitationalInfluencer;
    private Dictionary<string, UnityEngine.Object> CelestialObjects = new Dictionary<string, UnityEngine.Object>(); //Prefabs
    public float temperature = 0.0f;
    public bool isStar = false;
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

    public bool _drawFuturePath = false;
    public bool _drawPastPath = false;
    private float timeSinceLastPosition = 0.0f;

    private FixedSizedQueue<Vector3> relativeTrailPositions = new FixedSizedQueue<Vector3>(150);



    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        if (rb == null)
            Debug.Log("Rigidbody not found by " + this.name + "!");

        //Set properties
        radius = transform.localScale.x;
        calculateVolume(radius);
        calculateDensity(rb.mass, volume);
        forceMultiplier = 10;
        densityLocked = true;
        massLocked = false;
        radiusLocked = false;
        physicsEngine = FindObjectOfType<PhysicsEngine>();

        // Set uniquie name
        name = name.Replace("(Clone)", "");
        if (name.StartsWith("["))
            name = name.TrimStart('[', '0', '1', '2', '3', '4', '5', '6', '7', '8', '9', ']', ' ');
        bool idFound = false;

        // Loop until free name is found
        for (int i = 0; !idFound; i++)
        {
            if (!physicsEngine.objectIDs.Contains(i))
            {
                ID = i;
                idFound = true;
            }
        }
        // Set name
        name = "[" + ID + "] " + name;
        physicsEngine.objectIDs.Add(ID);

    }


    /// This function is called when the object becomes enabled and active.
    void OnEnable()
    {
        if (physicsObjects == null)
            physicsObjects = new Dictionary<int, PhysicsObject>();

        physicsObjects.Add(ID, this);

        physicsEngine.AddObject(this);
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

        //Load Celestial Objects
        UnityEngine.Object[] CelestialObj = Resources.LoadAll("Prefabs/Objects");
        foreach (UnityEngine.Object obj in CelestialObj)
        {
            CelestialObjects.Add(obj.name, obj);
        }

        //Apply Random Spin around local Y axis
        Vector3 spinVector = transform.up * UnityEngine.Random.Range(0.1f, 2.0f) / rb.mass;
        rb.angularVelocity = spinVector;

        //Clear Bugged Trail
        trailRenderer.Clear();

        // Setup linerenderer
        lineRenderer.positionCount = 0;
        lineRenderer.widthMultiplier = 1.0f;
        lineRenderer.startWidth = 1.0f;
        lineRenderer.endWidth = 1.0f;

        // Add to entities list
        UiManager.AddToEntitiesPanel(this.gameObject);

        // Spawn in orbit around strongest influencer
        biggestGravitationalInfluencer = null;
        if (spawnWithOrbit)
        {
            //Find object with highest gravitational influence (As physics engine won't have calculated this yet)
            biggestGravitationalInfluencer = GetBiggestGravitationalInfluencer();

            //Attempt to achive stable orbit
            if (biggestGravitationalInfluencer != null)
            {
                //Obtain Oblique vector along y plane
                Vector3 dir = rb.position - biggestGravitationalInfluencer.rb.position;
                float dist = dir.magnitude;
                Vector3 requiredV = new Vector3(dir.z, dir.y, -dir.x);
                float vMag = Mathf.Sqrt(PhysicsEngine.G * biggestGravitationalInfluencer.rb.mass / dist);
                requiredV = requiredV.normalized * (vMag + (UiManager.orbVMultiplier * vMag / 5));
                rb.velocity = requiredV + biggestGravitationalInfluencer.rb.velocity;
            }
        }
        if (biggestGravitationalInfluencer != null)
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
                    Vector2 pos = new Vector2(rb.position.x - biggestGravitationalInfluencer.rb.position.x,
                        rb.position.z - biggestGravitationalInfluencer.rb.position.z);
                    //Perform Rotation
                    Vector2 newPos = new Vector2(pos.x * Mathf.Cos(theta) - pos.y * Mathf.Sin(theta),
                        pos.y * Mathf.Cos(theta) + pos.x * Mathf.Sin(theta));
                    //Translate coordinate system back to its original state
                    newPos = new Vector2(newPos.x + biggestGravitationalInfluencer.rb.position.x, newPos.y + biggestGravitationalInfluencer.rb.position.z);
                    //Apply position
                    spawnedObj.transform.position = new Vector3(newPos.x, biggestGravitationalInfluencer.rb.position.y, newPos.y);
                    //Set spawnee flag to prevent spawning loop
                    spawnedObj.GetComponent<PhysicsObject>().spawnee = true;
                    //Reset velocity
                    spawnedObj.GetComponent<PhysicsObject>().rb.velocity = Vector3.zero;
                }
            }
        }

        defaultSettings.mass = rb.mass;
        defaultSettings.scale = gameObject.transform.localScale;
        defaultSettings.temperature = temperature;
        defaultSettings.velocity = rb.velocity;

    }

    //Finds and returns object with highest gravitational influence or null
    PhysicsObject GetBiggestGravitationalInfluencer()
    {
        PhysicsObject _strongestObj = null;
        float strongestForce = 0.0f;

        //Find Object with highest gravitational influence
        foreach (KeyValuePair<int, PhysicsObject> pair in physicsObjects)
        {
            //Obtain Direction Vector
            Vector3 dir = rb.position - pair.Value.rb.position;
            //Obtain Distance, return if 0
            float distSqr = dir.sqrMagnitude;
            if (distSqr != 0)
            {
                //Calculate Magnitude of force
                float magnitude = PhysicsEngine.G * (rb.mass * pair.Value.rb.mass) / distSqr;
                //Calculate force
                Vector3 force = dir.normalized * magnitude;
                if (force.magnitude >= strongestForce)
                {
                    _strongestObj = pair.Value;
                    strongestForce = force.magnitude;
                }
            }
        }
        return _strongestObj;
    }

    /// This function is called when the behaviour becomes disabled or inactive.
    void OnDisable()
    {
        physicsEngine.RemoveObject(this);
        physicsObjects.Remove(ID);
    }

    void Update()
    {
        //gameObject.transform.localScale = new Vector3(radius, radius, radius);

        //Set trail renderer thickness to scale with camera distance
        if (trailRenderer != null)
        {
            float widthMultiplier = mainCamController._TargetDistance / 500.0f;
            trailRenderer.widthMultiplier = widthMultiplier;
            lineRenderer.widthMultiplier = widthMultiplier;
        }
        else
        {
            Debug.Log(this.name + " has no trail renderer!");
        }

        // Process Heat
        if (!isStar && temperature != 0.0f)
        {
            temperature -= UnityEngine.Random.Range(0.9f, 1.1f) * (Time.deltaTime * physicsEngine.coolingCurve.Evaluate(temperature));

            // Change colour based on heat
            Material material = GetComponentInChildren<MeshRenderer>().material;
            material.EnableKeyword("_EMISSION");
            material.SetColor("_EmissionColor", PhysicsEngine.HEAT_COLOR * temperature);

            // Cooled
            if (temperature <= 0.0f)
            {
                material.DisableKeyword("_EMISSION");
                temperature = 0.0f;
            }
        }
    }

    /// LateUpdate is called every frame, if the Behaviour is enabled.
    /// It is called after all Update functions have been called.
    void LateUpdate()
    {

        // Check incase object has been destroyed or force is yet to be calculated
        PhysicsObject newInfluencer;
        PhysicsEngine.ForceExerter forceExerter;
        if (physicsEngine.strongest_force.TryGetValue(ID, out forceExerter) && physicsObjects.ContainsKey(forceExerter.id))
        {
            newInfluencer = physicsObjects[forceExerter.id];
            // If new influencer
            if (newInfluencer != biggestGravitationalInfluencer)
            {
                if (_drawPastPath)
                    // Clear previous trail if in new orbit
                    relativeTrailPositions.Clear();
            }

            // Update
            biggestGravitationalInfluencer = newInfluencer;
        }

        // Future relative predicted path
        if (UiManager.displayFuturePath)
        {
            // First frame setup
            if (!_drawFuturePath)
            {
                // Setup colour keys
                GradientColorKey[] colorKeys = new GradientColorKey[2];
                colorKeys[0].color = Color.blue;
                colorKeys[0].time = 0.0f;
                colorKeys[1].color = Color.blue;
                colorKeys[1].time = 0.0f;

                // Setup alpha keys
                GradientAlphaKey[] alphaKeys = new GradientAlphaKey[3];
                alphaKeys[0].alpha = 1.0f;
                alphaKeys[0].time = 0.0f;
                alphaKeys[1].alpha = 1.0f;
                alphaKeys[1].time = 0.5f;
                alphaKeys[2].alpha = 0.0f;
                alphaKeys[2].time = 1.0f;

                // Set colour gradient
                Gradient gradient = new Gradient();
                gradient.SetKeys(colorKeys, alphaKeys);
                lineRenderer.colorGradient = gradient;

                // Set flag
                _drawFuturePath = true;
            }

            lineRenderer.useWorldSpace = true;
            uint segments = 500;

            // Predict future path
            Vector3[] positions = PredictOrbit(biggestGravitationalInfluencer, segments);

            if (positions != null)
            {
                // Set positions
                lineRenderer.positionCount = (int)segments;
                lineRenderer.SetPositions(positions);
            }
        }
        // Past Drawn relative path
        else if (UiManager.displayPastPath)
        {
            // First frame setup
            if (!_drawPastPath)
            {
                // Setup colour keys
                GradientColorKey[] colorKeys = new GradientColorKey[2];
                colorKeys[0].color = Color.cyan;
                colorKeys[0].time = 0.0f;
                colorKeys[1].color = Color.cyan;
                colorKeys[1].time = 0.0f;

                // Setup alpha keys
                GradientAlphaKey[] alphaKeys = new GradientAlphaKey[3];
                alphaKeys[0].alpha = 0.0f;
                alphaKeys[0].time = 0.0f;
                alphaKeys[1].alpha = 1.0f;
                alphaKeys[1].time = 0.5f;
                alphaKeys[2].alpha = 1.0f;
                alphaKeys[2].time = 1.0f;

                // Set colour gradient
                Gradient gradient = new Gradient();
                gradient.SetKeys(colorKeys, alphaKeys);
                lineRenderer.colorGradient = gradient;

                // Set flag
                _drawPastPath = true;
            }

            // Update trail vertex count
            lineRenderer.positionCount = relativeTrailPositions.Count;

            // Manually translate all positions to acheive local space without rotation or scale
            lineRenderer.useWorldSpace = true;
            // for each position
            for (int i = 0; i < lineRenderer.positionCount; i++)
            {
                Vector3 relativePos = relativeTrailPositions.ElementAt(i);
                lineRenderer.SetPosition(i, relativePos + biggestGravitationalInfluencer.transform.position);
            }

            // Track position
            if (timeSinceLastPosition > 1.0f && Time.timeScale != 0.0f)
            {
                // Add to relative positions queue
                relativeTrailPositions.Enqueue(transform.position -
                    biggestGravitationalInfluencer.transform.position);

                // Reset timer
                timeSinceLastPosition = 0.0f;
            }
            // Increment timer
            else
            {
                timeSinceLastPosition += Time.unscaledDeltaTime * 100.0f;
            }
        }

        // If flag is true but realtime value false:
        // Turn off future path
        if (_drawFuturePath && !UiManager.displayFuturePath)
        {
            // Reset flag
            _drawFuturePath = false;
            // Clear path
            lineRenderer.positionCount = 0;
        }

        // If flag is true but realtime value false:
        // Turn off past path
        if (_drawPastPath && !UiManager.displayPastPath)
        {
            // Reset flag
            _drawPastPath = false;
            // Clear path
            lineRenderer.positionCount = 0;
            relativeTrailPositions.Clear();
        }
    }

    Vector3[] PredictOrbit(PhysicsObject _strongestObject, uint steps)
    {
        try
        {
            Vector3[] positions = new Vector3[steps];

            PhysicsObjectPair pair = new PhysicsObjectPair();
            pair.O1 = this;
            pair.O2 = _strongestObject;


            // Calculate Orbital Period for this
            //float period = (2.0f * Mathf.PI) * Mathf.Sqrt( Mathf.Pow(Vector3.Distance(pair.O1.transform.position, pair.O2.transform.position),3.0f) / G * ( pair.O1.rb.mass + pair.O2.rb.mass)  );



            Vector3 position1 = this.transform.position;
            Vector3 position2 = _strongestObject.transform.position;

            Vector3 velocity1 = this.rb.velocity - _strongestObject.rb.velocity;
            //Vector3 velocity2 = _strongestObject.rb.velocity;
            //Vector3 velocity2 = -velocity1;
            Vector3 velocity2 = Vector3.zero;
            //Vector3 velocity2 = _strongestObject.rb.velocity - this.rb.velocity;


            float mass1 = this.rb.mass;
            float mass2 = _strongestObject.rb.mass;

            // Note, this is flawed as it assumes circular orbit
            float period = (Mathf.PI * 2.0f) * Vector3.Distance(pair.O1.transform.position, pair.O2.transform.position) /
                velocity1.magnitude;

            float timestep = period / steps;

            for (int i = 0; i < steps; i++)
            {
                positions[i] = position1;

                // Calculate force
                Vector3 force = new Vector3();
                //Obtain Direction Vector
                Vector3 dir = position1 - position2;
                //Obtain Distance, return if 0
                float dist = dir.magnitude;
                if (dist == 0)
                    force = Vector3.zero;
                //Calculate Magnitude of force
                float magnitude = PhysicsEngine.G * (mass1 * mass2) / Mathf.Pow(dist, 2);
                force = dir.normalized * magnitude;

                // Calculate accelerations
                Vector3 a1 = -force / mass1;
                Vector3 a2 = force / mass2;

                // Update positions
                position1 += velocity1 * timestep + 0.5f * a1 * timestep * timestep;
                position2 += velocity2 * timestep + 0.5f * a2 * timestep * timestep;

                // Update velocities
                velocity1 += a1 * timestep;
                velocity2 += a2 * timestep;

            }

            return positions;
        }
        catch
        {
            lineRenderer.positionCount = 0;
            return default;
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

    private void CollisionResolution(Collision collision)
    {

        PhysicsObject theirPhysObj = collision.gameObject.GetComponent<PhysicsObject>();

        // Check for larger object
        if (theirPhysObj.rb.mass < rb.mass)
        {
            //If Bigger, Absorb
            Absorb(theirPhysObj);
        }
        // If Equal...
        else if (theirPhysObj.rb.mass == rb.mass)
        {
            // Shatter both
            if (!isShard)
                Shatter(5.0f);
            if (!theirPhysObj.isShard)
                theirPhysObj.Shatter(5.0f);

            // If both shards then just absorb
            if (theirPhysObj.isShard && isShard && theirPhysObj.ID > ID)
                Absorb(theirPhysObj);
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        CollisionResolution(collision);
    }

    private void Absorb(PhysicsObject smallerObject)
    {
        float their_mass = smallerObject.rb.mass;

        temperature = Mathf.Min(temperature + their_mass / rb.mass, PhysicsEngine.MAX_TEMP);

        // Only absorb mass if won't fragment
        if (smallerObject.isShard)
            setMass(rb.mass + their_mass);

        smallerObject.BeAbsorbed(this.gameObject);
    }

    public void BeAbsorbed(GameObject _absorber)
    {
        // If camera is following this object, refocus
        if (mainCamController.FocalObject == this.gameObject.transform)
            mainCamController.SetFocalObject(_absorber);

        // Particle Effect
        GameObject emitter = Instantiate(physicsEngine.particleEffects["ShatterEffect"], transform.position, transform.rotation, _absorber.transform);
        emitter.transform.localScale = transform.localScale;
        Destroy(emitter, 0.5f);

        // Shatter
        if (!isShard)
            Shatter(10.0f);
        else
            PoolManager.PoolDictionary["Shard1"].ReturnObjectToPool(this);
    }

    // Shatter this object into the specified number of hot shards
    // This game object is destroyed in the process
    private void Shatter(float _shards, float _intensity = 1.0f)
    {
        if (!Shattered)
        {
            Shattered = true;
            for (int i = 0; i < _shards; i++)
            {
                // Calculate offset
                Vector3 offset = new Vector3(UnityEngine.Random.Range(-10f, 10f), UnityEngine.Random.Range(-10f, 10f), UnityEngine.Random.Range(-10f, 10f));

                // Instantiate
                Vector3 pos = transform.position + UnityEngine.Random.insideUnitSphere * GetComponentInChildren<Collider>().bounds.extents.x;
                //PhysicsObject shard = Instantiate((GameObject)CelestialObjects["Shard1"], pos, Quaternion.Euler(offset * 32.0f)).GetComponent<PhysicsObject>();

                PhysicsObject shard = PoolManager.PoolDictionary["Shard1"].SpawnFromPool(pos, Quaternion.Euler(offset * 32.0f)).GetComponent<PhysicsObject>();


                // Set physical properties
                //shard.transform.localScale = Vector3.one * rb.mass / no_shards;
                shard.isShard = true;
                shard.density = 2.0f;
                shard.setMass(rb.mass / _shards);
                shard.rb.velocity = rb.velocity + offset.normalized * 0.8f * Mathf.Sqrt((2.0f * PhysicsEngine.G * rb.mass) / (pos - transform.position).magnitude);
                shard.rb.angularVelocity = offset * 100.0f;
                shard.temperature = PhysicsEngine.MAX_TEMP;
            }

            Destroy(this.gameObject);
        }
    }

    void OnMouseDown()
    {
        if (EventSystem.current.IsPointerOverGameObject())
            return;

        dragtime = 0.0f;

        //Get mouse position on screen
        dragStart = gameObject.transform.position;

    }

    void OnMouseDrag()
    {
        if (EventSystem.current.IsPointerOverGameObject())
            return;

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
        if (EventSystem.current.IsPointerOverGameObject())
            return;

        if (dragtime < 0.3f)
        {
            if (UiManager.laser)
            {
                Shatter(10.0f, 2.0f);
            }
            else
            {
                //Send selected object to Ui Manager
                UiManager.SetSelectedObject(this);
            }
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
        if (EventSystem.current.IsPointerOverGameObject())
            return;

        //Right click
        if (Input.GetMouseButtonDown(1))
        {
            UiManager.contextMenu.SetTarget(this.gameObject);
        }
    }
    void OnDestroy()
    {
        if (UiManager != null)
        {
            UiManager.RemoveFromEntitiesPanel(this.gameObject);
            physicsEngine.objectIDs.Remove(ID);
        }
    }
}
