using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StarController : MonoBehaviour
{

    // Start is called before the first frame update
    void Start()
    {
        ParticleSystem[] ps = gameObject.GetComponentsInChildren<ParticleSystem>();
        foreach (ParticleSystem p in ps)
        {
            var main = p.main;
            main.useUnscaledTime  = true;
        }
    }

    // Update is called once per frame
    void Update()
    {

    }
}
