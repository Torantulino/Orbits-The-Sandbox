using UnityEngine;
using System.Collections;
using System.IO;
using UnityEngine.UI;


//Author: jashan, Adapted by Torantula
public class HiResScreenShots : MonoBehaviour
{
    public int resWidth = 1920;
    public int resHeight = 1080;


    private Camera cam;
    private Camera bloom_cam;
    private bool takeHiResShot = false;

    private void Start()
    {
        cam = this.GetComponent<Camera>();
        //bloom_cam = transform.GetComponentInChildren<Camera>();
        bloom_cam = transform.GetChild(0).GetComponent<Camera>();
    }

    public static string ScreenShotName(int width, int height)
    {
        if (!Directory.Exists(Application.dataPath + "/../screenshots"))
        {
            Directory.CreateDirectory(Application.dataPath + "/../screenshots");
        }
        return string.Format("{0}/screenshots/screen_{1}x{2}_{3}.png",
            Application.dataPath + "/../",
            width, height,
            System.DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss"));
    }

    public void TakeHiResShot()
    {
        takeHiResShot = true;
    }

    void LateUpdate()
    {
        takeHiResShot |= Input.GetKeyDown(KeyCode.F12);
        if (takeHiResShot)
        {
            RenderTexture rt = new RenderTexture(resWidth, resHeight, 24);
            cam.targetTexture = rt;
            bloom_cam.targetTexture = rt;
            Texture2D screenShot = new Texture2D(resWidth, resHeight, TextureFormat.RGB24, false);
            cam.Render();
            bloom_cam.Render();
            RenderTexture.active = rt;
            screenShot.ReadPixels(new Rect(0, 0, resWidth, resHeight), 0, 0);
            cam.targetTexture = null;
            bloom_cam.targetTexture = null;
            RenderTexture.active = null; // JC: added to avoid errors
            Destroy(rt);
            byte[] bytes = screenShot.EncodeToPNG();
            string filename = ScreenShotName(resWidth, resHeight);
            System.IO.File.WriteAllBytes(filename, bytes);
            takeHiResShot = false;
        }
    }
}