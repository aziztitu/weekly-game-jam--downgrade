using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundEffectsManager : SingletonMonoBehaviour<SoundEffectsManager>
{
    public AudioSource audioSource;

    public List<AudioClip> soundEffects;

    private readonly Dictionary<string, AudioClip> soundEffectsDict = new Dictionary<string, AudioClip>();

    new void Awake()
    {
        base.Awake();

        soundEffectsDict.Clear();
        foreach (var clip in soundEffects)
        {
            soundEffectsDict[clip.name] = clip;
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

    public void Play(string key)
    {
        if (soundEffectsDict.ContainsKey(key))
        {
            Play(soundEffectsDict[key]);
        }
    }

    public void Play(AudioClip audioClip)
    {
        if (audioClip == null)
        {
            return;
        }

        audioSource.spatialBlend = 0f;
        audioSource.PlayOneShot(audioClip);
    }

    public void PlayAt(AudioClip audioClip, Vector3 position)
    {
        if (audioClip == null)
        {
            return;
        }

        audioSource.spatialBlend = 1f;
        audioSource.transform.position = position;
        audioSource.PlayOneShot(audioClip);
    }
}