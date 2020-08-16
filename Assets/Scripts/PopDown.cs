using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PopDown : MonoBehaviour
{
    public static int ScreenHeight = 1080;
    RectTransform rt;
    Vector3 downPos;
    Vector3 upPos;
    bool isDown;

    void Start()
    {
        //get the recttransform of the sliding panel 
        rt = GetComponent<RectTransform>();
        downPos = rt.localPosition;
        upPos = downPos + new Vector3(0, PopDown.ScreenHeight, 0);
        SetUp();
    }

    public void SetDown()
    {
        rt.localPosition = downPos;
    }

    public void SetUp()
    {
        rt.localPosition = upPos;
    }
}