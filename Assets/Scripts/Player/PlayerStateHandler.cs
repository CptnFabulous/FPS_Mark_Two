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
    public UnityEvent onResume;
    public Canvas pauseMenu;
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
    void Start()
    {
        CurrentState = CurrentState; // Assigns value to itself to trigger the appropriate code
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetButtonDown("Pause")) // Toggle between paused and unpaused
        {
            TogglePause();
        }
    }

    public void TogglePause()
    {
        switch (CurrentState)
        {
            case PlayerState.Active: // pause game
                CurrentState = PlayerState.Paused;
                break;
            case PlayerState.Paused: // resume game
                CurrentState = PlayerState.Active;
                break;
            case PlayerState.InMenus: // exit out of menu
                CurrentState = PlayerState.Active;
                break;
        }
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
    }
    void ReturnToGameplay()
    {
        Debug.Log("Returning to gameplay");
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        Time.timeScale = 1;
    }


    /*
    public void Die()
    {
        
    }
    */
}
