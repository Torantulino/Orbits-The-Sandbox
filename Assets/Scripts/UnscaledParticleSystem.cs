using UnityEngine;

public class UnscaledParticleSystem : MonoBehaviour
{
    private ParticleSystem ps;
    void Awake()
    {
        ps = GetComponent<ParticleSystem>();
        var main = ps.main;
        main.useUnscaledTime = true;
    }

    /// This function is called when the object becomes enabled and active.
    void OnEnable()
    {
        ps.Play();
    }
}
