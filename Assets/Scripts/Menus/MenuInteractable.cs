using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class MenuInteractable : MonoBehaviour//, ISelectHandler
{
    public Text title;
    public Sprite graphic;
    public string description = "This is a selectable option.";
    /*
    public void OnSelect(BaseEventData eventData)
    {
        throw new System.NotImplementedException();
    }
    */
    private void OnValidate()
    {
        // Finds the first text object and designates it as its child.
        // Auto-updates the title text object to match the object's name, for easy editing.
        title = GetComponentInChildren<Text>();
        title.text = name;
    }
}
