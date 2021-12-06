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
    public Selectable firstSelection;
    public Button back;
    public Image selectedGraphic;
    public Text selectionDescription;

    Canvas canvas;
    CanvasGroup visualElements;
    RectTransform rt;
    
    MenuWindow[] parents;
    MenuWindow immediateParent;
    MenuWindow root;
    MenuWindow[] children;

    private void OnValidate()
    {
        if (firstSelection == null)
        {
            Debug.LogError("No default selection is present for " + name + " and the Event System won't know what to select first! Assign something to 'firstSelection'!");
        }
        if (back == null)
        {
            Debug.LogWarning("Button 'back' is null, so " + name + " cannot assign a listener for returning to the previous menu. Have you added one manually, or ensured the player does not need to return to its parent menu?");
        }
    }
    private void Awake()
    {
        canvas = GetComponent<Canvas>();
        visualElements = GetComponent<CanvasGroup>();
        visualElements.ignoreParentGroups = true;
        rt = GetComponent<RectTransform>();

        if (back != null)
        {
            back.onClick.AddListener(ReturnToParentMenu);
        }

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
            for (int i = 0; i < children.Length; i++)
            {
                children[i].root = this;
            }

            if (back != null)
            {
                back.gameObject.SetActive(false); // Disable the back button if there's no parent to return to
            }
        }
    }
    private void Start()
    {
        if (root == this)
        {
            SwitchWindow(this);
        }
    }

    /// <summary>
    /// Enables or disables a menu window in a way that doesn't interfere with the functioning of any child menus.
    /// </summary>
    /// <param name="active"></param>
    public void SetActiveState(bool active)
    {
        gameObject.SetActive(true);
        visualElements.interactable = active; // Objects in hidden menus are disabled so they aren't picked up by the event system
        visualElements.blocksRaycasts = active; // Objects in hidden menus are disabled so they don't block the player from clicking buttons in the current menu
        visualElements.alpha = active ? 1 : 0; // Alpha is adjusted to show visibility. If I disable the gameobject or canvas component it will hide children as well
    }
    /// <summary>
    /// Switches to a new window. Assign this as a button listener for menu transitions.
    /// </summary>
    /// <param name="newWindow"></param>
    public void SwitchWindow(MenuWindow newWindow)
    {
        //Debug.Log(newWindow);
        // Disable all windows except for the current one and its parents
        // The root is not part of this specific for loop but that doesn't matter, it needs to be active in order for itself or any of its children to be active
        for (int i = 0; i < root.children.Length; i++)
        {
            root.children[i].SetActiveState(false);
        }
        /*
        // Enable new window and parents, using canvas group to hide parents
        newWindow.SetActiveState(true);
        */
        for (int i = 0; i < newWindow.parents.Length; i++)
        {
            newWindow.parents[i].SetActiveState(false);
        }

        // Enable new window and parents, using canvas group to hide parents
        newWindow.SetActiveState(true);


        // Switch EventSystem so player automatically selects the first selectable
        Debug.Log("Assigning " + newWindow.firstSelection + " to event system");
        EventSystem.current.SetSelectedGameObject(newWindow.firstSelection.gameObject);
    }
    public void ReturnToParentMenu()
    {
        SwitchWindow(immediateParent);
    }
    public void ReturnToRoot()
    {
        SwitchWindow(root);
    }


    public void ReloadCurrentScene()
    {
        string currentSceneName = SceneManager.GetActiveScene().name;
        LoadScene(currentSceneName);
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
