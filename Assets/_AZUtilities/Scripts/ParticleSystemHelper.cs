using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class ParticleSystemHelper : MonoBehaviour
{
    [Serializable]
    public class GameObjectUnityEvent : UnityEvent<GameObject> { }

    public GameObjectUnityEvent onParticleCollisionEvent;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void OnParticleCollision(GameObject other)
    {
        onParticleCollisionEvent?.Invoke(other);
    }
}
