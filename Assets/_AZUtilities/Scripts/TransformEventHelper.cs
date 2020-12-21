using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TransformEventHelper : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void MoveTo(Transform other)
    {
        transform.position = other.position;
        transform.rotation = other.rotation;
    }
}
