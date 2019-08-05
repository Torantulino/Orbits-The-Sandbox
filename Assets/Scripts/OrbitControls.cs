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

    private float _Distance;
    private void Awake()
    {
    }

    void Start()
    {
        _FocalObject = GameObject.FindGameObjectWithTag("host").transform;

        _Distance = Vector3.Distance(_FocalObject.position, transform.position);

        transform.LookAt(_FocalObject.position);

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

        var zoom = Input.mouseScrollDelta.y * _Sensitivity.z;
        if (zoom != 0 &&
            Input.mousePosition.x >= 0 && Input.mousePosition.x <= Screen.width &&
            Input.mousePosition.y >= 0 && Input.mousePosition.y <= Screen.height)
        {
            _Distance *= 1 + zoom;
        }

        // Always update position even with no input in case the target is moving.
        UpdatePosition();
    }
    private void UpdatePosition()
    {
        transform.position = _FocalObject.position - transform.forward * _Distance;
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
