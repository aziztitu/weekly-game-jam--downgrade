using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Weapon : MonoBehaviour
{
    public int weaponId;
    public float damage = 10;

    public Collider weaponCollider { get; private set; }
    private CharacterModel owner;

    void Awake()
    {
        weaponCollider = GetComponentInChildren<Collider>();
        owner = GetComponentInParent<CharacterModel>();
    }

    void OnTriggerEnter(Collider other)
    {
        if (!owner.characterMeleeController.isAttackSequenceActive)
        {
            return;
        }

        if (other.transform.tag == "Character")
        {
            CharacterModel otherCharacterModel = other.GetComponent<CharacterModel>();
            if (otherCharacterModel != owner)
            {
                otherCharacterModel.characterMeleeController.OnIncomingAttack(owner,
                    damage * owner.characterMeleeController.curDamageMultiplier, out var _);
                weaponCollider.enabled = false;
            }
        }
    }
}