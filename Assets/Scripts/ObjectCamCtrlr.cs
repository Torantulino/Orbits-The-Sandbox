using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectCamCtrlr : MonoBehaviour
{
    private PhysicsObject target;
    private float targetRad;
    private bool inTargetMode;
    private float minDist;
    private float dist;
    private float camXRot;
    private float camYRot;
    private float rotSensitivity = 1;
    private Vector3 dir;


    // Use this for initialization
    void Start()
    {
        dist = 5000.0f;
        target = GameObject.FindGameObjectWithTag("host").GetComponent<PhysicsObject>();
        targetRad = target.Radius;
        SetCamTarget(GameObject.FindGameObjectWithTag("host").GetComponent<PhysicsObject>());
        if (target == null)
            Debug.Log("System host not found.");
    }

    // Update is called once per frame
    void FixedUpdate()
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
        }
    }

    public void SetCamTarget(PhysicsObject obj)
    {
        //Obtain Scaled Distance to target
        dist *= obj.transform.localScale.x / targetRad;

        target = obj;
        targetRad = target.Radius;

        //Obtain Directional Unit Vector to Target
        dir = transform.position - target.transform.position;
        dir = dir.normalized;
    }
}