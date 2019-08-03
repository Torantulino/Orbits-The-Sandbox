using System.Collections;
using System.Collections.Generic;
using Cinemachine;
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
    private UIManager UiManager;
    private ObjectCamCtrlr objectCamCtrlr;
    private CinemachineVirtualCamera mainVirtualCamera;

    void OnEnable()
    {
    }

    // Use this for initialization
    void Start ()
    {
        UiManager = FindObjectOfType<UIManager>();
	    dist = 20000f;
	    target = GameObject.FindGameObjectWithTag("host").GetComponent<PhysicsObject>();
	    targetRad = target.transform.localScale.x;
	    SetCamTarget(target);
        objectCamCtrlr = FindObjectOfType<ObjectCamCtrlr>();
        mainVirtualCamera = FindObjectOfType<CinemachineVirtualCamera>();
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
	            if (dist > minDist && Input.GetKey(KeyCode.W) || dist > minDist && Input.GetKey(KeyCode.UpArrow) || dist > minDist && Input.GetAxis("Mouse ScrollWheel") > 0f)
                {
                    if (Input.GetAxis("Mouse ScrollWheel") > 0f)
                    {
                        //Zoom in faster with mouse wheel
                        dist -= dist * Time.unscaledDeltaTime * 5.0f;
                        transform.position = target.transform.position + (dist * dir);
                    }
	                // Zoom
	                dist -= dist * Time.unscaledDeltaTime;
	                transform.position = target.transform.position + (dist * dir);
	            }
	            if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow) || Input.GetAxis("Mouse ScrollWheel") < 0f)
	            {
	                if (Input.GetAxis("Mouse ScrollWheel") < 0f)
	                {
	                    //Zoom out faster with mouse wheel
	                    dist += dist * Time.unscaledDeltaTime * 5.0f;
	                    transform.position = target.transform.position + (dist * dir);

                    }
                    //Zoom out
                    dist += dist * Time.unscaledDeltaTime;
	                transform.position = target.transform.position + (dist * dir);
	            }
	        }
            //Cycle Left
	        if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.A))
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
	            //Send selected object to Ui Manager
	            UiManager.SetSelectedObject(PhysicsObjects[targetNo]);
                //Send selected object to preview cam
	            objectCamCtrlr.SetCamTarget(PhysicsObjects[targetNo]);
            }
            //Cycle Right
            if (Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.D))
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
	            //Send selected object to Ui Manager
	            UiManager.SetSelectedObject(PhysicsObjects[targetNo]);
	            //Send selected object to preview cam
	            objectCamCtrlr.SetCamTarget(PhysicsObjects[targetNo]);

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
