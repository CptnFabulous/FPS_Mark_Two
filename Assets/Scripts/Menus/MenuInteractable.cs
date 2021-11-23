using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class MenuInteractable : MonoBehaviour, ISelectHandler, IPointerEnterHandler
{
    public Text title;
    public Sprite graphic;
    public string description = "This is a selectable option.";

    MenuWindow menu;


    private void OnValidate()
    {
        // Finds the first text object and designates it as its child.
        // Auto-updates the title text object to match the object's name, for easy editing.
        title = GetComponentInChildren<Text>();
        title.text = name;
    }



    void Awake()
    {
        menu = GetComponentInParent<MenuWindow>();
    }


    public void OnSelect(BaseEventData eventData)
    {
        RefreshSelectionInfo();
    }
    public void OnPointerEnter(PointerEventData eventData)
    {
        RefreshSelectionInfo();
    }

    void RefreshSelectionInfo()
    {
        if (menu.selectedGraphic != null)
        {
            menu.selectedGraphic.sprite = graphic;
        }
        if (menu.selectionDescription != null)
        {
            menu.selectionDescription.text = description;
        }
    }
}
