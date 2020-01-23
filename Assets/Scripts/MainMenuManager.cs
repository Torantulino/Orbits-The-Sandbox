using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuManager : MonoBehaviour
{
    public void StartGame()
    {
        StartCoroutine("StartGameWaiter");
    }
     
    IEnumerator StartGameWaiter()
    {
        yield return new WaitForSeconds(1.1f);
        SceneManager.LoadScene("Realism");
        yield return new WaitForSeconds(0.0f);
    }

    public void Settings()
    {

    }

    public void Quit()
    {
        Application.Quit();
    }
    
}
