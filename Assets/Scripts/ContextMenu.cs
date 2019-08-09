using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ContextMenu : MonoBehaviour
{
    private Transform targetObject;
    private Canvas canvas;
    private float distance;
    private Vector3 targetPosition;
    private OrbitControls orbitControls;
    private float spdx;
    private float spdy;
    private float spdz;
    float targetTime;

    // Start is called before the first frame update
    void Start()
    {
        canvas = GetComponent<Canvas>();
        orbitControls = GameObject.FindObjectOfType<OrbitControls>();
    }


    // LateUpdate is called every frame, if the Behaviour is enabled.
    // It is called after all Update functions have been called.
    void LateUpdate()
    {
        if(targetObject != null && canvas.enabled)
        {
            targetPosition = targetObject.position + Vector3.up * (targetObject.lossyScale.y*0.75f);
            transform.localScale = new Vector3(orbitControls._Distance/2000.0f, orbitControls._Distance/2000.0f, 1.0f);
            transform.rotation = Camera.main.transform.rotation;
            if(Time.realtimeSinceStartup - targetTime < 1.0f)
            {
                spdx = Mathf.Lerp(spdx, (targetPosition.x - transform.position.x) * 0.7f, 0.4f);
                spdy = Mathf.Lerp(spdx, (targetPosition.y - transform.position.y) * 0.7f, 0.4f);
                spdz = Mathf.Lerp(spdx, (targetPosition.z - transform.position.z) * 0.7f, 0.4f);
                transform.position = new Vector3(transform.position.x + spdx, transform.position.y + spdy, transform.position.z + spdz);
            }
            else
                transform.position = targetPosition;

        }
        else
            canvas.enabled = false;
    }

    public void SetTarget(GameObject _obj)
    {
        canvas.enabled = true;
        targetObject = _obj.transform;

        spdx = 0;
        spdy = 0;
        spdz = 0;
        targetTime = Time.realtimeSinceStartup;
    }
}
