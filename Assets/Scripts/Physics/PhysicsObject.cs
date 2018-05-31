using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class PhysicsObject : MonoBehaviour
{
    public Vector3 position;
    public Vector3 velocity;
    public float mass;
    public float density;
    public float radius;
    public bool massLocked;
    public bool densityLocked;
    public bool radiusLocked;
    public float volume;

    private bool massChanged;
    private bool densityChanged;
    private bool radiusChanged;
    private bool volumeChanged;
    private float oldRadius;
    private float oldMass;
    private float oldDensity;

	// Use this for initialization
	void Start ()
	{
	    position = gameObject.transform.position;
	    radius = gameObject.transform.localScale.x;
	}

    void Update()
    {
        gameObject.transform.localScale = new Vector3(radius, radius, radius);

        //Check for Property Changes
        if (radius != oldRadius)
        {
            radiusChanged = true;
        }
        else
        {
            radiusChanged = false;
        }


        if (massChanged || densityChanged || radiusChanged || volumeChanged)
        {
            UpdateProperties();
        }
        oldRadius = radius;
        oldMass = mass;
        oldDensity = density;
    }

    // Simulate
    void FixedUpdate ()
	{
        Integrate(Time.deltaTime);
	    gameObject.transform.position = position;
	}

    void Integrate(float deltaT)
    {
        position += velocity * deltaT;
    }

    void UpdateProperties()
    {
        if (massChanged)
        {
            if (massLocked)
            {
                mass = oldMass;
            }
            //Update Radius
            radius = Mathf.Pow((3 * (volume / 4 * Mathf.PI)),  1 / 3);
            //Update Volume
            volume = (4 / 3) * Mathf.PI * Mathf.Pow(radius, 3);
        }
        if (densityChanged)
        {
            if (densityLocked)
            {
                
            }
            mass = density * volume;
        }
        if (radiusChanged)
        {
            volume = (4 / 3) * Mathf.PI * Mathf.Pow(radius, 3);
            mass = density * volume;
        }
    }

}
