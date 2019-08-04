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
    private float rotSensitivity = 1;
    private Vector3 dir;
    public List<PhysicsObject> PhysicsObjects;
    private int targetNo;
    private float targetRad;
    private UIManager UiManager;
    private ObjectCamCtrlr objectCamCtrlr;
    public CinemachineVirtualCamera mainVirtualCamera;

    void OnEnable()
    {
    }

    // Use this for initialization
    void Start()
    {
        UiManager = FindObjectOfType<UIManager>();
        dist = 20f;
        dir = Vector3.up;
        target = GameObject.FindGameObjectWithTag("host").GetComponent<PhysicsObject>();
        targetRad = target.transform.localScale.x;
        SetCamTarget(target);
        objectCamCtrlr = FindObjectOfType<ObjectCamCtrlr>();

    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (inTargetMode)
        {
            if (target == null)
            {
                target = FindObjectOfType<PhysicsObject>();
                if (target == null)
                    return;
            }
            
            // Look at Target
            mainVirtualCamera.m_LookAt = target.transform;

            // Update camera position
            mainVirtualCamera.transform.position = target.transform.position + (dist * dir);

            //Update Directional Unit Vector to Target - in order to account for movement
            dir = transform.position - target.transform.position;
            dir = dir.normalized;

            //Update distance from target - in order to account for movement
            //dist = Vector3.Distance(transform.position, target.transform.position);

            //If not too close to surface of planet (to prevent clipping) zoom!
            // if (dist > minDist && Input.GetKey(KeyCode.W) || dist > minDist && Input.GetKey(KeyCode.UpArrow) || dist > minDist && Input.GetAxis("Mouse ScrollWheel") > 0f)
            // {
            //     if (Input.GetAxis("Mouse ScrollWheel") > 0f)
            //     {
            //         //Zoom in faster with mouse wheel
            //         dist -= dist * Time.unscaledDeltaTime * 5.0f;
            //         mainVirtualCamera.GetCinemachineComponent<CinemachineTransposer>().m_FollowOffset = target.transform.position + (dist * dir);
            //     }
            //     // Zoom
            //     dist -= dist * Time.unscaledDeltaTime;
            //     mainVirtualCamera.GetCinemachineComponent<CinemachineTransposer>().m_FollowOffset = target.transform.position + (dist * dir);
            // }
            // if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow) || Input.GetAxis("Mouse ScrollWheel") < 0f)
            // {
            //     if (Input.GetAxis("Mouse ScrollWheel") < 0f)
            //     {
            //         //Zoom out faster with mouse wheel
            //         dist += dist * Time.unscaledDeltaTime * 5.0f;
            //         mainVirtualCamera.GetCinemachineComponent<CinemachineTransposer>().m_FollowOffset = target.transform.position + (dist * dir);

            //     }
            //     //Zoom out
            //     dist += dist * Time.unscaledDeltaTime;
            //     transform.position = target.transform.position + (dist * dir);
            //     mainVirtualCamera.GetCinemachineComponent<CinemachineTransposer>().m_FollowOffset = target.transform.position + (dist * dir);
            // }

            // Left
            if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.A))
            {

            }
            // Right
            if (Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.D))
            {

            }
        }
    }

    public void SetCamTarget(PhysicsObject obj)
    {
        //Obtain Scaled Distance to target
        //dist *= obj.transform.localScale.x / targetRad;

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
