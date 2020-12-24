﻿using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PauseMenu : MonoBehaviour
{
    public GameObject pauseMenu;

    private bool paused;

    // Start is called before the first frame update
    void Awake()
    {
        pauseMenu.SetActive(false);
    }

    private void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        //Checks to pause game
        if (Input.GetButtonDown("Pause"))
        {
            if (paused)
            {
                ResumeGame();
            }
            else
            {
                PauseGame();
            }
        }
    }

    public void Restart()
    {
        GameManager.Instance.RestartCurrentScene();
    }

    public void ResumeGame()
    {
        paused = false;
        Time.timeScale = 1f;

        pauseMenu.SetActive(false);

        HelperUtilities.UpdateCursorLock(true);
    }

    void PauseGame()
    {
        paused = true;
        Time.timeScale = 0f;

        pauseMenu.SetActive(true);
        HelperUtilities.UpdateCursorLock(false);
    }

    [UsedImplicitly]
    public void GoToMainMenu()
    {
        GameManager.Instance.GoToMainMenu();
    }
}