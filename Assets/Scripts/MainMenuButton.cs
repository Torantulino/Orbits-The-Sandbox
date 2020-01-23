using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainMenuButton : MonoBehaviour
{
    AudioSource audioSource;
    Vector3 initial_position;
    Animator animator;

    void Start()
    {
        audioSource = FindObjectOfType<AudioSource>();
        animator = gameObject.GetComponent<Animator>();
        initial_position = transform.position;
    }

    void PlaySound(AudioClip _sound)
    {
        audioSource.PlayOneShot(_sound);
    }

    // Update is called once per frame
    void Update()

    {
        // if (animator.GetCurrentAnimatorStateInfo(0).IsName("Normal"))
        // {
        //     Vector3 offset = new Vector3(Mathf.PerlinNoise(Time.realtimeSinceStartup/7.5f, Time.realtimeSinceStartup / 7.5f), Mathf.PerlinNoise(Time.realtimeSinceStartup/7.5f + 0.33f, Time.realtimeSinceStartup / 7.5f), 0.0f);
        //     transform.position = initial_position + offset * 10.0f;
        // }
        
    }
}
