using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(Canvas), typeof(CanvasGroup))]
public class MenuWindow : MonoBehaviour
{
    public string description = "A menu.";
    
    [Header("Additional menu elements")]
    public Text title;
    public Button back;
    public Image selectedGraphic;
    public Text selectionDescription;

    Canvas canvas;
    CanvasGroup visualElements;
    RectTransform rt;
    EventSystem eventSystem;

    
    MenuWindow[] parents;
    MenuWindow immediateParent;
    MenuWindow root;
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
        canvas = GetComponent<Canvas>();
        visualElements = GetComponent<CanvasGroup>();
        visualElements.ignoreParentGroups = true;
        rt = GetComponent<RectTransform>();
        eventSystem = GetComponent<EventSystem>();
        Debug.Log(back + ", " + name);

        if (back != null)
        {
            back.onClick.AddListener(ReturnToParentMenu);
        }
    }

    private void Start()
    {
        // Assign children
        List<MenuWindow> childMenus = new List<MenuWindow>(GetComponentsInChildren<MenuWindow>(true));
        childMenus.Remove(this);
        children = childMenus.ToArray();


        // Assign parents
        List<MenuWindow> parentMenus = new List<MenuWindow>(GetComponentsInParent<MenuWindow>(true));
        parentMenus.Remove(this);
        parents = parentMenus.ToArray();
        immediateParent = null;
        if (transform.parent != null)
        {
            immediateParent = transform.parent.GetComponentInParent<MenuWindow>();
        }

        // If no parent could be found, this object is the root
        if (immediateParent == null)
        {
            root = this;
            for(int i = 0; i < children.Length; i++)
            {
                children[i].root = this;
            }

            if (back != null)
            {
                back.gameObject.SetActive(false); // Disable the back button if there's no parent to return to
            }
            
        }

        ReturnToRoot();
    }


    public void SwitchWindow(MenuWindow newWindow)
    {
        Debug.Log(newWindow + ", " + name);
        // Disable all windows except for the current one and its parents
        // The root is not part of this specific for loop but that doesn't matter, it needs to be active in order for itself or any of its children to be active
        Debug.Log(root.children);
        for (int i = 0; i < root.children.Length; i++)
        {
            root.children[i].gameObject.SetActive(false);
        }

        // Enable new window and parents, using canvas group to hide parents
        Debug.Log(newWindow.parents);
        newWindow.gameObject.SetActive(true);
        newWindow.visualElements.alpha = 1;
        for (int i = 0; i < newWindow.parents.Length; i++)
        {
            newWindow.parents[i].gameObject.SetActive(true);
            newWindow.parents[i].visualElements.alpha = 0;
        }
    }
    public void ReturnToParentMenu()
    {
        SwitchWindow(immediateParent);
    }
    public void ReturnToRoot()
    {
        SwitchWindow(root);
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
