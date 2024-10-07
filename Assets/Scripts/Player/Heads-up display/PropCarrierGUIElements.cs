using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PropCarrierGUIElements : MonoBehaviour
{
    public PropCarryingHandler propCarrier;
    public RectTransform throwGUI;

    private void Awake()
    {
        // When item is picked up, enable button prompts
        propCarrier.onPickup.AddListener((_) => throwGUI.gameObject.SetActive(true));
        // When item is dropped, disable button prompts
        propCarrier.onDrop.AddListener((_) => throwGUI.gameObject.SetActive(false));

        throwGUI.gameObject.SetActive(false);
    }
}