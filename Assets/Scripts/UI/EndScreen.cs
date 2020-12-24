using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;

public class EndScreen : SingletonMonoBehaviour<EndScreen>
{
    public GameObject root;

    public AudioClip gameOverClip;

    new void Awake()
    {
        base.Awake();
        //root.DOFade(0, 0).Play();
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
    }

    void ShowEndScreen()
    {
        SoundEffectsManager.Instance.Play(gameOverClip);

        Time.timeScale = 0f;
        HelperUtilities.UpdateCursorLock(false);

        root.SetActive(true);
        // root.DOFade(1, transitionDuration).SetUpdate(true).Play();
    }

    public void Restart()
    {
        GameManager.Instance.RestartCurrentScene();
    }

    public void MainMenu()
    {
        GameManager.Instance.GoToMainMenu();
    }
}