using UnityEngine;

public class UnscaledParticleSystem : MonoBehaviour
{
    private ParticleSystem ps;
    public GameObject tutorialCursor;

    void Awake()
    {
        ps = GetComponent<ParticleSystem>();
        var main = ps.main;
        main.useUnscaledTime = true;

    }

    /// This function is called when the object becomes enabled and active.
    void OnEnable()
    {
        //Vector3 cursorPos = cam.ScreenToWorldPoint(Input.mousePosition);
        //transform.position = new Vector3(cursorPos.x, cursorPos.y, transform.position.z);
        //transform.LookAt(Camera.main.transform.position);
        ps.Play();
    }

    
}
