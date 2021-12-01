using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneController : MonoBehaviour
{
    //test
    public void LoadMenu()
    {
        Time.timeScale = 1;
        SceneManager.LoadScene("MainMenu");
    }

    public void LoadGame()
    {
        Time.timeScale = 1;
        SceneManager.LoadScene("TurnBasedGame");
    }

    public void ExitApplication()
    {
        Application.Quit();
    }
}
