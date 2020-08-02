using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class ContextMenu : MonoBehaviour
{
    private Transform targetObject;
    private float distance;
    private Vector3 targetPosition;
    private OrbitControls orbitControls;
    private float spdx;
    private float spdy;
    private float spdz;
    float targetTime;
    private CinemachineVirtualCamera previewCamCtrlr;
    private Text objectTitle;
    private PhysicsObject targetPhysicsObject;
    LineRenderer lineRenderer;

    private InputField inptMassVal;
    private InputField inptRadiusVal;
    private InputField inptDensityVal;
    private InputField inptPosX;
    private InputField inptPosY;
    private InputField inptPosZ;
    private Canvas canvas;
    private CUIColorPicker colourPicker;

    // Start is called before the first frame update
    void Start()
    {
        previewCamCtrlr = FindObjectOfType<CinemachineVirtualCamera>();
        if (previewCamCtrlr == null)
            Debug.Log("Preview Cam Controller not found by " + this.name + "!");
            
        objectTitle = transform.Find("TitleObj").GetComponent<Text>();
        if(!objectTitle)
            Debug.LogError("Context Menu's 'TitleObj' not found!");
        inptPosX = transform.Find("txtPosX/inptPosX").GetComponent<InputField>();
        if(!inptPosX)
            Debug.LogError("Context Menu's 'inptPosX' not found!");
        inptPosY = transform.Find("txtPosY/inptPosY").GetComponent<InputField>();
        if (!inptPosY)
            Debug.LogError("Context Menu's 'inptPosY' not found!");
        inptPosZ = transform.Find("txtPosZ/inptPosZ").GetComponent<InputField>();
        if (!inptPosZ)
            Debug.LogError("Context Menu's 'inptPosZ' not found!");
        inptMassVal = transform.Find("txtMass/inptMassVal").GetComponent<InputField>();
        if (!inptMassVal)
            Debug.LogError("Context Menu's 'inptMassVal' not found!");
        inptRadiusVal = transform.Find("txtRadius/inptRadiusVal").GetComponent<InputField>();
        if (!inptRadiusVal)
            Debug.LogError("Context Menu's 'inptRadiusVal' not found!");
        inptDensityVal = transform.Find("txtDensity/inptDensityVal").GetComponent<InputField>();
        if (!inptDensityVal)
            Debug.LogError("Context Menu's 'inptDensityVal' not found!");
            
        canvas = FindObjectOfType<Canvas>();
        orbitControls = GameObject.FindObjectOfType<OrbitControls>();
        lineRenderer = GetComponent<LineRenderer>();
        colourPicker = FindObjectOfType<CUIColorPicker>();

        colourPicker.SetOnValueChangeCallback(SetTrailColour);
    }


    // LateUpdate is called every frame, if the Behaviour is enabled.
    // It is called after all Update functions have been called.
    void LateUpdate()
    {
        // Right click on empty space
        if (Input.GetMouseButtonDown(1) && !EventSystem.current.IsPointerOverGameObject())
        {
            RaycastHit hit;
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (!Physics.Raycast(ray, out hit, 10000))
                Close();
        }


        if (targetObject != null && gameObject.activeInHierarchy)
        {
            // Enable line renderer
            lineRenderer.enabled = true;
            
            // Calculate position diection based on camera
            Vector3 direction = Vector3.Normalize(Vector3.Normalize(Camera.main.transform.position - targetObject.transform.position) + Camera.main.transform.up);
            // Set target posiiton
            targetPosition = targetObject.position + direction * (targetObject.lossyScale.y * 0.75f);
            
            // Scale and rotate
            //transform.localScale = new Vector3(orbitControls._Distance / 2000.0f, orbitControls._Distance / 2000.0f, 1.0f) * (canvas.scaleFactor * 1000.0f);
            //transform.rotation = Camera.main.transform.rotation;
            // Move
            // if (Time.realtimeSinceStartup - targetTime < 0.5f)
            // {
            //     spdx = Mathf.Lerp(spdx, (targetPosition.x - transform.position.x) * 0.7f, 0.4f);
            //     spdy = Mathf.Lerp(spdx, (targetPosition.y - transform.position.y) * 0.7f, 0.4f);
            //     spdz = Mathf.Lerp(spdx, (targetPosition.z - transform.position.z) * 0.7f, 0.4f);
            //     transform.position = Camera.main.WorldToScreenPoint(new Vector3(transform.position.x + spdx, transform.position.y + spdy, transform.position.z + spdz));
            // }
            // else
                transform.position = Camera.main.WorldToScreenPoint(targetPosition);

            transform.position = new Vector3(transform.position.x, transform.position.y, 0.0f);

            // Draw line to planet
            lineRenderer.SetPosition(0, targetObject.position);
            lineRenderer.SetPosition(1, transform.position);


            //Update UI based on selected onject
            //Mass
            if (!inptMassVal.isFocused)
                inptMassVal.text = targetPhysicsObject.rb.mass.ToString();
            //Radius
            if (!inptRadiusVal.isFocused)
                inptRadiusVal.text = targetPhysicsObject.Radius.ToString();
            //Density
            if (!inptDensityVal.isFocused)
                inptDensityVal.text = targetPhysicsObject.Density.ToString();
            //PosX
            if (!inptPosX.isFocused)
                inptPosX.text = targetPhysicsObject.rb.position.x.ToString();
            //PosY
            if (!inptPosY.isFocused)
                inptPosY.text = targetPhysicsObject.rb.position.y.ToString();
            //PosZ
            if (!inptPosZ.isFocused)
                inptPosZ.text = targetPhysicsObject.rb.position.z.ToString();
        }
        else
        {
            gameObject.SetActive(false);
            lineRenderer.enabled = false;
        }
    }

    public void Close()
    {
        targetObject = null;
    }

    public void LockToggled(Toggle tgl)
    {
        if (targetPhysicsObject != null)
        {
            if (tgl.name == "tglMassLock")
            {
                targetPhysicsObject.massLocked = tgl.isOn;
                inptMassVal.interactable = !tgl.isOn;
            }
            else if (tgl.name == "tglDensityLock")
            {
                targetPhysicsObject.densityLocked = tgl.isOn;
                inptDensityVal.interactable = !tgl.isOn;
            }
            else if (tgl.name == "tglRadiusLock")
            {
                targetPhysicsObject.radiusLocked = tgl.isOn;
                inptRadiusVal.interactable = !tgl.isOn;
            }
        }
    }

    public void finEditingMass(string val)
    {
        float valResult;
        if (float.TryParse(val, out valResult))
        {
            targetPhysicsObject.setMass(valResult);
        }
    }
    public void finEditingRadius(string val)
    {
        float valResult;
        if (float.TryParse(val, out valResult))
        {
            targetPhysicsObject.setRadius(valResult);
        }
    }
    public void finEditingDensity(string val)
    {
        float valResult;
        if (float.TryParse(val, out valResult))
        {
            targetPhysicsObject.setDensity(valResult);
        }
    }

    public void finEditingPosX(string val)
    {
        float valResult;
        if (float.TryParse(val, out valResult))
        {
            Vector3 objPos = targetPhysicsObject.transform.position;
            targetPhysicsObject.transform.position = new Vector3(valResult, objPos.y, objPos.z);
        }
    }
    public void finEditingPosY(string val)
    {
        float valResult;
        if (float.TryParse(val, out valResult))
        {
            Vector3 objPos = targetPhysicsObject.transform.position;
            targetPhysicsObject.transform.position = new Vector3(objPos.x, valResult, objPos.z);
        }
    }
    public void finEditingPosZ(string val)
    {
        float valResult;
        if (float.TryParse(val, out valResult))
        {
            Vector3 objPos = targetPhysicsObject.transform.position;
            targetPhysicsObject.transform.position = new Vector3(objPos.x, objPos.y, valResult);
        }
    }

    public void SetTarget(GameObject _obj)
    {
        gameObject.SetActive(true);
        targetObject = _obj.transform;

        spdx = 0;
        spdy = 0;
        spdz = 0;
        targetTime = Time.realtimeSinceStartup;

        objectTitle.text = targetObject.name;

        targetPhysicsObject = targetObject.GetComponent<PhysicsObject>();

        // Update target camera
        previewCamCtrlr.m_Follow = targetObject.transform;
        previewCamCtrlr.m_LookAt = targetObject.transform;
        CinemachineTransposer transposer = previewCamCtrlr.GetCinemachineComponent<CinemachineTransposer>();
        transposer.m_FollowOffset = new Vector3(0.0f, 0.0f, Mathf.Max(targetObject.transform.localScale.z * 10.0f, 1.5f + targetObject.transform.localScale.z)); ;

    }
    public void SetTrailColour(Color color)
    {
        TrailRenderer trailRenderer = targetObject.GetComponentInChildren<TrailRenderer>();
        trailRenderer.startColor = color;
        trailRenderer.endColor = color;
        // Ensure alpha value is 0
        trailRenderer.endColor = new Vector4(trailRenderer.endColor.r, trailRenderer.endColor.g, trailRenderer.endColor.b, 0.0f);
    }
}