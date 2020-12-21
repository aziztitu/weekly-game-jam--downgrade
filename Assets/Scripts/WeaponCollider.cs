using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponCollider : MonoBehaviour
{
    Collider weaponCollider;

    void Awake()
    {
        weaponCollider = GetComponent<Collider>();
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.transform.tag == "Enemy")
        {
            Debug.Log("OnAttack Function Called"); // TODO (Gavin): Add OnAttack function
            weaponCollider.enabled = false;
        }
    }
}
