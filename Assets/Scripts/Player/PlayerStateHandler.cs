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
    }

    public Player controlling;
    [SerializeField] PlayerState state = PlayerState.Active;

    [Header("Menus")]
    public Canvas headsUpDisplay;
    public string gameplayControls = "On foot";
    public UnityEvent onResume;
    public Canvas pauseMenu;
    public string menuControls = "UI";
    public Button resumeButton;
    public UnityEvent onPause;
    public Canvas gameOverMenu;
    public UnityEvent onDeath;
    
    public PlayerState CurrentState
    {
        get
        {
            return state;
        }
        set
        {
            state = value;
            switch (value)
            {
                case PlayerState.Paused:

                    onPause.Invoke();
                    Time.timeScale = 0;
                    SwitchMenu(pauseMenu);
                    EnterMenu();

                    break;
                case PlayerState.Dead:

                    onDeath.Invoke();
                    Time.timeScale = 1;
                    SwitchMenu(gameOverMenu);
                    EnterMenu();

                    break;
                default: // Resume game

                    onResume.Invoke();
                    SwitchMenu(headsUpDisplay);
                    ReturnToGameplay();

                    break;
            }
        }
    }

    // Start is called before the first frame update
    void Awake()
    {
        resumeButton.onClick.AddListener(() => CurrentState = PlayerState.Active);
        CurrentState = CurrentState; // Assigns value to itself to trigger the appropriate code
    }

    void OnPause()
    {
        if (CurrentState == PlayerState.Active)
        {
            CurrentState = PlayerState.Paused;
        }
    }
    void OnEnterMenu()
    {

    }

    void SwitchMenu(Canvas currentMenu)
    {
        headsUpDisplay.gameObject.SetActive(false);
        pauseMenu.gameObject.SetActive(false);
        gameOverMenu.gameObject.SetActive(false);

        currentMenu.gameObject.SetActive(true);
    }
    void EnterMenu()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        controlling.controls.SwitchCurrentActionMap(menuControls);
    }
    void ReturnToGameplay()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        Time.timeScale = 1;
        controlling.controls.SwitchCurrentActionMap(gameplayControls);
    }
}
