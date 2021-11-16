using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class MenuWindow : MonoBehaviour
{
    public string description = "A menu.";
    
    [Header("Additional menu elements")]
    public Text title;
    public Button back;
    public Image selectedGraphic;
    public Text selectionDescription;
    
    Canvas canvas;
    EventSystem eventSystem;

    MenuWindow root;
    MenuWindow parent;
    MenuWindow[] children;

    private void OnValidate()
    {
        // Finds the first text object and designates it as its child.
        // Auto-updates the title text object to match the object's name, for easy editing.
        title = GetComponentInChildren<Text>();
        title.text = name;
    }
    /*
    private void Awake()
    {
        StandaloneInputModule s;
        s.se
    }
    */




    public void SwitchWindow(MenuWindow newWindow)
    {

    }


    void RefreshSelectionInfo()
    {
        MenuInteractable current = eventSystem.currentSelectedGameObject.GetComponent<MenuInteractable>();
        if (current != null)
        {
            selectedGraphic.sprite = current.graphic;
            selectionDescription.text = current.description;
        }
    }



    public void ReturnToMainMenu()
    {
        LoadScene(mainMenu);
    }
    public static readonly string mainMenu = "Main Menu";

    public void QuitGame()
    {
        Application.Quit();
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #endif
    }


    public void LoadScene(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
        //StartCoroutine(LoadingScreen.LoadSequence(sceneName));
    }

    
}
