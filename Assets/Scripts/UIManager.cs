using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Runtime.Remoting.Messaging;
using System.Threading;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Experimental.UIElements;
using UnityEngine.PostProcessing;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Button = UnityEngine.Experimental.UIElements.Button;
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
    public PhysicsEngine PhysicsEngine;
    public int manipMode; //0 = Launch Mode, 1 = Move mode
    public bool spawnWithOrbit = true;
    public bool spawnSymetry;
    public int symDivs;
    public float orbVMultiplier;

    private AudioVisualTranslator audioVT;
    private PhysicsObject selectedObject;
    private CUIColorPicker colPicker;
    private Dictionary<string, Object> CelestialObjects = new Dictionary<string, Object>();
    private Dictionary<string, Material> Skyboxes = new Dictionary<string, Material>();
    private GameObject objectToSpawn;
    private GameObject activePanel;
    private GameObject planetPanel;
    private GameObject starPanel;
    private GameObject othersPanel;
    private GameObject pauseButton;
    private GameObject playButton;
    private InputField inptTime;
    private InputField inptDivs;
    private Text objectName;
    private CanvasGroup canvasGroup;
    private CamController camController;
    private GameObject pausePanel;
    private InputField inptPosX;
    private InputField inptPosY;
    private InputField inptPosZ;
    private Image imgSpawnObj;
    private Color desiredTrailColor;
    private GameObject panObjects;
    private GameObject panBrush;
    private GameObject panSpawn;
    private GameObject panObject;
    private UnityEngine.UI.Button tabObj;
    private UnityEngine.UI.Button tabScene;


    void Awake()
    {
        manipMode = 1;
        symDivs = 5;
        spawnSymetry = true;
        orbVMultiplier = 0;
    }

    // Use this for initialization
    void Start ()
    {
        panObjects = transform.Find("panLeft").gameObject;
        panBrush = transform.Find("panBrush").gameObject;
        panSpawn = transform.Find("panSpawn").gameObject;
        panObject = transform.Find("panObject").gameObject;
	    objectName = transform.Find("panObject/TitleObj").GetComponent<Text>();
	    playButton = transform.Find("panBottom/btnPlay").gameObject;
	    pauseButton = transform.Find("panBottom/btnPause").gameObject;
	    inptTime = transform.Find("panBottom/txtTimeScale/inptTime").GetComponent<InputField>();
	    inptDivs = transform.Find("panSpawn/txtSym/inptDivs").GetComponent<InputField>();
        planetPanel = transform.Find("panLeft/panPlanets").gameObject;
        starPanel = transform.Find("panLeft/panStars").gameObject;
        othersPanel = transform.Find("panLeft/panOthers").gameObject;
	    pausePanel = transform.Find("panPause").gameObject;
	    inptPosX = transform.Find("panObject/txtPosX/inptPosX").GetComponent<InputField>();
	    inptPosY = transform.Find("panObject/txtPosY/inptPosY").GetComponent<InputField>();
	    inptPosZ = transform.Find("panObject/txtPosZ/inptPosZ").GetComponent<InputField>();
	    imgSpawnObj = transform.Find("panBrush/imgSpawnObj").GetComponent<Image>();
        tabObj = transform.Find("panTabs/tabObjs/btnObjs").GetComponent<UnityEngine.UI.Button>();
        tabScene = transform.Find("panTabs/tabScene/btnScene").GetComponent<UnityEngine.UI.Button>();
        audioVT = GameObject.FindObjectOfType<AudioVisualTranslator>();
	    colPicker = GameObject.FindObjectOfType<CUIColorPicker>();
        activePanel = starPanel;
	    canvasGroup = transform.GetComponent<CanvasGroup>();

	    inptDivs.text = symDivs.ToString();

	    inptTime.text = Time.timeScale.ToString();

	    camController = FindObjectOfType<CamController>();

	    //colPicker.SetOnValueChangeCallback(TrailColChanged);                                                  ##########PUT BACK##########

        //Highlight Active Tab
        ColorBlock colBlock = ColorBlock.defaultColorBlock;
        colBlock.colorMultiplier = 1.5f;
        tabObj.colors = colBlock;

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
	void Update ()      
	{
        //Update properties of selected object based on UI
	    if (selectedObject != null)
	    {
            //Mass
            if(!inptMassVal.isFocused)
	            inptMassVal.text = selectedObject.rb.mass.ToString();
            //Radius
            if(!inptRadiusVal.isFocused)
                inptRadiusVal.text = selectedObject.Radius.ToString();
            //Density
            if(!inptDensityVal.isFocused)
                inptDensityVal.text = selectedObject.Density.ToString();            
            //Name
	        objectName.text = selectedObject.name.ToUpper();
	        //PosX
            if (!inptPosX.isFocused)
	            inptPosX.text = selectedObject.rb.position.x.ToString();
	        //PosY
            if(!inptPosY.isFocused)
	            inptPosY.text = selectedObject.rb.position.y.ToString();
	        //PosZ
            if(!inptPosZ.isFocused)
    	        inptPosZ.text = selectedObject.rb.position.z.ToString();

        }

        //Update timescale based on UI
        if (!inptTime.isFocused)
	        inptTime.text = Time.timeScale.ToString();

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
	    if (Input.GetKeyDown(KeyCode.Delete) || Input.GetKeyDown(KeyCode.Backspace))
	    {
	        Destroy(selectedObject.gameObject);
	    }
	}

    public void SetSkybox(string name)
    {
        if(Skyboxes[name] != null)
            RenderSettings.skybox = Skyboxes[name];
    }

    public void SetSelectedObject(PhysicsObject obj)
    {
        selectedObject = obj;
    }

    public void ToggleNeon(bool val)
    {
        BloomModel.Settings bloomSettings = Camera.main.GetComponent<PostProcessingBehaviour>().profile.bloom.settings;
        if (val)
        {
            bloomSettings.bloom.intensity = 2.35f;
            bloomSettings.bloom.threshold = 0.4f;
            bloomSettings.bloom.radius = 4.0f;
            bloomSettings.lensDirt.intensity = 0.0f;
            audioVT.isActivated = true;
        }
        else
        {
            audioVT.isActivated = false;
            bloomSettings.bloom.intensity = 0.5f;
            bloomSettings.bloom.threshold = 1.0f;
            bloomSettings.bloom.radius = 2.99f;
            bloomSettings.lensDirt.intensity = 10;
        }
        Camera.main.GetComponent<PostProcessingBehaviour>().profile.bloom.settings = bloomSettings;
    }

    public void SetObjectToSpawn(string name)
    {
        objectToSpawn = (GameObject)CelestialObjects[name];
        //Set colour picker UI to reflect trail colour
        colPicker.Color = objectToSpawn.GetComponentInChildren<TrailRenderer>().startColor;
        //reset desired trail colour
        desiredTrailColor = colPicker.Color;
    }

    public void TrailColChanged(Color col)
    {
        //Update desired trail colour based on user selection
        desiredTrailColor = col;
    }

    public void SetImgSpawnObj(Image btnImage)
    {
        imgSpawnObj.sprite = btnImage.sprite;
    }


    public void ReloadScene()
    {
        Application.LoadLevel(Application.loadedLevel);
    }

    //Pause
    public void pausePressed()
    {
        PhysicsEngine.pauseSimulation();
        pauseButton.SetActive(false);
        playButton.SetActive(true);
    }

    //Play
    public void playPressed()
    {
        PhysicsEngine.resumeSimulation();
        playButton.SetActive(false);
        pauseButton.SetActive(true);
    }

    public void timeScaled(string scale)
    {
        try
        {
            PhysicsEngine.timeScale = int.Parse(scale);
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
        if(state)
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
        spawnWithOrbit = state;
    }

    public void SwitchPanels(int id)
    {
        if (id == 0)
        {
            if (activePanel != starPanel)
            {
                activePanel.SetActive(false);
                starPanel.SetActive(true);
                activePanel = starPanel;
            }
        }
        else if (id == 1)
        {
            if (activePanel != planetPanel)
            {
                activePanel.SetActive(false);
                planetPanel.SetActive(true);
                activePanel = planetPanel;
            }
        }
        else if (id == 2)
        {
            if (activePanel != othersPanel)
            {
                activePanel.SetActive(false);
                othersPanel.SetActive(true);
                activePanel = othersPanel;
            }
        }
    }

    void SpawnObject()
    {
        if (objectToSpawn != null)
        {
            Debug.Log("test!");
            //Get mouse position on screen
            Vector3 screenPosition = Input.mousePosition;
            screenPosition.z = camController.transform.position.y;
            //Translate to world position
            Vector3 worldPosition = Camera.main.ScreenToWorldPoint(screenPosition);

            GameObject SpawnedObj = Instantiate(objectToSpawn);
            SpawnedObj.transform.position = worldPosition;

            //Set Trail colour based on UI selection
            TrailRenderer tR = SpawnedObj.GetComponentInChildren<TrailRenderer>();
            tR.startColor = desiredTrailColor;
            tR.endColor = desiredTrailColor;
            //Ensure alpha value is 0
            tR.endColor = new Vector4(tR.endColor.r, tR.endColor.g, tR.endColor.b, 0.0f);

        }
    }

    public void SwitchTab(int val)
    {
        //If Obejcts Tab Pressed
        if (val == 0)
        {
            //Toggle All panels
            panObjects.SetActive(!panObjects.activeSelf);
            panBrush.SetActive(!panBrush.activeSelf);
            panSpawn.SetActive(!panSpawn.activeSelf);
            panObject.SetActive(!panObject.activeSelf);

            if (panObject.activeSelf)
            {
                //Highlight Active Tab
                ColorBlock colBlock = ColorBlock.defaultColorBlock;
                colBlock.colorMultiplier = 1.5f;
                tabObj.colors = colBlock;
            }
            else
            {
                //Highlight Active Tab
                ColorBlock colBlock = ColorBlock.defaultColorBlock;
                colBlock.colorMultiplier = 1.0f;
                tabObj.colors = colBlock;

            }
        } 
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
            if(result > 1 && result < 100)
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
        PhysicsEngine.pauseSimulation();
    }

    public void ResumeGame()
    {
       // if(pausePanel == null)
        //    pausePanel = GameObject.Find("panPause").gameObject;
        pausePanel.SetActive(false);
        PhysicsEngine.resumeSimulation();
    }

    public void Quit()
    {
        Application.Quit();
    }
}
