using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Runtime.Remoting.Messaging;
using System.Threading;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{

    public InputField inptMassVal;
    public InputField inptRadiusVal;
    public InputField inptDensityVal;
    public bool editingMass;
    public bool editingRadius;
    public bool editingDensity;
    public Transform contentPanel;
    public Transform viewPort;
    public PhysicsEngine PhysicsEngine;

    private PhysicsObject selectedObject;
    private Dictionary<string, Object> CelestialObjects = new Dictionary<string, Object>();
    private GameObject objectToSpawn;
    private GameObject activePanel;
    private GameObject planetPanel;
    private GameObject starPanel;
    private GameObject othersPanel;
    private GameObject pauseButton;
    private GameObject playButton;

    public void SetSelectedObject(PhysicsObject obj)
    {
        selectedObject = obj;
    }

    public void SetObjectToSpawn(string name)
    {
        objectToSpawn = (GameObject)CelestialObjects[name];
    }

	// Use this for initialization
	void Start ()
	{
	    editingMass = false;
	    editingDensity = false;
	    editingRadius = false;

	    playButton = transform.Find("panBottom/btnPlay").gameObject;
	    pauseButton = transform.Find("panBottom/btnPause").gameObject;
        planetPanel = transform.Find("panLeft/panPlanets").gameObject;
        starPanel = transform.Find("panLeft/panStars").gameObject;
        othersPanel = transform.Find("panLeft/panOthers").gameObject;
	    activePanel = starPanel;


        Object[] CelestialObj = Resources.LoadAll("Prefabs/Objects");
	    foreach (Object obj in CelestialObj)
	    {
	        CelestialObjects.Add(obj.name, obj);
	    }
    }

	
	// Update is called once per frame
	void Update ()      
	{
	    if (selectedObject != null)
	    {
            if(!editingMass && !selectedObject.massLocked)
	            inptMassVal.text = selectedObject.rb.mass.ToString();
            if(!editingRadius && !selectedObject.radiusLocked)
                inptRadiusVal.text = selectedObject.radius.ToString();
            if(!editingDensity && !selectedObject.densityLocked)
                inptDensityVal.text = selectedObject.density.ToString();
	    }



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

    public void IsEditingMass()
    {
        editingMass = true;
    }
    public void IsEditingRadius()
    {
        editingRadius = true;
    }
    public void IsEditingDensity()
    {
        editingDensity = true;
    }
    public void finEditingMass(string val)
    {
        float valResult;
        if (float.TryParse(val, out valResult))
        {
            selectedObject.rb.mass = valResult;
            selectedObject.massChanged = true;
        }
        editingMass = false;
    }
    public void finEditingRadius(string val)
    {
        float valResult;
        if (float.TryParse(val, out valResult))
        {
            selectedObject.radius = valResult;
            selectedObject.radiusChanged = true;
        }
        editingRadius = false;
    }
    public void finEditingDensity(string val)
    {
        float valResult;
        if (float.TryParse(val, out valResult))
        {
            selectedObject.density = valResult;
            selectedObject.densityChanged = true;
        }
        editingDensity = false;
    }

}
