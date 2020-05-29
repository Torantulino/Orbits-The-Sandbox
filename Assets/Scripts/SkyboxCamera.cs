using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkyboxCamera : MonoBehaviour
{
    private Camera _camera;
    // Start is called before the first frame update
    void Start()
    {
        _camera = GetComponent<Camera>();
    }

    // Update is called once per frame
    void Update()
    {
        _camera.fieldOfView = Camera.main.fieldOfView;
        _camera.transform.rotation = Camera.main.transform.rotation;

        _camera.transform.position = Camera.main.transform.position / 11.21f;
    }
}
