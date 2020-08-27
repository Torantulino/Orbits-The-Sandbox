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
    public Image ColorMinus1;
    public Button GenerateButton;
    public TMP_Text _txtBrushSize;
    public TMP_InputField _inptFldFilename;
    public TMP_InputField _inptFldMass;
    public TMP_InputField _inptFldRadius;
    public GameObject LoadPanel;
    public List<CUIColorPicker> colourPickers;
    public List<RawImage> selectors;
    bool firstFrame = true;


    private void Start()
    {
        foreach (CUIColorPicker picker in colourPickers)
        {
            picker.SetRandomColor();
        }

        LoadPanel = transform.Find("LoadPanel").gameObject;
        Debug.Assert(LoadPanel);
        // GenerateButton.onClick.RemoveAllListeners();
        // GenerateButton.onClick.AddListener(() => { Generator.StartGeneration(); });

        // Randomize.onClick.RemoveAllListeners();
        // Randomize.onClick.AddListener(() => { RandomizeColors(); });
    }
    void Update() { if (firstFrame) Initialise(); }
    private void Initialise()
    {
        firstFrame = false;
        ApplyUiPickedColours();
    }

    public void SetSelector(int i)
    {
        foreach (RawImage s in selectors)
            s.color = new Color(s.color.r, s.color.g, s.color.b, 0f);

        selectors[i].color = new Color(selectors[i].color.r, selectors[i].color.g, selectors[i].color.b, 1f);
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
        Generator.SetColour(-1, ColorMinus1.color);

        Generator.ApplyColours();
        Generator.GenerateMesh();
    }

    public void ExportPlanet()
    {
        Debug.Assert(_inptFldFilename.text != "", "<b><color=magenta>Please enter a filename before saving!</color></b>");
        if (_inptFldFilename.text == "") return;

        GameObject check = Resources.Load<GameObject>("Prefabs/Objects/PlanetGenerator/" + _inptFldFilename.text);
        Debug.Assert(!check, "<b><color=magenta>There is already an object named " + _inptFldFilename.text + " , please enter a unique name!</color></b>");
        if (check)
            return;

        Debug.Log("Exporting planet as " + _inptFldFilename.text);
        Generator.Save(_inptFldFilename.text, float.Parse(_inptFldRadius.text), float.Parse(_inptFldMass.text));
    }

    public void LoadPressed()
    {
        LoadPanel.SetActive(true);

        Transform scrollView = LoadPanel.transform.Find("Panel").Find("ScrollView");
        Debug.Assert(scrollView);
        GameObject[] objects = Resources.LoadAll<GameObject>("Prefabs/Objects");

        GameObject sampleItem = scrollView.Find("Viewport/Content/SampleButton").gameObject;

        foreach (GameObject obj in objects)
        {
            GameObject item = Instantiate(sampleItem);
            item.transform.SetParent(sampleItem.transform.parent, false);
            item.GetComponentInChildren<TMP_Text>().text = obj.name;
            item.SetActive(true);
        }

        // Resize Scrollview content to fit
        sampleItem.transform.parent.GetComponent<RectTransform>().sizeDelta = new Vector2(0f, objects.Length * sampleItem.GetComponent<RectTransform>().sizeDelta.y);
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
