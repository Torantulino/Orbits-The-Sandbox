using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using UnityEngine;

public class PhysicsEngine : MonoBehaviour
{
    public static float COOLING_SPEED = 0.05f;
    public static Color HEAT_COLOR = new Color(1.498f, 0.1411f, 0.0549f);
    public static float MAX_TEMP = 4.0f;
    public static List<PhysicsObjectPair> ObjectPairs;
    private float timeAtPause;
    private bool paused;
    public static float G = 667.408f;
    public AnimationCurve coolingCurve;
    public const float TIMESCALER = 0.01f;
    public HashSet<int> objectIDs = new HashSet<int>();
    public Dictionary<string, GameObject> particleEffects = new Dictionary<string, GameObject>();
    public Dictionary<int, ForceExerter> strongest_force = new Dictionary<int, ForceExerter>();   //Strongest force acting on the object ID=KEY this physics update
    public static float PHYSICS_TIMESTEP = 0.04f;

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
    // Initialize
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


        Stopwatch Auto_Method = new Stopwatch();
        Stopwatch Manual_Method = new Stopwatch();


        // TESTING

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
            float mag =  Mathf.Sqrt(sqrMag1);
            force2 = new Vector3(dir1.x / mag, dir1.y /mag, dir1.z / mag);
        }
        Manual_Method.Stop();

        UnityEngine.Debug.Assert(force1 == force2, "force1:" + force1 + "force2:" + force2);
        UnityEngine.Debug.Log("<b>Auto <color=purple>Force Calculation</color> Method time: </b>" + Auto_Method.Elapsed.TotalMilliseconds);
        UnityEngine.Debug.Log("<b>Manual <color=green>Force Calculation</color> Method time: </b>" + Manual_Method.Elapsed.TotalMilliseconds);
        //-----------------------

    }

    // Simulate
    void FixedUpdate()
    {
        // Reset strongest force tracking
        strongest_force.Clear();

        foreach (PhysicsObjectPair objectPair in ObjectPairs.ToList())
        {
            Vector3 force = CalculateGravitationalForce(objectPair);
            //Excert force on both objects, due to Newton's Third Law of motion

            objectPair.O2.rb.AddForce(force);
            objectPair.O1.rb.AddForce(force * -1.0f); //in opositite direction
        }
    }

    public Vector3 CalculateGravitationalForce(PhysicsObjectPair pair)
    {
        //Obtain Direction Vector
        Vector3 dir = pair.O1.rb.position - pair.O2.rb.position;    //NOTE: This causes 2x Rigidbody AND 2x Collider Syncs 

        //Obtain Distance, return if 0
        float distSqr = dir.sqrMagnitude;
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
        Vector3 force = dir.normalized * forceMagnitude;

        return force;
    }

    public void AddObject(PhysicsObject physicsObject)
    {
        if (ObjectPairs == null)
            ObjectPairs = new List<PhysicsObjectPair>();

        foreach (KeyValuePair<int, PhysicsObject> obj in PhysicsObject.physicsObjects)
        {
            //For every other object
            if (obj.Value != physicsObject)
            {
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

    public void RemoveObject(PhysicsObject physicsObject)
    {
        foreach (PhysicsObjectPair objectPair in ObjectPairs.ToList())
        {
            if (objectPair.O1 == physicsObject || objectPair.O2 == physicsObject)
                ObjectPairs.Remove(objectPair);
        }
    }

    public void pauseSimulation()
    {
        timeAtPause = Time.timeScale;
        paused = true;
        Time.timeScale = 0;
    }

    public void resumeSimulation()
    {
        paused = false;
        Time.timeScale = timeAtPause;
    }

    public void ScaleTime(float scale)
    {
        scale *= TIMESCALER;

        if (scale >= 0.0f && scale <= 100.0f)
        {
            Time.timeScale = scale;
            Time.fixedDeltaTime = scale * PHYSICS_TIMESTEP;
        }
    }

    public void AddjustTimeScale(float ammount)
    {
        ammount *= TIMESCALER;

        ammount = Mathf.Clamp(Time.timeScale + ammount, 0.0f, 100.0f) - Time.timeScale;

        Time.timeScale += ammount;
        Time.fixedDeltaTime = Time.timeScale * PHYSICS_TIMESTEP;
    }
}

public class PhysicsObjectPair
{
    public PhysicsObject O1, O2;
}
