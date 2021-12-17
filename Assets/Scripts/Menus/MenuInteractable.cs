using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class MenuInteractable : MonoBehaviour, ISelectHandler, IPointerEnterHandler
{
    public Text title;
    public Sprite graphic;
    [Multiline] public string description = "This is a selectable option.";

    MenuWindow menu;

    private void OnValidate()
    {
        // Finds the first text object and designates it as its child.
        // Auto-updates the title text object to match the object's name, for easy editing.
        title = GetComponentInChildren<Text>();
        title.text = name;
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
        if (menu == null)
        {
            menu = GetComponentInParent<MenuWindow>();
            if (menu == null)
            {
                Debug.Log(name + ", menu not found for some reason");
                return;
            }
        }
        
        
        if (menu.selectedGraphic != null)
        {
            menu.selectedGraphic.sprite = graphic;
            menu.selectedGraphic.enabled = graphic != null;
        }
        if (menu.selectionDescription != null)
        {
            menu.selectionDescription.text = description;
        }
    }




    #region Miscellaneous menu functions

    public void QuitGame()
    {
        Application.Quit();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }


    #endregion
}
