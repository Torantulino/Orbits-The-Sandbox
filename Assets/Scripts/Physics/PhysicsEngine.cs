using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using UnityEngine;

public class PhysicsEngine : MonoBehaviour
{
    // - Time -
    public const float TIMESCALER = 0.01f;
    public static float PHYSICS_TIMESTEP = 1.0f / 75.0f;
    private bool paused;
    private float timeAtPause;
    // -    -
    // - Physical Objects -
    public static List<PhysicsObjectPair> ObjectPairs;
    public HashSet<int> objectIDs = new HashSet<int>();
    // -    -
    // - Temperature -
    public static float COOLING_SPEED = 0.05f;
    public static Color HEAT_COLOR = new Color(1.498f, 0.1411f, 0.0549f) * 0.5f;
    public static float MAX_TEMP = 4.0f;
    public AnimationCurve coolingCurve;
    // -    -
    // - Effects -
    public Dictionary<string, GameObject> particleEffects = new Dictionary<string, GameObject>();
    // -    -
    // Newtonian Physics
    public static float G = 667.408f;
    public Dictionary<int, ForceExerter> strongest_force = new Dictionary<int, ForceExerter>();   //Strongest force acting on the object ID=KEY this physics update
    public struct ForceExerter
    {
        public int id;
        public float magnitude;
        public ForceExerter(int _id, float _mag)
        {
            id = _id;
            magnitude = _mag;
        }
    }
    // -    -
    
    /// Start is called on the frame when a script is enabled just before
    /// any of the Update methods is called the first time.
    void Start()
    {
        ScaleTime(1);

        // Setup cooling keys (time = current temperature, value = coolingspeed)
        {
            //ALPHA
            Keyframe[] tempKeys = new Keyframe[5];
            // Coolest
            tempKeys[0].time = 0.0f;
            tempKeys[0].value = 0.00001f;
            // Middle
            tempKeys[1].time = 0.35f;
            tempKeys[1].value = 0.0095f;
            // Hottest
            tempKeys[2].time = 2.0f;
            tempKeys[2].value = COOLING_SPEED;
            // Initial Flash!
            tempKeys[3].time = 3.0f;
            tempKeys[3].value = COOLING_SPEED * 10.0f;
            tempKeys[4].time = 4.0f;
            tempKeys[4].value = COOLING_SPEED * 300.0f;
            // Set
            AnimationCurve curve = new AnimationCurve();

            for (int i = 0; i < tempKeys.Length; i++)
                curve.AddKey(tempKeys[i]);

            //Set
            coolingCurve = curve;
        }

        // Load particle prefabs
        UnityEngine.Object[] objs = Resources.LoadAll("Prefabs/Particles");
        foreach (UnityEngine.Object obj in objs)
            particleEffects.Add(obj.name, (GameObject)obj);

        //RunBenchmarks();
    }

    // Various benchmarks for optimisation experiments during development
    private static void RunBenchmarks()
    {
        Stopwatch Auto_Method = new Stopwatch();
        Stopwatch Manual_Method = new Stopwatch();

        // Inter-Object Vector Obtain
        Vector3 position1 = new Vector3(123.321f, 456.65f, 234.0f);
        Vector3 position2 = new Vector3(33.10f, 123.4f, 454.4f);
        Vector3 dir1 = Vector3.one;
        Vector3 dir2 = Vector3.zero;

        Auto_Method.Start();
        for (int i = 0; i < 1000000; i++)
        {
            dir1 = position1 - position2;
        }
        Auto_Method.Stop();

        Manual_Method.Start();
        for (int i = 0; i < 1000000; i++)
        {
            dir2 = new Vector3(position1.x - position2.x, position1.y - position2.y, position1.z - position2.z);
        }
        Manual_Method.Stop();

        UnityEngine.Debug.Assert(dir1 == dir2);
        UnityEngine.Debug.Log("<b>Auto <color=magenta>Vector Subtraction</color> Method time: </b>" + Auto_Method.Elapsed.TotalMilliseconds);
        UnityEngine.Debug.Log("<b>Manual <color=magenta>Vector Subtraction</color> Method time: </b>" + Manual_Method.Elapsed.TotalMilliseconds);
        //-----------------------

        // Magnitude Calculation
        Auto_Method.Reset();
        Manual_Method.Reset();

        float sqrMag1 = 1;
        float sqrMag2 = 0;

        Auto_Method.Start();
        for (int i = 0; i < 1000000; i++)
        {
            sqrMag1 = dir1.sqrMagnitude;
        }
        Auto_Method.Stop();

        Manual_Method.Start();
        for (int i = 0; i < 1000000; i++)
        {
            sqrMag2 = dir1.x * dir1.x + dir1.y * dir1.y + dir1.z * dir1.z;
        }
        Manual_Method.Stop();

        UnityEngine.Debug.Assert(sqrMag1 == sqrMag2);
        UnityEngine.Debug.Log("<b>Auto <color=green>Squared Magnitude</color> Method time: </b>" + Auto_Method.Elapsed.TotalMilliseconds);
        UnityEngine.Debug.Log("<b>Manual <color=green>Squared Magnitude</color> Method time: </b>" + Manual_Method.Elapsed.TotalMilliseconds);
        //-----------------------

        // Force Calculation
        Auto_Method.Reset();
        Manual_Method.Reset();
        Vector3 force1 = Vector3.one;
        Vector3 force2 = Vector3.forward;

        Auto_Method.Start();
        for (int i = 0; i < 1000000; i++)
        {
            force1 = dir1.normalized;
        }
        Auto_Method.Stop();

        Manual_Method.Start();
        for (int i = 0; i < 1000000; i++)
        {
            float mag = Mathf.Sqrt(sqrMag1);
            force2 = new Vector3(dir1.x / mag, dir1.y / mag, dir1.z / mag);
        }
        Manual_Method.Stop();

        UnityEngine.Debug.Assert(force1 == force2);
        UnityEngine.Debug.Log("<b>Auto <color=purple>Force Calculation</color> Method time: </b>" + Auto_Method.Elapsed.TotalMilliseconds);
        UnityEngine.Debug.Log("<b>Manual <color=purple>Force Calculation</color> Method time: </b>" + Manual_Method.Elapsed.TotalMilliseconds);
        //-----------------------
    }

    /// This function is called every fixed framerate frame, if the MonoBehaviour is enabled.
    void FixedUpdate()
    {
        // Reset strongest force tracking
        strongest_force.Clear();

        // N-Body Simulation
        // For each pair of physics objects
        foreach (PhysicsObjectPair objectPair in ObjectPairs.ToList())
        {
            // Calculate force
            Vector3 force = CalculateGravitationalForce(objectPair);
            //Excert force on both objects, due to Newton's Third Law of motion
            objectPair.O2.rb.AddForce(force);
            objectPair.O1.rb.AddForce(force * -1.0f); //in opositite direction
        }
    }

    // Calculates and returns the gravitational force between two physics objects this tick
    public Vector3 CalculateGravitationalForce(PhysicsObjectPair pair)
    {
        //Obtain Direction Vector
        Vector3 pos1 = pair.O1.rb.position; //OPTIMISATION NOTE: These together cause 2x Rigidbody AND 2x Collider Syncs 
        Vector3 pos2 = pair.O2.rb.position;
        Vector3 dir = new Vector3(pos1.x - pos2.x, pos1.y - pos2.y, pos1.z - pos2.z);

        //Obtain Distance, return if 0
        float distSqr = dir.x * dir.x + dir.y * dir.y + dir.z * dir.z;
        if (distSqr == 0.0f)
            return Vector3.zero;

        //Calculate Magnitude of force
        float forceMagnitude = G * (pair.O1.rb.mass * pair.O2.rb.mass) / distSqr;

        // Compare to current largest force for both objects
        ForceExerter current_max;
        // 1
        if (!strongest_force.TryGetValue(pair.O1.ID, out current_max) || forceMagnitude > current_max.magnitude)
            strongest_force[pair.O1.ID] = new ForceExerter(pair.O2.ID, forceMagnitude);
        // 2
        if (!strongest_force.TryGetValue(pair.O2.ID, out current_max) || forceMagnitude > current_max.magnitude)
            strongest_force[pair.O2.ID] = new ForceExerter(pair.O1.ID, forceMagnitude);

        //Calculate force
        float dirMag = Mathf.Sqrt(distSqr);
        Vector3 force = new Vector3(dir.x / dirMag, dir.y / dirMag, dir.z / dirMag) * forceMagnitude;

        return force;
    }

    // Adds the given physics object to the physics engine
    // Registers and stores all possible pairs ahead of simulation
    // This is called whenever a new physics object becomes enabled and active.
    public void AddObject(PhysicsObject physicsObject)
    {
        // If pairs list does not exist, create it
        if (ObjectPairs == null)
            ObjectPairs = new List<PhysicsObjectPair>();

        foreach (KeyValuePair<int, PhysicsObject> obj in PhysicsObject.physicsObjects)
        {
            //For every other object
            if (obj.Value != physicsObject)
            {
                // Creat new pair
                PhysicsObjectPair pair = new PhysicsObjectPair();
                pair.O1 = physicsObject;
                pair.O2 = obj.Value;
                //Check if list already contains pair
                bool alreadyInList = false;
                foreach (PhysicsObjectPair objectPair in ObjectPairs.ToList())
                {
                    //If pair already exists
                    if ((objectPair.O1 == pair.O1 && objectPair.O2 == pair.O2) ||
                        (objectPair.O2 == pair.O1 && objectPair.O1 == pair.O2))
                    {
                        alreadyInList = true;
                    }
                }
                //If pair not in list, add pair
                if (!alreadyInList)
                    ObjectPairs.Add(pair);
            }
        }
    }

    // Removes the given physics object from the physics engine
    // Unregisters all pairs in which it is present
    // This is called whenever a physics object becomes disabled or inactive.
    public void RemoveObject(PhysicsObject physicsObject)
    {
        foreach (PhysicsObjectPair objectPair in ObjectPairs.ToList())
        {
            if (objectPair.O1 == physicsObject || objectPair.O2 == physicsObject)
                ObjectPairs.Remove(objectPair);
        }
    }

    // Takes note of the current simulation speed before pausing the physics simulation
    public void pauseSimulation()
    {
        timeAtPause = Time.timeScale;
        paused = true;
        Time.timeScale = 0;
    }

    // Resumes simulating physics at the previous speed
    public void resumeSimulation()
    {
        paused = false;
        Time.timeScale = timeAtPause;
    }

    // Sets the timescale to the specified value, adjusted bu const
    public void ScaleTime(float scale)
    {
        // Apply constant Timescaler
        scale *= TIMESCALER;

        if (scale >= 0.0f && scale <= 100.0f)
        {
            Time.timeScale = scale;
            Time.fixedDeltaTime = scale * PHYSICS_TIMESTEP;
        }
    }

    // Raises or lowers the timescale by the specified ammount
    public void AddjustTimeScale(float ammount)
    {
        ammount *= TIMESCALER;

        ammount = Mathf.Clamp(Time.timeScale + ammount, 0.0f, 100.0f) - Time.timeScale;

        Time.timeScale += ammount;
        Time.fixedDeltaTime = Time.timeScale * PHYSICS_TIMESTEP;
    }
}

public struct PhysicsObjectPair
{
    public PhysicsObject O1, O2;
}