using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PhysicsEngine : MonoBehaviour
{
    private List<PhysicsObject> physicsObjects;

	// Initialize
	void Start () {
		
	}
	
	// Simulate
	void FixedUpdate () {
        //Simulate every Physics Object
	    for (int i = 0; i < physicsObjects.Count; i++)
	    {
	        
	    }
	}

    void AddObject(PhysicsObject pObject)
    {
        physicsObjects.Add(pObject);
    }

}
