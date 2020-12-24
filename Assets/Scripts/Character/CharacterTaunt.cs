using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterTaunt : MonoBehaviour
{
    public Animator anim;

    void Start()
    {
        int rand = Random.Range(0, 4);

        switch (rand)
        {
            case 0:
                anim.SetTrigger("WA");
                break;
            case 1:
                anim.SetTrigger("Rob");
                break;
            case 2:
                anim.SetTrigger("BP");
                break;
            case 3:
                anim.SetTrigger("1990");
                break;
        }
    }
}
