using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Rendering.PostProcessing;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Image = UnityEngine.UI.Image;
using Object = UnityEngine.Object;
using Toggle = UnityEngine.UI.Toggle;

public class UIManager : MonoBehaviour
{

    public InputField inptMassVal;
    public InputField inptRadiusVal;
    public InputField inptDensityVal;
    public Transform contentPanel;
    public Transform viewPort;
    public PhysicsEngine physicsEngine;
    public int manipMode; //0 = Launch Mode, 1 = Move mode
    public bool spawnSymetry;
    public int symDivs;
    public float orbVMultiplier;

    private AudioVisualTranslator audioVT;
    private PhysicsObject selectedObject;
    private CUIColorPicker colPicker;
    private Dictionary<string, Object> CelestialObjects = new Dictionary<string, Object>();
    private Dictionary<string, Material> Skyboxes = new Dictionary<string, Material>();
    private GameObject objectToSpawn;
    private GameObject activeObjectPanel;
    private GameObject planetPanel;
    private GameObject starPanel;
    private GameObject othersPanel;
    private GameObject pauseButton;
    private GameObject playButton;
    private InputField inptTime;
    private InputField inptDivs;
    private Text objectName;
    private CanvasGroup canvasGroup;
    private GameObject pausePanel;
    private InputField inptPosX;
    private InputField inptPosY;
    private InputField inptPosZ;
    private Image imgSpawnObj;
    private Color desiredTrailColor;
    private GameObject panObjects;
    private GameObject panEntities;

    private bool uiAnimating = false;
    InfiniteGrids placementGrid;

    void Awake()
    {
        manipMode = 1;
        symDivs = 5;
        spawnSymetry = false;
        orbVMultiplier = 0;

        physicsEngine = FindObjectOfType<PhysicsEngine>();
    }

    // Use this for initialization
    void Start()
    {
        panObjects = transform.Find("panObjects").gameObject;
        panEntities = transform.Find("panEntities").gameObject;
        //objectName = transform.Find("panObjects/panObject/TitleObj").GetComponent<Text>();
        playButton = transform.Find("panBottom/btnPlay").gameObject;
        pauseButton = transform.Find("panBottom/btnPause").gameObject;
        inptTime = transform.Find("panBottom/txtTimeScale/inptTime").GetComponent<InputField>();
        inptDivs = transform.Find("panObjects/panel/panSpawn/txtSym/inptDivs").GetComponent<InputField>();
        planetPanel = transform.Find("panObjects/panel/panLeft/panPlanets").gameObject;
        starPanel = transform.Find("panObjects/panel/panLeft/panStars").gameObject;
        othersPanel = transform.Find("panObjects/panel/panLeft/panOthers").gameObject;
        pausePanel = transform.Find("panPause").gameObject;
        // inptPosX = transform.Find("panObjects/panObject/txtPosX/inptPosX").GetComponent<InputField>();
        // inptPosY = transform.Find("panObjects/panObject/txtPosY/inptPosY").GetComponent<InputField>();
        // inptPosZ = transform.Find("panObjects/panObject/txtPosZ/inptPosZ").GetComponent<InputField>();
        // imgSpawnObj = transform.Find("panObjects/panBrush/imgSpawnObj").GetComponent<Image>();
        audioVT = GameObject.FindObjectOfType<AudioVisualTranslator>();
        // colPicker = GameObject.FindObjectOfType<CUIColorPicker>();
        activeObjectPanel = planetPanel;
        canvasGroup = transform.GetComponent<CanvasGroup>();
        placementGrid = FindObjectOfType<InfiniteGrids>();

        inptDivs.text = symDivs.ToString();


        // colPicker.SetOnValueChangeCallback(TrailColChanged);

        //Load Celestial Objects
        Object[] CelestialObj = Resources.LoadAll("Prefabs/Objects");
        foreach (Object obj in CelestialObj)
        {
            CelestialObjects.Add(obj.name, obj);
        }

        //Load Skyboxes
        Object[] sbxs = Resources.LoadAll("Materials/Skyboxes");
        foreach (Object skybox in sbxs)
        {
            Skyboxes.Add(skybox.name, (Material)skybox);
        }

        SetSelectedObject(GameObject.FindGameObjectWithTag("host").GetComponent<PhysicsObject>());
    }


    // Update is called once per frame
    void Update()
    {
        //TODO: Move to rightclick menu
        //Update properties of selected object based on UI
        // if (selectedObject != null)
        // {
        //     //Mass
        //     if (!inptMassVal.isFocused)
        //         inptMassVal.text = selectedObject.rb.mass.ToString();
        //     //Radius
        //     if (!inptRadiusVal.isFocused)
        //         inptRadiusVal.text = selectedObject.Radius.ToString();
        //     //Density
        //     if (!inptDensityVal.isFocused)
        //         inptDensityVal.text = selectedObject.Density.ToString();
        //     //Name
        //     objectName.text = selectedObject.name.ToUpper();
        //     //PosX
        //     if (!inptPosX.isFocused)
        //         inptPosX.text = selectedObject.rb.position.x.ToString();
        //     //PosY
        //     if (!inptPosY.isFocused)
        //         inptPosY.text = selectedObject.rb.position.y.ToString();
        //     //PosZ
        //     if (!inptPosZ.isFocused)
        //         inptPosZ.text = selectedObject.rb.position.z.ToString();

        // }

        //Update timescale based on UI
        if (!inptTime.isFocused)
        inptTime.text = (Time.timeScale / PhysicsEngine.TIMESCALER).ToString();

        //Select object
        if (Input.GetMouseButtonDown(0) && !EventSystem.current.IsPointerOverGameObject())
        {
            RaycastHit hit;
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (!Physics.Raycast(ray, out hit, 10000))
            {
                SpawnObject();
            }
            else
            {
                Debug.Log("Object Clicked!");
            }
        }
        //Show/Hide UI
        if (Input.GetKeyDown(KeyCode.F1))
        {
            if (canvasGroup.alpha == 1.0f)
            {

                canvasGroup.alpha = 0.0f;
                canvasGroup.blocksRaycasts = false;
            }
            else
            {
                canvasGroup.alpha = 1.0f;
                canvasGroup.blocksRaycasts = true;
            }
        }
        //Pause
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (!pausePanel.activeSelf)
            {
                PauseGame();
            }
            else
            {
                ResumeGame();
            }
        }
        //Delete
        if (Input.GetKeyDown(KeyCode.Delete))
        {
            Destroy(selectedObject.gameObject);
        }
    }

    public void SetSkybox(string name)
    {
        if (Skyboxes[name] != null)
            RenderSettings.skybox = Skyboxes[name];
    }

    public void SetSelectedObject(PhysicsObject obj)
    {
        selectedObject = obj;
    }

    public void ToggleNeon(bool val)
    {
        Bloom bloomSettings = Camera.main.GetComponent<PostProcessLayer>().GetSettings<Bloom>();
        if (val)
        {
            bloomSettings.intensity.value = 2.35f;
            bloomSettings.threshold.value = 0.4f;
            //bloomSettings.radius = 4.0f;
            audioVT.isActivated = true;
        }
        else
        {
            audioVT.isActivated = false;
            bloomSettings.intensity.value = 0.0f;
            bloomSettings.threshold.value = 1.0f;
            //bloomSettings.setRadius(0.0f);
            //bloomSettings.lensDirt.intensity = 0;
        }
        //TODO: Do settings need to be set again?
    }

    public void SetObjectToSpawn(string name)
    {
        objectToSpawn = (GameObject)CelestialObjects[name];

        //Set colour picker UI to reflect trail colour
        // colPicker.Color = objectToSpawn.GetComponentInChildren<TrailRenderer>().startColor;
        //reset desired trail colour
        // desiredTrailColor = colPicker.Color;
    }

    public void TrailColChanged(Color col)
    {
        //Update desired trail colour based on user selection
        desiredTrailColor = col;
    }

    // public void SetImgSpawnObj(Image btnImage)
    // {
    //     imgSpawnObj.sprite = btnImage.sprite;
    // }


    public void ReloadScene()
    {
        Application.LoadLevel(Application.loadedLevel);
    }

    //Pause
    public void pausePressed()
    {
        physicsEngine.pauseSimulation();
        pauseButton.SetActive(false);
        playButton.SetActive(true);
    }

    //Play
    public void playPressed()
    {
        physicsEngine.resumeSimulation();
        playButton.SetActive(false);
        pauseButton.SetActive(true);
    }

    public void timeScaled(string scale)
    {
        try
        {
            physicsEngine.ScaleTime(int.Parse(scale));
        }
        catch (ArgumentNullException)
        {
        }
        catch (FormatException)
        {
        }
        catch (OverflowException)
        {
        }
    }

    public void OrbVMultiplierChanged(float val)
    {
        orbVMultiplier = val;
    }

    public void trailsToggled(bool state)
    {
        if (state)
            Camera.main.cullingMask = Camera.main.cullingMask | (1 << 8);
        else
            Camera.main.cullingMask = Camera.main.cullingMask & ~(1 << 8);
    }

    public void ManipModeToggled(int mode)
    {
        manipMode = mode;
    }

    public void SpawnWithOrbitToggled(bool state)
    {
        foreach (Object item in CelestialObjects.Values)
        {
            ((GameObject)item).GetComponent<PhysicsObject>().spawnWithOrbit = state;
        }
    }

    // Called from Unity
    public void SwitchObjectPanel(int id)
    {
        if (id == 0)
        {
            if (activeObjectPanel != starPanel)
            {
                activeObjectPanel.SetActive(false);
                starPanel.SetActive(true);
                activeObjectPanel = starPanel;
            }
        }
        else if (id == 1)
        {
            if (activeObjectPanel != planetPanel)
            {
                activeObjectPanel.SetActive(false);
                planetPanel.SetActive(true);
                activeObjectPanel = planetPanel;
            }
        }
        else if (id == 2)
        {
            if (activeObjectPanel != othersPanel)
            {
                activeObjectPanel.SetActive(false);
                othersPanel.SetActive(true);
                activeObjectPanel = othersPanel;
            }
        }
    }
    void SpawnObject()
    {
        if (objectToSpawn != null)
        {

            Debug.Log("Object Spawn Start!");
            // Get mouse position on screen
            Vector3 screenPosition = Input.mousePosition;

            // Raycast into screen looking for placement plane
            Ray ray = Camera.main.ScreenPointToRay(screenPosition);

            // Check if ray hit, if so, get hitpoint
            float rayLength = 0.0f;
            Vector3 hitPoint = new Vector3();
            if (placementGrid.plane.Raycast(ray, out rayLength))
            {
                hitPoint = ray.GetPoint(rayLength);
            }
            else
            {
                Debug.Log("Ray did not hit placement plane.");
                return;
            }

            // Spawn object
            GameObject SpawnedObj = Instantiate(objectToSpawn);
            SpawnedObj.transform.position = hitPoint;

            // Set Trail colour based on UI selection
            TrailRenderer tR = SpawnedObj.GetComponentInChildren<TrailRenderer>();
            tR.startColor = desiredTrailColor;
            tR.endColor = desiredTrailColor;
            // Ensure alpha value is 0
            tR.endColor = new Vector4(tR.endColor.r, tR.endColor.g, tR.endColor.b, 0.0f);

        }
    }

    // Switches the main UI panel
    // 0: Objects, 1: Entities
    IEnumerator SwitchActivePanel(int val)
    {
        if(uiAnimating)
            yield break;
        uiAnimating = true;
        GameObject parent = null;
        GameObject panel = null;
        GameObject maximiser = null;
        bool expanding;
        int side = 0;
        float dist = 204.0f;

        // Get panel
        switch (val)
        {
            case 0: // Objects Panel
                parent = panObjects;
                side = -1;
                break;
            case 1: // Entity Panel
                parent = panEntities;
                side = 1;
                break;
        }

        {
            maximiser = parent.transform.Find("btnMaximise").gameObject;
            if (maximiser == null)
                Debug.LogError("Panel Maximise button not found.");
            panel = parent.transform.Find("panel").gameObject;
            if (panel == null)
                Debug.LogError("Panel not found.");
        }

        // Check if currently acive
        if (panel.activeSelf)
            expanding = false;
        else
            expanding = true;

        float spdx;
        float start_time;

        //##Contract##
        if (!expanding)
        {
            Debug.Log("Contracter Start pos: " + parent.transform.position);
            Vector3 target_pos = new Vector3(parent.transform.position.x + dist * side, parent.transform.position.y, parent.transform.position.z);
            spdx = 0;
            start_time = Time.realtimeSinceStartup;
            while (Time.realtimeSinceStartup - start_time < 0.1f)
            {
                spdx = Mathf.Lerp(spdx, (target_pos.x - parent.transform.position.x) * 0.7f, 0.4f);
                parent.transform.position = new Vector3(parent.transform.position.x + spdx, parent.transform.position.y, parent.transform.position.z);
                yield return new WaitForEndOfFrame();
            }
            // Lock panel inplace
            parent.transform.position = target_pos;
            Debug.Log("Contracter End pos: " + parent.transform.position);
        }


        //##After contracting##
        // Panel Exiting
        if (panel.activeSelf)
        {
            panel.SetActive(false);
            maximiser.SetActive(true);
        }
        // Panel Entering
        else
        {
            panel.SetActive(true);
            maximiser.SetActive(false);
        }

        //##EXPAND##
        if (expanding)
        {
            Debug.Log("Expander Start pos: " + parent.transform.position);
            Vector3 target_pos = new Vector3(parent.transform.position.x - dist * side, parent.transform.position.y, parent.transform.position.z);
            spdx = 0;
            start_time = Time.realtimeSinceStartup;
            while (Time.realtimeSinceStartup - start_time < 0.5f)
            {
                spdx = Mathf.Lerp(spdx, (target_pos.x - parent.transform.position.x) * 0.7f, 0.4f);
                parent.transform.position = new Vector3(parent.transform.position.x  + spdx, parent.transform.position.y, parent.transform.position.z);
                yield return new WaitForEndOfFrame();
            }
            parent.transform.position = target_pos;
            Debug.Log("Expander End pos: " + parent.transform.position);
        }

        uiAnimating = false;
        yield return new WaitForSeconds(0.0f);
    }
    public void SwitchTab(int val)
    {
        StartCoroutine(SwitchActivePanel(val));
    }

    public void LockToggled(Toggle tgl)
    {
        if (selectedObject != null)
        {
            if (tgl.name == "tglMassLock")
            {
                selectedObject.massLocked = tgl.isOn;
                inptMassVal.interactable = !tgl.isOn;
            }
            else if (tgl.name == "tglDensityLock")
            {
                selectedObject.densityLocked = tgl.isOn;
                inptDensityVal.interactable = !tgl.isOn;
            }
            else if (tgl.name == "tglRadiusLock")
            {
                selectedObject.radiusLocked = tgl.isOn;
                inptRadiusVal.interactable = !tgl.isOn;
            }
        }
    }

    public void toggleSymetry(bool state)
    {
        spawnSymetry = state;
        inptDivs.interactable = state;
    }

    public void DivsChanged()
    {
        int result;
        if (int.TryParse(inptDivs.text, out result))
        {
            if (result > 1 && result < 100)
            {
                symDivs = result;
            }
            else
            {
                inptDivs.text = symDivs.ToString();
            }
        }
        else
        {
            inptDivs.text = symDivs.ToString();
        }
    }

    public void finEditingMass(string val)
    {
        float valResult;
        if (float.TryParse(val, out valResult))
        {
            selectedObject.setMass(valResult);
        }
    }
    public void finEditingRadius(string val)
    {
        float valResult;
        if (float.TryParse(val, out valResult))
        {
            selectedObject.setRadius(valResult);
        }
    }
    public void finEditingDensity(string val)
    {
        float valResult;
        if (float.TryParse(val, out valResult))
        {
            selectedObject.setDensity(valResult);
        }
    }

    public void finEditingPosX(string val)
    {
        float valResult;
        if (float.TryParse(val, out valResult))
        {
            Vector3 objPos = selectedObject.transform.position;
            selectedObject.transform.position = new Vector3(valResult, objPos.y, objPos.z);
        }
    }
    public void finEditingPosY(string val)
    {
        float valResult;
        if (float.TryParse(val, out valResult))
        {
            Vector3 objPos = selectedObject.transform.position;
            selectedObject.transform.position = new Vector3(objPos.x, valResult, objPos.z);
        }
    }
    public void finEditingPosZ(string val)
    {
        float valResult;
        if (float.TryParse(val, out valResult))
        {
            Vector3 objPos = selectedObject.transform.position;
            selectedObject.transform.position = new Vector3(objPos.x, objPos.y, valResult);
        }
    }

    public void PauseGame()
    {
        pausePanel.SetActive(true);
        physicsEngine.pauseSimulation();
    }

    public void ResumeGame()
    {
        // if(pausePanel == null)
        //    pausePanel = GameObject.Find("panPause").gameObject;
        pausePanel.SetActive(false);
        physicsEngine.resumeSimulation();
    }

    public void Quit()
    {
        Application.Quit();
    }
}
