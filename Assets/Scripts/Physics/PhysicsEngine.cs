using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PhysicsEngine : MonoBehaviour
{
    public static List<Pair> ObjectPairs;
    public float timeScale;
    private float timeAtPause;
    private bool paused;
    private float G = 667.408f;

    // Initialize
    void Start ()
    {
        timeScale = Time.timeScale;
        ObjectPairs = new List<Pair>();
    }
	
	void Update ()
	{
        if(!paused)
            Time.timeScale = timeScale;
	}

    // Simulate
    void FixedUpdate()
    {
        foreach (Pair objectPair in ObjectPairs)
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
                foreach (Pair objectPair in ObjectPairs)
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

    public void timeScaled(int scale)
    {
        if (scale >= 0 && scale <= 100)
        {
            timeScale = scale;
        }
    }
}

public class Pair
{
    public PhysicsObject O1, O2;
}
