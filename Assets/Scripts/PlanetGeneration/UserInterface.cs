using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UserInterface : MonoBehaviour
{
    public PlanetGenerator Generator;
    public Image Color2;
    public Image Color1;
    public Image Color0;
    public Button GenerateButton;
    public TMP_Text _txtBrushSize;

    private void Start()
    {
        // GenerateButton.onClick.RemoveAllListeners();
        // GenerateButton.onClick.AddListener(() => { Generator.StartGeneration(); });

        // Randomize.onClick.RemoveAllListeners();
        // Randomize.onClick.AddListener(() => { RandomizeColors(); });
    }

    public void HorizontalRotationUpdated(float _newValue)
    {
        Generator.Rotate = false;
        Vector3 currentAngles = Generator.transform.eulerAngles;
        Generator.transform.eulerAngles = new Vector3(currentAngles.x, _newValue * 360.0f, currentAngles.z);
    }

    public void VerticalRotationUpdated(float _newValue)
    {
        Generator.Rotate = false;
        Vector3 currentAngles = Generator.transform.eulerAngles;
        Generator.transform.eulerAngles = new Vector3(currentAngles.x, currentAngles.y, Mathf.Lerp(90.0f, -90.0f, _newValue));
    }

    public void BrushSizeSliderUpdate(float _newValue)
    {
        _txtBrushSize.text = _newValue.ToString("0.000");
        Generator.SetBrushSize(_newValue);
    }

    public void ApplyUiPickedColours()
    {

        Generator.SetColour(2, Color2.color);
        Generator.SetColour(1, Color1.color);
        Generator.SetColour(0, Color0.color);

        Generator.ApplyColours();
        Generator.GenerateMesh();
    }

    private void RandomizeColors()
    {
        Dictionary<int, Color> randomizedColors = new Dictionary<int, Color>();

        foreach (KeyValuePair<int, Color> kvp in Generator.Colors)
            randomizedColors.Add(kvp.Key, new Color(Random.Range(0f, 1f), Random.Range(0f, 1f), Random.Range(0f, 1f)));

        Generator.Colors.Clear();

        foreach (KeyValuePair<int, Color> kvp in randomizedColors)
            Generator.Colors.Add(kvp.Key, kvp.Value);

        Generator.ApplyColours();
        Generator.GenerateMesh();
    }

    private void RandomizeSettings()
    {
        Generator.MaxAmountOfContinents = Random.Range(0, 15);
        Generator.ContinentsMinSize = Random.Range(0.1f, 0.5f);
        Generator.ContinentsMaxSize = Random.Range(.5f, 1f);
        Generator.MountainBaseSize = Random.Range(.1f, 1f);
        Generator.MaxAmountOfMountains = Random.Range(1, 19);
        Generator.MinMountainHeight = Random.Range(.01f, 0.1f);
        Generator.MaxMountainHeight = Random.Range(.1f, .5f);
        Generator.MinLandExtrusionHeight = Random.Range(.01f, .05f);
        Generator.MaxLandExtrusionHeight = Random.Range(.05f, 1f);
        Generator.MinBumpFactor = Random.Range(.75f, 1.01f);
        Generator.MaxBumpFactor = Random.Range(1.01f, 1.25f);
        Generator.DrawShore = Random.Range(0, 100) < 50 ? false : true;
        Generator.MinShoreWidth = Random.Range(.01f, 0.05f);
        Generator.MaxShoreWidth = Random.Range(0.05f, 0.15f);
        Generator.SmoothNormals = Random.Range(0, 100) <= 50 ? true : false;

    }
}
