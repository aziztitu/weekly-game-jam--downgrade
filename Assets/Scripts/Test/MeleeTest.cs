using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeleeTest : MonoBehaviour
{
    Animator anim;

    public GameObject currentWeapon;
    Collider weaponCollider;

    public GameObject currentShield;
    public float shieldingAngle = 0;
    bool isShielding = false;
    
    public Vector3 attackerPosition;

    // Start is called before the first frame update
    void Start()
    {
        anim = GetComponent<Animator>();

        weaponCollider = currentWeapon.GetComponent<Collider>();
        weaponCollider.enabled = false;
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(attackerPosition, new Vector3(0.2f, 0.2f, 0.2f));
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            anim.SetTrigger("slash");
            weaponCollider.enabled = true;
        }

        if (Input.GetMouseButtonDown(1))
        {
            isShielding = true;
            anim.SetBool("isShielding", isShielding);
        }
        if (Input.GetMouseButtonUp(1))
        {
            isShielding = false;
            anim.SetBool("isShielding", isShielding);
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

        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (isShielding)
            {
                float enemyAttackAngle = Vector3.Angle(transform.forward, attackerPosition);

                if(enemyAttackAngle > shieldingAngle)
                {
                    Debug.Log("Shielded");
                }
                else
                {
                    Debug.Log("Not Shielded");
                }
            }
            else
            {
                Debug.Log("Not Shielded");
            }
        }
    }

    void DisableWeaponCollider()
    {
        weaponCollider.enabled = false;
    }
}
