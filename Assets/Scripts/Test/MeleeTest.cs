using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeleeTest : MonoBehaviour
{
    Animator anim;

    // Start is called before the first frame update
    void Start()
    {
        anim = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.W))
        {
            anim.SetTrigger("slash");
        }

        if (Input.GetKeyDown(KeyCode.S))
        {
            anim.SetTrigger("shieldBlock");
        }

        if (Input.GetKeyDown(KeyCode.Q))
        {
            anim.SetTrigger("left");
        }

        if (Input.GetKeyDown(KeyCode.E))
        {
            anim.SetTrigger("right");
        }

        if (Input.GetKeyDown(KeyCode.D))
        {
            anim.SetTrigger("parryKick");
        }
    }
}
