﻿using System;
using UnityEngine;
using System.Collections;
using System.Linq;
using UnityEngine.PostProcessing;

public class AudioVisualTranslator : MonoBehaviour
{

    public GameObject mainCam;
    private PostProcessingProfile PPP;
    private BloomModel.Settings bloomSettings;
    private AudioSource audioSource;
    public float[] samples;
    public float[] freqBands;
    public float bass;
    public float tenor;
    public float alto;
    public float soprano;
    private float prevVal;
    private float thirdVal;
    private float lastAv;
    private bool isThird;

    // Use this for initialization
    void Start()
    {
        samples = new float[64];
        freqBands = new float[8];
        mainCam = Camera.main.gameObject;
        bloomSettings = mainCam.GetComponent<PostProcessingBehaviour>().profile.bloom.settings;
        audioSource = FindObjectOfType<AudioSource>();
    }

    // Update is called once per frame
    void Update()
    {
        GetFreqBands();
        audioSource.GetSpectrumData(samples, 0, FFTWindow.BlackmanHarris);
        float av = (prevVal + thirdVal + bass) / 3;
        if (av > 10)
            av = 10;
#if UNITY_EDITOR
        bloomSettings.bloom.intensity = 0.0f;


#else
        bloomSettings.bloom.intensity = Mathf.Lerp(Mathf.Max(lastAv, 1.0f), Mathf.Max(av, 1.0f), Time.unscaledDeltaTime * 10);
        mainCam.GetComponent<PostProcessingBehaviour>().profile.bloom.settings = bloomSettings;

#endif

        if (isThird)
        {
            thirdVal = bass;
            isThird = false;
        }
        else
        {
            prevVal = bass;
            isThird = true;
        }

        lastAv = av;
        /*
        Get8Bands();
        audioSource.GetSpectrumData(samples, 0, FFTWindow.BlackmanHarris);
        if (freqBands[1] > 10)
            freqBands[1] = 10;
        bloomSettings.bloom.intensity = Mathf.Max(freqBands[1], 1.0f);
        mainCam.GetComponent<PostProcessingBehaviour>().profile.bloom.settings = bloomSettings;
        */



    }

    void GetFreqBands()
    {
        //Bass
        float average = 0;
        for (int i = 0; i < 15; i++)
        {
            average += samples[i];
        }
        bass = average / 16;
        bass *= 100;
        //Tenor
        average = 0;
        for (int i = 16; i < 31; i++)
        {
            average += samples[i];
        }
        tenor = average / 16;
        tenor *= 1000;
        //Alto
        average = 0;
        for (int i = 32; i < 47; i++)
        {
            average += samples[i];
        }
        alto = average / 16;
        alto *= 1000;
        //Soprano
        average = 0;
        for (int i = 48; i < 63; i++)
        {
            average += samples[i];
        }
        soprano = average / 16;
        soprano *= 1000;
    }

    void Get8Bands()
    {
        int count = 0;

        for (int i = 0; i < 8; i++)
        {
            float average = 0;
            int sampleCount = (int) Mathf.Pow(2, i) * 2;

            if (i == 7)
            {
                sampleCount += 2;
            }
            for (int j = 0; j < sampleCount; j++)
            {
                average += samples[count] * (count + 1);
                count++;
            }

            average /= count;

            freqBands[i] = average * 10;
        }
    }
}