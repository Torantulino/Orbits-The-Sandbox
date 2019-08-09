using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PhysicsEngine : MonoBehaviour
{
    public static List<Pair> ObjectPairs;
    private float timeAtPause;
    private bool paused;
    private float G = 667.408f;

    public const float TIMESCALER = 0.01f;


    // Initialize
    void Start ()
    {
        ScaleTime(1);
    }
	
	void Update ()
	{

	}

    // Simulate
    void FixedUpdate()
    {
        foreach (Pair objectPair in ObjectPairs.ToList())
        {
            //Obtain Direction Vector
            Vector3 dir = objectPair.O1.rb.position - objectPair.O2.rb.position;
            //Obtain Distance, return if 0
            float dist = dir.magnitude;
            if (dist == 0)
                return;
            //Calculate Magnitude of force
            float magnitude = G * (objectPair.O1.rb.mass * objectPair.O2.rb.mass) / Mathf.Pow(dist, 2);
            //Calculate force
            Vector3 force = dir.normalized * magnitude;
            //Excert force on both objects, due to Newton's Third Law of motion
            objectPair.O2.rb.AddForce(force);
            objectPair.O1.rb.AddForce(force * -1); //in opositite direction
        } 
    }

    public void AddObject(PhysicsObject physicsObject)
    {
        if(ObjectPairs == null)
            ObjectPairs = new List<Pair>();

        foreach (PhysicsObject obj in PhysicsObject.physicsObjects)
        {
            //For every other object
            if (obj != physicsObject)
            {
                Pair pair = new Pair();
                pair.O1 = physicsObject;
                pair.O2 = obj;
                //Check if list already contains pair
                bool alreadyInList = false;
                foreach (Pair objectPair in ObjectPairs.ToList())
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
        foreach (Pair objectPair in ObjectPairs.ToList())
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
            Time.fixedDeltaTime = scale * 0.02f;
        }
    }
}

public class Pair
{
    public PhysicsObject O1, O2;
}
