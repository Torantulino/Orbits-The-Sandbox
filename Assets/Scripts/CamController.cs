using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions.Comparers;

public class CamController : MonoBehaviour
{

    public PhysicsObject target;
    private bool inTargetMode;
    private float minDist;
    private float dist;
    private float camXRot;
    private float camYRot;
    private float rotSensitivity = 1;
    private Vector3 dir;

	// Use this for initialization
	void Start ()
	{
	    inTargetMode = false;
	}
	
	// Update is called once per frame
	void FixedUpdate () {
	    if (inTargetMode)
	    {
	        if (target != null)
	        {
	            //Look at Target
	            transform.LookAt(target.transform);

	            //Keep pace with target
	            transform.position = target.transform.position + (dist * dir);

	            //Update Directional Unit Vector to Target
	            dir = transform.position - target.transform.position;
	            dir = dir.normalized;

	            //Update distance from target
	            dist = Vector3.Distance(transform.position, target.transform.position);


	            //If not too close to surface of planet (to prevent clipping) zoom!
	            if (dist > minDist && Input.GetKey(KeyCode.W))
	            {
	                // Zoom
	                dist -= dist * Time.deltaTime;
	                transform.position = target.transform.position + (dist * dir);
	            }
	            if (Input.GetKey(KeyCode.S))
	            {
	                //Zoom out
	                dist += dist * Time.deltaTime;
	                transform.position = target.transform.position + (dist * dir);
	            }
	        }
	    }
	}

    public void SetCamTarget(PhysicsObject obj)
    {
        target = obj;
        //Obtain Directional Unit Vector to Target
        dir = transform.position - target.transform.position;
        dir = dir.normalized;
        //Obtain Distance to target
        dist = Vector3.Distance(transform.position, target.transform.position);
        minDist = target.transform.localScale.x + 10;
        inTargetMode = true;
    }
}
