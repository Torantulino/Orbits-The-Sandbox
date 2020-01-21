using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SocialMediaButton : MonoBehaviour
{
    public string url;

    public bool disableOnce;
    AudioSource audioSource;

    void Start()
    {
        audioSource = FindObjectOfType<AudioSource>();
    }

    void PlaySound(AudioClip _sound)
    {

        if (!disableOnce)
        {
            audioSource.PlayOneShot(_sound);
        }
        else
        {
            disableOnce = false;
        }
    }

    void VistWebsite()
    {
        Application.OpenURL(url);
    }
}
