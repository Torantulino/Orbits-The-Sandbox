using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PhysicsEngine : MonoBehaviour
{
    public float timeScale;

    // Initialize
    void Start ()
    {
        timeScale = Time.timeScale;
    }
	
	// Simulate
	void Update () {

	    Time.timeScale = timeScale;

    }

}
