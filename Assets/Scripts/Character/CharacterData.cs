using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Character", menuName = "Data/Character")]
public class CharacterData : ScriptableObject
{
    public string name;
    public GameObject prefab;
}