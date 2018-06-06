using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Runtime.Remoting.Messaging;
using System.Threading;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Object = UnityEngine.Object;

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


    private PhysicsObject selectedObject;
    private Dictionary<string, Object> CelestialObjects = new Dictionary<string, Object>();
    private GameObject objectToSpawn;
    private GameObject activePanel;
    private GameObject planetPanel;
    private GameObject starPanel;
    private GameObject othersPanel;
    private GameObject pauseButton;
    private GameObject playButton;
    private InputField inptTime;
    private Text objectName;


    public void SetSelectedObject(PhysicsObject obj)
    {
        selectedObject = obj;
    }

    public void SetObjectToSpawn(string name)
    {
        objectToSpawn = (GameObject)CelestialObjects[name];
    }

    void Awake()
    {
        manipMode = 0;
    }

    // Use this for initialization
    void Start ()
	{
	    objectName = transform.Find("panObject/TitleObj").GetComponent<Text>();
	    playButton = transform.Find("panBottom/btnPlay").gameObject;
	    pauseButton = transform.Find("panBottom/btnPause").gameObject;
	    inptTime = transform.Find("panBottom/txtTimeScale/inptTime").GetComponent<InputField>();
        planetPanel = transform.Find("panLeft/panPlanets").gameObject;
        starPanel = transform.Find("panLeft/panStars").gameObject;
        othersPanel = transform.Find("panLeft/panOthers").gameObject;
	    activePanel = starPanel;

	    inptTime.text = Time.timeScale.ToString();

        Object[] CelestialObj = Resources.LoadAll("Prefabs/Objects");
	    foreach (Object obj in CelestialObj)
	    {
	        CelestialObjects.Add(obj.name, obj);
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
	        objectName.text = selectedObject.name;
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

	}

    /*
    void PopulatePlanetSpawner()
    {
        foreach (GameObject obj in CelestialObjects)
        {
            Transform newContent = Instantiate(contentPanel, viewPort);
            
            AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(obj), ImportAssetOptions.ForceUpdate);
            //Create texture from Prefab
            Texture newTex = null;
            //newTex = AssetPreview.GetMiniThumbnail(obj);
            newTex = AssetDatabase.GetCachedIcon(AssetDatabase.GetAssetPath(obj));
            //Create sprite from texture
            Sprite newSprite = Sprite.Create(newTex as Texture2D, new Rect(0.0f, 0.0f, newTex.width, newTex.height), new Vector2(0.5f, 0.5f));
            //Assign Spirte to button
            newContent.GetComponent<Image>().sprite = newSprite;
            
        }
        
    }*/

    public void ReloadScene()
    {
        Application.LoadLevel(Application.loadedLevel);
    }

    public void pausePressed()
    {
        PhysicsEngine.pauseSimulation();
        pauseButton.SetActive(false);
        playButton.SetActive(true);
    }

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
            screenPosition.z = Camera.main.transform.position.y;
            //Translate to world position
            Vector3 worldPosition = Camera.main.ScreenToWorldPoint(screenPosition);

            GameObject SpawnedObj = Instantiate(objectToSpawn);
            SpawnedObj.transform.position = worldPosition;
            objectToSpawn = null;
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
}
