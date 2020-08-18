using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlanetGenerator : MonoBehaviour
{
    [Header("Base Settings")]
    public float Radius;
    public int IcosphereSubdivisions;
    public Material PlanetMaterial;
    public bool SmoothNormals;
    public bool Rotate;
    public float TurnSpeed;
    public List<ColorSetting> Colours = new List<ColorSetting>();

    [Header("Ocean:")]
    public bool DrawShore;
    public float minShoreWidth;
    public float maxShoreWidth;

    [Header("Continents:")]
    public int MaxAmountOfContinents;
    public float ContinentsMinSize;
    public float ContinentsMaxSize;
    public float MinLandExtrusionHeight;
    public float MaxLandExtrusionHeight;

    [Header("Mountains:")]
    public float MaxAmountOfMountains;
    public float MountainBaseSize;
    public float MinMountainHeight;
    public float MaxMountainHeight;

    [Header("Bumpiness:")]
    public float MinBumpFactor;
    public float MaxBumpFactor;

    private GameObject planetGameObject;
    private MeshRenderer meshRenderer;
    private MeshFilter meshFilter;

    public void GeneratePlanet()
    {
        if(!planetGameObject)
            planetGameObject = this.gameObject;

        if(!meshRenderer)
        {
            meshRenderer = gameObject.AddComponent<MeshRenderer>();
            meshRenderer.material = PlanetMaterial;
        }

        if(!meshFilter)
            meshFilter = gameObject.AddComponent<MeshFilter>();

        planetMesh = new Mesh();
        GenerateIcosphere();
        SetMesh();
    }
}
