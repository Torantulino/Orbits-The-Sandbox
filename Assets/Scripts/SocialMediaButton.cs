using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SocialMediaButton : MonoBehaviour
{
    public string url;
    AudioSource audioSource;

    void Start()
    {
        audioSource = FindObjectOfType<AudioSource>();
    }

    void PlaySound(AudioClip _sound)
    {
        audioSource.PlayOneShot(_sound);
    }

    void VistWebsite()
    {
        Application.OpenURL(url);
    }
}
