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
    public List<PhysicsObject> PhysicsObjects;
    private int targetNo;
    private float targetRad;

    void OnEnable()
    {
    }

    // Use this for initialization
    void Start ()
	{
	    dist = 20000f;
	    target = GameObject.FindGameObjectWithTag("host").GetComponent<PhysicsObject>();
	    targetRad = target.transform.localScale.x;
	    SetCamTarget(target);
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
                dir = Vector3.up;
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
	        if (Input.GetKeyDown(KeyCode.LeftArrow))
	        {
	            if (targetNo > 0)
	            {
	                targetNo--;
	            }
	            else
	            {
	                targetNo = PhysicsObjects.Count - 1;
	            }
	            SetCamTarget(PhysicsObjects[targetNo]);
	        }
	        if (Input.GetKeyDown(KeyCode.RightArrow))
	        {
	            if (targetNo < PhysicsObjects.Count - 1)
	            {
	                targetNo++;
	            }
                else
	            {
	                targetNo = 0;
	            }
	            SetCamTarget(PhysicsObjects[targetNo]);
            }

        }
    }

    public void SetCamTarget(PhysicsObject obj)
    {
        //Obtain Scaled Distance to target
        dist *= obj.transform.localScale.x / targetRad;

        target = obj;
        targetRad = target.Radius;
        targetNo = PhysicsObjects.IndexOf(obj);

        if (transform.position.y < 10000)
        {
            Vector3 pos = new Vector3(transform.position.x, 500000f + 1000 * target.Radius, transform.position.z);
            pos.y = 100f * target.Radius;
            pos.x = transform.position.x;
            pos.z = transform.position.z;
            transform.position = pos;
        }

        //Obtain Directional Unit Vector to Target
        dir = transform.position - target.transform.position;
        dir = dir / dir.magnitude;
        minDist = target.transform.localScale.x + 10;
        inTargetMode = true;
    }
}
