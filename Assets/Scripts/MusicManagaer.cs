using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MusicManagaer : MonoBehaviour
{

    private AudioSource source;
    private Object[] soundtrack;
    private int lastPlayed;

    void Start()
    {
        //Load soundtrack
        soundtrack = Resources.LoadAll("Music");
        //Obtain reference to audiosource
        source = transform.Find("Source").GetComponent<AudioSource>();

        StartCoroutine(playSoundtrack());
    }

    IEnumerator playSoundtrack()
    {
        yield return null;

        //Loop through soundtrack
        for (int i = 0; i < soundtrack.Length; i++)
        {
            //Assign current track to source
            source.clip = (AudioClip) soundtrack[i];

            //Start track
            source.Play();

            //Wait for track to finish
            while (source.isPlaying)
            {
                yield return null;
            }
        }
        StartCoroutine(playSoundtrack());
    }
}