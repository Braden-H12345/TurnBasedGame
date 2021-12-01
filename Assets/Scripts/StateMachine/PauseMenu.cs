using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PauseMenu : MonoBehaviour
{
    [SerializeField] GameObject _pausePanel;



    public void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Pause();
        }
    }
    public void Pause()
    {
        _pausePanel.SetActive(true);
        Time.timeScale = 0;
    }
    public void Unpause()
    {
        Time.timeScale = 1;
        _pausePanel.SetActive(false);
    }
}
