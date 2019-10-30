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
    private Image imgSpawnObj;
    private Color desiredTrailColor;
    private GameObject panObjects;
    private GameObject panEntities;
    private Transform contentEntites;
    private Button lastEntityBtnSelected;
    private bool uiLeftAnimating = false;
    private bool uiRightAnimating = false;
    private List<GameObject> selectedEntites = new List<GameObject>();
    private OrbitControls mainCamController;
    private Canvas canvas;
    private Texture2D cursor_default;
    private Texture2D cursor_hand;
    private Texture2D cursor_hand_click;
    private Texture2D cursor_drag;
    public bool displayFuturePath;
    public bool displayPastPath;
    private bool scalingTime;
    private int scalingTimeDirection;
    private float scalingTimeStart;
    private Dictionary<string, Tuple<Texture2D, Vector2>> cursors;
    InfiniteGrids placementGrid;
    GameObject tutorialCursor;
    public ParticleSystem tutorialParticleSystem;
    Animator tutorialCursorAnimator;
    bool tutorial = false;
    bool tutorialTrigger = false;
    float middleMouseHoldTime = 0.0f;
    float mouseScroll;

    void Awake()
    {
        manipMode = 1;
        symDivs = 5;
        spawnSymetry = false;
        orbVMultiplier = 0;

        physicsEngine = FindObjectOfType<PhysicsEngine>();

        panObjects = transform.Find("panObjects").gameObject;
        panEntities = transform.Find("panEntities").gameObject;
        contentEntites = panEntities.transform.Find("panel/Scroll View/Viewport/Content");
        tutorialCursor = GameObject.Find("tutorialCursor");
        tutorialCursorAnimator = tutorialCursor.GetComponent<Animator>();

        // Load cursors
        cursor_default = Resources.Load<Texture2D>("Textures/UI/cursors/cursor");
        if (cursor_default == null)
            Debug.LogError("Default cursor not found");
        cursor_hand = Resources.Load<Texture2D>("Textures/UI/cursors/hand");
        if (cursor_hand == null)
            Debug.LogError("Hand cursor not found");
        cursor_hand_click = Resources.Load<Texture2D>("Textures/UI/cursors/hand-click");
        if (cursor_hand_click == null)
            Debug.LogError("Hand click cursor not found");
        cursor_drag = Resources.Load<Texture2D>("Textures/UI/cursors/drag");
        if (cursor_drag == null)
            Debug.LogError("Drag cursor not found");

        cursors = new Dictionary<string, Tuple<Texture2D, Vector2>>{
            {
                "default", new Tuple<Texture2D, Vector2>(cursor_default, new Vector2(33.0f, 15.0f))
            },
            {
                "hand", new Tuple<Texture2D, Vector2>(cursor_hand, new Vector2(33.0f, 15.0f))
            },
            {
                "hand-click", new Tuple<Texture2D, Vector2>(cursor_hand_click, new Vector2(33.0f, 15.0f))
            },
            {
                "drag", new Tuple<Texture2D, Vector2>(cursor_drag, new Vector2(33.0f, 15.0f))
            }
        };
    }

    // Use this for initialization
    void Start()
    {
        mainCamController = Camera.main.GetComponent<OrbitControls>();
        if (mainCamController == null)
            Debug.Log("Main Cam Controller fot found by " + this.name + "!");

        //objectName = transform.Find("panObjects/panObject/TitleObj").GetComponent<Text>();
        playButton = transform.Find("panBottom/timescale/btnPlay").gameObject;
        pauseButton = transform.Find("panBottom/timescale/btnPause").gameObject;
        inptTime = transform.Find("panBottom/timescale/inptTime").GetComponent<InputField>();
        inptDivs = transform.Find("panObjects/panel/panSpawn/txtSym/inptDivs").GetComponent<InputField>();
        planetPanel = transform.Find("panObjects/panel/panLeft/panPlanets").gameObject;
        starPanel = transform.Find("panObjects/panel/panLeft/panStars").gameObject;
        othersPanel = transform.Find("panObjects/panel/panLeft/panOthers").gameObject;
        pausePanel = transform.Find("panPause").gameObject;
        // imgSpawnObj = transform.Find("panObjects/panBrush/imgSpawnObj").GetComponent<Image>();
        audioVT = GameObject.FindObjectOfType<AudioVisualTranslator>();
        // colPicker = GameObject.FindObjectOfType<CUIColorPicker>();
        activeObjectPanel = planetPanel;
        canvasGroup = transform.GetComponent<CanvasGroup>();
        placementGrid = FindObjectOfType<InfiniteGrids>();
        canvas = GetComponent<Canvas>();
        
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
        //Update timescale based on UI
        if (!inptTime.isFocused)
        {
            float roundedTime = (Time.timeScale / PhysicsEngine.TIMESCALER) * 100.0f;
            roundedTime = Mathf.Round(roundedTime) / 100.0f;
            inptTime.text = (roundedTime).ToString();
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
            foreach (GameObject _obj in selectedEntites)
            {
                Destroy(_obj);
            }
        }

        // Adjust timescale
        if(scalingTime)
        {
            float timeSpentScaling = Time.realtimeSinceStartup - scalingTimeStart;

            float ammount = 0.5f * timeSpentScaling;

            ammount *= scalingTimeDirection;
            
            physicsEngine.AddjustTimeScale(ammount);
        }


        // Track middle mouse hold time
        if (Input.GetMouseButton(2))
            middleMouseHoldTime += Time.unscaledDeltaTime;
        else
            middleMouseHoldTime = 0.0f;

        // Track mouse scrolling
        mouseScroll = Input.mouseScrollDelta.y; 
    }
    /// LateUpdate is called every frame, if the Behaviour is enabled.
    /// It is called after all Update functions have been called.
    void LateUpdate()
    {
        //Raycast under mouse
        RaycastHit raycastHit;
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        // If Ray hits physics object (Phsycis object under mouse).
        if (Physics.Raycast(ray, out raycastHit, 10000) && raycastHit.transform.gameObject.GetComponent<PhysicsObject>() != null)
        {
            // Object Click
            if(Input.GetMouseButton(0))
                SwitchCursor(2);
            // Pan
            else if (Input.GetMouseButton(2))
                SwitchCursor(3);
            // Hover
            else
                SwitchCursor(1);
        }
        // Else no physics object under mouse
        else
        {
            // Pan
            if (Input.GetMouseButton(2))
                SwitchCursor(3);
            // Default Cursor
            else
            SwitchCursor(0);

            // Select if no other object (such as UI) under mouse.
            if (Input.GetMouseButtonDown(0) && !EventSystem.current.IsPointerOverGameObject())
                SpawnObject();
        }
    }
    // Tutorial Coroutine
    IEnumerator Tutorial()
    {
        tutorialCursorAnimator.SetInteger("tutorialPhase", 1);

        // Un-Dissolve
        tutorialParticleSystem.transform.parent.gameObject.SetActive(true);
        ((Image)tutorialParticleSystem.GetComponentInParent<Image>()).fillAmount = 1.0f;
        tutorialParticleSystem.gameObject.SetActive(false);

        while (tutorial)
        {
            //Tutorial
            int phase = tutorialCursorAnimator.GetInteger("tutorialPhase");
            switch (phase)
            {
                // Teach: PAN
                case 1:
                    if (Input.GetMouseButton(2) && middleMouseHoldTime > 0.5f)
                    {
                        // Dissolve
                        ((Image)tutorialParticleSystem.GetComponentInParent<Image>()).fillAmount = 0.0f;
                        tutorialParticleSystem.gameObject.SetActive(true);

                        yield return new WaitForSecondsRealtime(1.0f);

                        tutorialCursorAnimator.SetInteger("tutorialPhase", phase + 1);

                        // Un-Dissolve
                        ((Image)tutorialParticleSystem.GetComponentInParent<Image>()).fillAmount = 1.0f;
                        tutorialParticleSystem.gameObject.SetActive(false);

                    }
                    break;
                // Teach: ZOOM OUT
                case 2:
                    if(mouseScroll < 0.0f)
                    {
                        // Dissolve
                        ((Image)tutorialParticleSystem.GetComponentInParent<Image>()).fillAmount = 0.0f;
                        tutorialParticleSystem.gameObject.SetActive(true);

                        yield return new WaitForSecondsRealtime(1.0f);

                        tutorialCursorAnimator.SetInteger("tutorialPhase", phase + 1);

                        // Un-Dissolve
                        ((Image)tutorialParticleSystem.GetComponentInParent<Image>()).fillAmount = 1.0f;
                        tutorialParticleSystem.gameObject.SetActive(false);
                    }
                    break;                
                // Teach: ZOOM IN
                case 3:
                    if(mouseScroll > 0.0f)
                    {
                        // Dissolve
                        ((Image)tutorialParticleSystem.GetComponentInParent<Image>()).fillAmount = 0.0f;
                        tutorialParticleSystem.gameObject.SetActive(true);

                        yield return new WaitForSecondsRealtime(1.0f);

                        tutorialCursorAnimator.SetInteger("tutorialPhase", phase + 1);

                        // Un-Dissolve
                        ((Image)tutorialParticleSystem.GetComponentInParent<Image>()).fillAmount = 1.0f;
                        tutorialParticleSystem.gameObject.SetActive(false);
                    }
                    break;
                // Teach: Open Left Panel
                case 4:
                    if(tutorialTrigger)
                    {
                        //Reset trigger
                        tutorialTrigger = false;

                        // Dissolve
                        ((Image)tutorialParticleSystem.GetComponentInParent<Image>()).fillAmount = 0.0f;
                        tutorialParticleSystem.gameObject.SetActive(true);

                        yield return new WaitForSecondsRealtime(1.0f);

                        tutorialCursorAnimator.SetInteger("tutorialPhase", phase + 1);

                        // Un-Dissolve
                        ((Image)tutorialParticleSystem.GetComponentInParent<Image>()).fillAmount = 1.0f;
                        tutorialParticleSystem.gameObject.SetActive(false);
                    }
                    break;
                // Teach: Spawn Object
                case 5:
                    if(tutorialTrigger)
                    {
                        //Reset trigger
                        tutorialTrigger = false;
                        
                        // Dissolve
                        ((Image)tutorialParticleSystem.GetComponentInParent<Image>()).fillAmount = 0.0f;
                        tutorialParticleSystem.gameObject.SetActive(true);

                        yield return new WaitForSecondsRealtime(1.0f);

                        tutorialCursorAnimator.SetInteger("tutorialPhase", phase + 1);

                        // Un-Dissolve
                        //((Image)tutorialParticleSystem.GetComponentInParent<Image>()).fillAmount = 1.0f;
                        //tutorialParticleSystem.gameObject.SetActive(false);
                    }
                    break;
                case 6:
                    StopTutorial();
                    transform.Find("panBottom/tutorial/btnStartTutorial").gameObject.SetActive(true);
                    transform.Find("panBottom/tutorial/btnStopTutorial").gameObject.SetActive(false);
                    break;
            }

            yield return new WaitForSecondsRealtime(0.2f);
        }

        yield return new WaitForEndOfFrame();
    }   
    // Initiates the Tutorial sequence
    public void StartTutorial()
    {
        tutorialCursorAnimator.SetInteger("tutorialPhase", 1);
        
        SwitchTab(0, 0);
        SwitchTab(1, 0);

        // Set tutorial panel to active
        tutorialCursor.transform.parent.gameObject.SetActive(true);
        tutorialCursor.SetActive(true);

        // Start
        tutorial = true;
        StartCoroutine(Tutorial());
    }
    public void StopTutorial()
    {
        tutorialCursorAnimator.SetInteger("tutorialPhase", -1);
        tutorialCursor.transform.parent.gameObject.SetActive(false);
        tutorial = false;
    }
    // Toggles the tutorial
    public void ToggleTuorial(bool start)
    {
        if(start)
            StartTutorial();
        else
            StopTutorial();
    }
    // Switches Cursor to the one specified and sets the appropriate hotspot
    public void SwitchCursor(uint i)
    {
        switch(i)
        {
            case 0:
                Cursor.SetCursor(cursors["default"].Item1, cursors["default"].Item2, CursorMode.Auto);
            break;
            case 1:
                Cursor.SetCursor(cursors["hand"].Item1, cursors["default"].Item2, CursorMode.Auto);
            break;
            case 2:
                Cursor.SetCursor(cursors["hand-click"].Item1, cursors["default"].Item2, CursorMode.Auto);
            break;
            case 3:
                Cursor.SetCursor(cursors["drag"].Item1, cursors["default"].Item2, CursorMode.Auto);
            break;        
        }
    }
    // Adds specified object to the Entities panel at right-hand side of screen.
    public void AddToEntitiesPanel(GameObject _obj)
    {
        GameObject sampleButton = contentEntites.Find("SampleButton").gameObject;
        GameObject newButton = Instantiate(sampleButton, contentEntites);

        newButton.GetComponentInChildren<Text>().text = _obj.name;
        newButton.name = _obj.name;
        newButton.SetActive(true);
    }
    // Removes specified object to the Entities panel at right-hand side of screen.
    public void RemoveFromEntitiesPanel(GameObject _obj)
    {
        GameObject toDestroy = contentEntites.Find(_obj.name).gameObject;
        Destroy(toDestroy);
    }
    // Selects and Highlights an entity - or group of entities - from the entites panel in response to user action.
    public void SelectEntityFromPanel(Button _btn)
    {
        // Clear Selected Entites
        selectedEntites.Clear();

        Button[] childButtons = contentEntites.GetComponentsInChildren<Button>(false);
        // Remove old highlighting
        foreach (Button button in childButtons)
        {
                ColorBlock normalColour = contentEntites.Find("SampleButton").GetComponent<Button>().colors;
                button.colors = normalColour;
        }


        if (_btn.name != "SampleButton")
        {
            ColorBlock redTint = _btn.colors;
            redTint.normalColor = new Color(0.00392f,  0.10196f,  0.12157f, 0.75f);
            redTint.selectedColor = new Color(0.00392f,  0.10196f,  0.12157f, 0.75f);

            // If shift button is down
            if ((Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift)) && lastEntityBtnSelected != null)
            {
                int _thisIndex = _btn.transform.GetSiblingIndex();
                int _lastIndex = lastEntityBtnSelected.transform.GetSiblingIndex();
                int _startIndex = Math.Min(_thisIndex, _lastIndex);
                int _endIndex = Math.Max(_thisIndex, _lastIndex);
                for (int i = _startIndex; i <= _endIndex; i++)
                {
                    contentEntites.GetChild(i).GetComponent<Button>().colors = redTint;
                    // Add to selected Entities
                    string _name = contentEntites.GetChild(i).GetComponentInChildren<Text>().text;
                    selectedEntites.Add(GameObject.Find(_name));
                }
            }
            else
            {
                _btn.colors = redTint;
                // Add to selected entites
                string _name = _btn.GetComponentInChildren<Text>().text;
                selectedEntites.Add(GameObject.Find(_name));
                // Focus camera on object
                SetSelectedObject(GameObject.Find(_btn.GetComponentInChildren<Text>().text).GetComponent<PhysicsObject>());
            }

            lastEntityBtnSelected = _btn;
        }
    }
    // Sets and updates the skybox
    public void SetSkybox(string name)
    {
        if (Skyboxes[name] != null)
            RenderSettings.skybox = Skyboxes[name];
    }
    // Changes the currently focused object to that specified
    public void SetSelectedObject(PhysicsObject obj)
    {
        selectedObject = obj;

        //Focus cameras
        mainCamController.SetFocalObject(obj.gameObject);
        mainCamController._Distance = Mathf.Max(obj.transform.localScale.z * 3.0f, 1.5f + obj.transform.localScale.z);
    }
    // Called when upon selection of object from entites panel
    public void SetObjectToSpawn(string name)
    {
        objectToSpawn = (GameObject)CelestialObjects[name];
    }
    // Reloads the currently loaded scene
    public void ReloadScene()
    {
        Application.LoadLevel(Application.loadedLevel);
    }
    // Toggles mute of the game audio
    public void ToggleMute()
    {
        MusicManagaer musicMan = FindObjectOfType<MusicManagaer>();
        musicMan.gameObject.GetComponentInChildren<AudioSource>().mute = 
            !musicMan.gameObject.GetComponentInChildren<AudioSource>().mute;
    }
    // Pauses simulation
    public void pausePressed()
    {
        physicsEngine.pauseSimulation();
        pauseButton.SetActive(false);
        playButton.SetActive(true);
    }
    // Resumes simulation
    public void playPressed()
    {
        physicsEngine.resumeSimulation();
        playButton.SetActive(false);
        pauseButton.SetActive(true);
    }
    // Time manupulation Click start
    public void TimeManipStart(int _direction)
    {
        if (Time.timeScale == 0)
            playPressed();

        scalingTime = true;
        scalingTimeStart = Time.realtimeSinceStartup;
        scalingTimeDirection = _direction;
    }
    // Time manupulation Click end
    public void TimeManipEnd()
    {
        scalingTime = false;
    }
    // Called from Timescale text input box to scale time
    public void timeScaled(string _scale)
    {
        
        try
        {
            int scale = int.Parse(_scale);

            if(Time.timeScale == 0 && scale != 0)
                playPressed();

            physicsEngine.ScaleTime(scale);
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
    // Called from Spawn Velocity slider when changed
    public void OrbVMultiplierChanged(float val)
    {
        orbVMultiplier = val;
    }
    // Called when Spawn Symmetry is toggled
    public void toggleSymetry(bool state)
    {
        spawnSymetry = state;
        inptDivs.interactable = state;
    }
    // Called when Spawn Symmetry No. Divisions input is changed
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
    // Called when Show Trails is toggled
    public void trailsToggled(bool state)
    {
        if (state)
            Camera.main.cullingMask = Camera.main.cullingMask | (1 << 8);
        else
            Camera.main.cullingMask = Camera.main.cullingMask & ~(1 << 8);
    }
    // Called when Show Grid is toggled
    public void gridToggled(bool state)
    {
        placementGrid.render = state;
    }
    // Called when Relative Paths (Future) is toggled
    public void futurePathToggled(bool state)
    {
        displayFuturePath = state;
    }
    // Called when Relative Paths (Past) is toggled
    public void pastPathToggled(bool state)
    {
        displayPastPath = state;
    }
    // NOT CURRENTLY IMPLEMENTED (Called when Spawn With Orbit is toggled)
    public void SpawnWithOrbitToggled(bool state)
    {
        foreach (Object item in CelestialObjects.Values)
        {
            ((GameObject)item).GetComponent<PhysicsObject>().spawnWithOrbit = state;
        }
    }
    // Called when Catagory (Stars, Planets, Moons) is selected in Object panel
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
    // Spawns the currently selected object at the current grid position under the mouse
    void SpawnObject()
    {
        if (objectToSpawn != null)
        {
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

            // Tutorial Trigger
            if (tutorial)
            {
                if (tutorialCursorAnimator.GetInteger("tutorialPhase") == 5)
                    tutorialTrigger = true;
            }
        }
    }
    // Toggles the visibility of the given UI panel
    // 0: Objects, 1: Entities
    // expand: -1 = ignore, 0 = false, 1 = true
    IEnumerator SwitchActivePanel(int val, int expand = -1)
    {   
        if(val == 0)
        {
            if (uiLeftAnimating)
                yield break;
            else
                uiLeftAnimating = true;
        }
        else if (val == 1)
        {
            if (uiRightAnimating)
                yield break;
            else
                uiRightAnimating = true;
        }
        
        GameObject parent = null;
        GameObject panel = null;
        GameObject maximiser = null;
        bool expanding;
        int side = 0;
        float dist = 304.0f * canvas.scaleFactor;

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

        //Check specified direction
        if(expand != -1)
        {
            if(expanding && expand == 0)
            {
                uiLeftAnimating = false;
                uiRightAnimating = false;
                yield break;
            }
            else if(!expanding && expand == 1)
            {
                uiLeftAnimating = false;
                uiRightAnimating = false;
                yield break;
            }
        }

        float spdx;
        float start_time;

        //##Contract##
        if (!expanding)
        {
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

        if(val == 0)
                uiLeftAnimating = false;
        else if (val == 1)
                uiRightAnimating = false;

        yield return new WaitForSeconds(0.0f);
    }
    // Called by unity when minimise/maximise panel UI button pressed
    // 0 Left, 1 Right
    public void SwitchTab(int val)
    {
        SwitchTab(val, -1);
    }
    public void SwitchTab(int val, int _expand)
    {
        StartCoroutine(SwitchActivePanel(val, _expand));

        //Tutorial Trigger
        if(tutorial)
        {
            if(tutorialCursorAnimator.GetInteger("tutorialPhase") == 4 && val == 0)
                tutorialTrigger = true;
        }
    }
    // Called when Settings and Back (from settings) buttons are called in pause menu
    public void ToggleSettings(bool state)
    {
        pausePanel.transform.Find("panSettings").gameObject.SetActive(state);
    }
    // Pauses the game, bringing up the pause menu
    public void PauseGame()
    {
        pausePanel.SetActive(true);
        physicsEngine.pauseSimulation();
    }
    // Resumes the game, hiding the pause menu
    public void ResumeGame()
    {
        pausePanel.SetActive(false);
        physicsEngine.resumeSimulation();
    }
    // Quits the game, called by Quit button in pause menu
    public void Quit()
    {
        Application.Quit();
    }
}