using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BackgroundMusic : SingletonMonoBehaviour<BackgroundMusic>
{
    public AudioSource audioSource { get; private set; }

    new void Awake()
    {
        base.Awake();

        audioSource = GetComponent<AudioSource>();

        if (Instance == this)
        {
            transform.parent = null;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            if (Instance.audioSource.clip != audioSource.clip)
            {
                Instance.PlayMusic(audioSource.clip);
            }

            audioSource.Stop();
            Destroy(gameObject);
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void PlayMusic(AudioClip clip)
    {
        audioSource.Stop();

        audioSource.clip = clip;
        audioSource.time = 0;

        audioSource.Play();
    }
}
