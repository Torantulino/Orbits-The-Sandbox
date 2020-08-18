using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UserInterface : MonoBehaviour
{
    public PlanetGenerator Generator;
    public Button Randomize;
    public Button GenerateButton;

    private void Start()
    {
        GenerateButton.onClick.RemoveAllListeners();
        GenerateButton.onClick.AddListener(() => {Generator.StartGeneration();});

        Randomize.onClick.RemoveAllListeners();
        Randomize.onClick.AddListener(() => {RandomizeColors();});
    }

    private void RandomizeColors()
    {
        RandomizeSettings();
        List<ColorSetting> randomizedColors = new List<ColorSetting>();
        for(int i = 0; i < Generator.Colors.Count; i++)
        {
            var currentColorSetting = new ColorSetting();
            currentColorSetting.name = Generator.Colors[i].name;
            currentColorSetting.color = new Color(Random.Range(0f,1f),Random.Range(0f,1f),Random.Range(0f,1f));
            randomizedColors.Add(currentColorSetting);
        }
        Generator.Colors.Clear();
        Generator.Colors.AddRange(randomizedColors);
        Generator.StartGeneration();
    }

    private void RandomizeSettings()
    {
        Generator.MaxAmountOfContinents = Random.Range(0,15);
        Generator.ContinentsMinSize = Random.Range(0.1f,0.5f);
        Generator.ContinentsMaxSize = Random.Range(.5f,1f);
        Generator.MountainBaseSize = Random.Range(.1f,1f);
        Generator.MaxAmountOfMountains = Random.Range(1,19);
        Generator.MinMountainHeight = Random.Range(.01f,0.1f);
        Generator.MaxMountainHeight = Random.Range(.1f,.5f);
        Generator.MinLandExtrusionHeight = Random.Range(.01f,.05f);
        Generator.MaxLandExtrusionHeight = Random.Range(.05f,1f);
        Generator.MinBumpFactor = Random.Range(.75f, 1.01f);
        Generator.MaxBumpFactor = Random.Range(1.01f, 1.25f);
        Generator.DrawShore = Random.Range(0,100) < 50 ? false : true;
        Generator.MinShoreWidth = Random.Range(.01f,0.05f);
        Generator.MaxShoreWidth = Random.Range(0.05f,0.15f);
        Generator.SmoothNormals = Random.Range(0,100) <= 50 ? true : false;

    }
}
