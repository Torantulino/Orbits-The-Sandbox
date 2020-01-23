// Original script: Animancer // Copyright 2019 Kybernetik //
// Modified by Toran Bruce Richards.

using UnityEngine;


[AddComponentMenu("Animancer/Examples/Orbit Controls")]
public class OrbitControls : MonoBehaviour
{
    [SerializeField]
    private Transform _FocalObject;
    public Transform FocalObject{
        get{ return _FocalObject; }
    }

    [SerializeField]
    [Range(-1, 2)]
    private int _MouseButton = 1;

    [SerializeField]
    private Vector3 _Sensitivity = new Vector3(15, -10, -0.1f);

    public float _TargetDistance;
    float _StartDistance;
    float _LerpTime;

    public AnimationCurve nearClippingCurve = new AnimationCurve();
    public AnimationCurve farClippingCurve = new AnimationCurve();

    float ZoomSpeed = 3.0f;
    
    UIManager uIManager;

    private Camera bloomCam;

    void Start()
    {
        bloomCam = GameObject.FindGameObjectWithTag("BloomCam").GetComponent<Camera>();
        uIManager = FindObjectOfType<UIManager>();

        _FocalObject = GameObject.FindGameObjectWithTag("host").transform;

        _TargetDistance = Vector3.Distance(_FocalObject.position, transform.position);

        transform.LookAt(_FocalObject.position);

        // Set-up animation curves
        // t = 0
        Keyframe key = new Keyframe(0.0f, 0.01f);

        nearClippingCurve.AddKey(0.0f, 0.01f);
        farClippingCurve.AddKey(0.0f, 10.0f);
        // t = 3
        nearClippingCurve.AddKey(3.0f, 1.0f);
        farClippingCurve.AddKey(3.0f, 20000.0f);
        // t = 100
        nearClippingCurve.AddKey(100.0f, 1.5f);
        farClippingCurve.AddKey(100.0f, 25000.0f);
        // t = 1000
        nearClippingCurve.AddKey(1000.0f, 10.0f);
        farClippingCurve.AddKey(1000.0f, 200000.0f);
        // t = 100000
        nearClippingCurve.AddKey(100000.0f, 200.0f);
        farClippingCurve.AddKey(100000.0f, 2000000.0f);

        for (int i = 0; i < 5; i++)
        {
            farClippingCurve.SmoothTangents(i, -1.0f);
            nearClippingCurve.SmoothTangents(i, -1.0f);
        }
    }

    private void Update()
    {
#if UNITY_EDITOR
            if (!UnityEditor.EditorApplication.isPlayingOrWillChangePlaymode)
            {
                transform.LookAt(_FocalObject.position);
                return;
            }
#endif
        if (uIManager.pausePanel.activeSelf)
            return;

        if (_MouseButton < 0 || Input.GetMouseButton(_MouseButton))
        {
            var movement = new Vector2(
                Input.GetAxis("Mouse X"),
                Input.GetAxis("Mouse Y"));

            if (movement != Vector2.zero)
            {
                var euler = transform.localEulerAngles;
                euler.y += movement.x * _Sensitivity.x;
                euler.x += movement.y * _Sensitivity.y;
                if (euler.x > 180)
                    euler.x -= 360;
                euler.x = Mathf.Clamp(euler.x, -80, 80);
                transform.localEulerAngles = euler;
            }
        }

        // Smooth Zoom
        var zoom = Input.mouseScrollDelta.y * _Sensitivity.z;
        if (zoom != 0 &&
            Input.mousePosition.x >= 0 && Input.mousePosition.x <= Screen.width &&
            Input.mousePosition.y >= 0 && Input.mousePosition.y <= Screen.height)
        {
            if((_TargetDistance * 1 + zoom) >= _FocalObject.transform.localScale.x / 1.1f || zoom > 0.0f)
            {
                _TargetDistance *= 1 + zoom;
                Camera.main.nearClipPlane = nearClippingCurve.Evaluate(_TargetDistance);
                bloomCam.nearClipPlane = nearClippingCurve.Evaluate(_TargetDistance);
                Camera.main.farClipPlane = farClippingCurve.Evaluate(_TargetDistance);
                bloomCam.farClipPlane = farClippingCurve.Evaluate(_TargetDistance);
            }

            _StartDistance = Vector3.Distance(transform.position, _FocalObject.position);
            _LerpTime = 0.0f;

            // Tutorial Trigger
            if (uIManager.tutorial)
            {
                Debug.Log("Zoom: " + zoom);
                if (zoom < -0.075f)
                {
                    if (uIManager.tutorialCursorAnimator.GetInteger("tutorialPhase") == 3)
                        uIManager.tutorialTrigger = true;
                }
                else if (zoom > 0.075f)
                {
                    if (uIManager.tutorialCursorAnimator.GetInteger("tutorialPhase") == 2)
                        uIManager.tutorialTrigger = true;
                }
            }
        }

        // Always update position even with no input in case the target is moving.
        UpdatePosition();
    }
    private void UpdatePosition()
    {
        try
        {
            _LerpTime += Time.unscaledDeltaTime * ZoomSpeed;

            //float _CurrentDisatance = Vector3.Distance(_FocalObject.transform.position, transform.position);
            float _Disatance = Mathf.Lerp(_StartDistance, _TargetDistance, _LerpTime);

            transform.position = _FocalObject.position - transform.forward * _Disatance;
        }
        catch
        {

        }
    }
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(0.5f, 1, 0.5f, 1);
        Gizmos.DrawLine(transform.position, _FocalObject.position);
    }

    public void SetFocalObject(GameObject obj)
    {
        _FocalObject = obj.transform;
    }
}
