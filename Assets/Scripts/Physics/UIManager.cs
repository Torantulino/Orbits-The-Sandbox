using System.Collections;
using System.Collections.Generic;
using System.Runtime.Remoting.Messaging;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{

    public InputField inptMassVal;
    public InputField inptRadiusVal;
    public InputField inptDensityVal;
    public bool editingMass;
    public bool editingRadius;
    public bool editingDensity;

    private PhysicsObject selectedObject;

    public void SetSelectedObject(PhysicsObject obj)
    {
        selectedObject = obj;
    }

	// Use this for initialization
	void Start ()
	{
	    editingMass = false;
	    editingDensity = false;
	    editingRadius = false;
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
