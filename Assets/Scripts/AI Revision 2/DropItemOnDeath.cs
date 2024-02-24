using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// A simple script for hiding an object on an enemy model and spawning an identical replacement.
/// This is used because enemy guns have a lot of info on them that's unnecessary as an item drop, and are overall kind of jankily designed.
/// So this script may become redundant in future if I refactor the weapons.
/// </summary>
public class DropItemOnDeath : MonoBehaviour
{
    public GameObject originalToHide;
    public Rigidbody itemToSpawn;

    private void Awake()
    {
        Character c = GetComponentInParent<Character>();
        c.health.onDeath.AddListener((_) => TriggerItemDrop());
    }
    void TriggerItemDrop()
    {
        Transform item = Instantiate(itemToSpawn, null).transform;
        item.position = originalToHide.transform.position;
        item.rotation = originalToHide.transform.rotation;
        originalToHide.gameObject.SetActive(false);
    }
}
