using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

public class PlayerStateHandler : MonoBehaviour
{
    public enum PlayerState
    {
        Active,
        Paused,
        InMenus,
        Dead,
        CompletedLevel,
    }

    public Player controlling;
    [SerializeField] PlayerState state = PlayerState.Active;

    [Header("Gameplay")]
    public Canvas headsUpDisplay;
    public string gameplayControls = "On foot";
    public UnityEvent onResume;

    [Header("Pausing")]
    public Canvas pauseMenu;
    public MenuWindow pauseWindow;
    public string menuControls = "UI";
    public Button resumeButton;
    public UnityEvent onPause;

    [Header("Extra menus")]
    public Canvas sideMenu;
    public UnityEvent onEnterSideMenu;

    [Header("Game over")]
    public Canvas gameOverMenu;
    public UnityEvent onDeath;
    
    public PlayerState CurrentState
    {
        get => state;
        set
        {
            state = value;
            switch (value)
            {
                case PlayerState.Paused:
                    onPause.Invoke();
                    navigatingMenus = true;
                    SwitchMenu(pauseMenu);

                    break;
                case PlayerState.Dead:

                    Debug.Log("Player has died");
                    onDeath.Invoke();
                    navigatingMenus = true;
                    Time.timeScale = 1;
                    SwitchMenu(gameOverMenu);

                    break;
                case PlayerState.Active: // Resume game

                    Debug.Log("Resuming game");
                    onResume.Invoke();
                    SwitchMenu(headsUpDisplay);
                    navigatingMenus = false;

                    break;
                    
                case PlayerState.InMenus:
                    navigatingMenus = true;
                    SwitchMenu(sideMenu);
                    onEnterSideMenu.Invoke();
                    break;
                    
                /*
                case PlayerState.CompletedLevel:
                    Time.timeScale = 1;
                    EnterMenu();
                    break;
                    */
            }
        }
    }
    public bool navigatingMenus
    {
        get => controlling.controls.currentActionMap.name == menuControls;
        set
        {
            Time.timeScale = value ? 0 : 1;
            Cursor.lockState = value ? CursorLockMode.None : CursorLockMode.Locked;
            Cursor.visible = value;
            controlling.controls.SwitchCurrentActionMap(value ? menuControls : gameplayControls);
            headsUpDisplay.gameObject.SetActive(!value);
        }
    }

    // Start is called before the first frame update
    void Awake()
    {
        resumeButton.onClick.AddListener(() => CurrentState = PlayerState.Active);
        CurrentState = CurrentState; // Assigns value to itself to trigger the appropriate code
    }

    /// <summary>
    /// The InputSystem function for pausing and unpausing the game.
    /// </summary>
    void OnPause()
    {
        Debug.Log("Toggling pause menu");
        switch (CurrentState)
        {
            case PlayerState.Active: CurrentState = PlayerState.Paused; break;
            case PlayerState.Paused: CurrentState = PlayerState.Active; break;
        }
    }
    /// <summary>
    /// The InputSystem function for entering the game's extra menus.
    /// </summary>
    void OnEnterMenu()
    {
        Debug.Log("Toggling side menu");
        switch (CurrentState)
        {
            case PlayerState.Active: CurrentState = PlayerState.InMenus; break;
            case PlayerState.InMenus: CurrentState = PlayerState.Active; break;
        }
    }
    void OnCancel()
    {
        Debug.Log("Attempting exiting menu");
        switch (CurrentState)
        {
            case PlayerState.Paused:

                // TO DO: fix this, it can sometimes work even if not at the root menu
                if (pauseWindow.root.menuIsActive && pauseWindow.parents.Length <= 0)
                {
                    CurrentState = PlayerState.Active;
                }


                break;
            case PlayerState.InMenus: CurrentState = PlayerState.Active; break;
        }
    }

    void SwitchMenu(Canvas currentMenu)
    {
        headsUpDisplay.gameObject.SetActive(false);
        pauseMenu.gameObject.SetActive(false);
        sideMenu.gameObject.SetActive(false);
        gameOverMenu.gameObject.SetActive(false);

        currentMenu.gameObject.SetActive(true);
    }
}
