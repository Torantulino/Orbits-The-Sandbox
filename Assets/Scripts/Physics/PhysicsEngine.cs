using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PhysicsEngine : MonoBehaviour
{
    public float timeScale;
    private float timeAtPause;
    private bool paused;

    // Initialize
    void Start ()
    {
        timeScale = Time.timeScale;
    }
	
	// Simulate
	void Update ()
	{
        if(!paused)
            Time.timeScale = timeScale;
	}

    public void pauseSimulation()
    {
        timeAtPause = Time.timeScale;
        paused = true;
        Time.timeScale = 0;
    }

    public void resumeSimulation()
    {
        paused = false;
        Time.timeScale = timeAtPause;
    }

    public void timeScaled(int scale)
    {
        if (scale >= 0 && scale <= 100)
        {
            timeScale = scale;
        }
    }
}
