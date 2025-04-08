using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ThrowGUI : MonoBehaviour
{
    public ThrowHandler throwHandler;
    public RectTransform throwGUI;

    private void Awake()
    {
        // When item is picked up, enable button prompts
        throwHandler.onPickup.AddListener((_) => throwGUI.gameObject.SetActive(true));
        // When item is dropped, disable button prompts
        throwHandler.onDrop.AddListener((_) => throwGUI.gameObject.SetActive(false));

        throwGUI.gameObject.SetActive(false);
    }
}