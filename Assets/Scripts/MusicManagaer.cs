using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MusicManagaer : MonoBehaviour
{
    public static MusicManagaer MusicMan = null;
    private AudioSource source;
    private Object[] soundtrack;
    private int lastPlayed;

    void Awake()
    {
        if (MusicMan == null)
        {
            MusicMan = this;
            DontDestroyOnLoad(this.gameObject);
        }
        else
        {
            Destroy(this.gameObject);

        }
    }

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
            source.clip = (AudioClip)soundtrack[i];

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