using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class LevelCompletionScreen : MonoBehaviour
{
    [SerializeField] Animator animator;
    [SerializeField] string triggerName = "Level Completed";
    [SerializeField] float delayBeforeTakingControl = 0;

    [Header("Level info")]
    [SerializeField] TMP_Text levelName;

    [Header("Bonus Objectives")]
    //public ObjectiveInfo bonusObjectives;
    [SerializeField] TMP_Text remainingObjectives;
    [SerializeField] string remainingObjectivesFormat = "{0} remaining";
    [SerializeField] string allCompletedMessage = "All completed!";

    [Header("Proceeding")]
    [SerializeField] Button nextLevelButton;
    [SerializeField] Button quitToMenuButton;

    string nextLevelName;

    bool nextLevelExists => string.IsNullOrEmpty(nextLevelName) == false;

    private void Awake()
    {
        nextLevelButton.onClick.AddListener(GoToNextLevel);
        quitToMenuButton.onClick.AddListener(QuitToMenu);
    }

    public IEnumerator EndLevel()
    {
        // Display stats
        ObjectiveHandler objectives = ObjectiveHandler.current;
        DisplayInfo(objectives);
        // Activate gameObject and trigger its animation
        gameObject.SetActive(true);
        animator.SetTrigger(triggerName);
        // Wait until the desired time and disable player inputs
        yield return new WaitForSeconds(delayBeforeTakingControl);
        objectives.targetPlayer.stateHandler.navigatingMenus = true;
    }

    void DisplayInfo(ObjectiveHandler objectives)
    {
        // Display name of level
        levelName.text = objectives.gameObject.scene.name;

        #region Display bonus objectives (and which ones were completed)

        List<Objective> optionalObjectives = new List<Objective>(objectives.allObjectives);
        optionalObjectives.RemoveAll((o) => o.optional == false);
        int remaining = optionalObjectives.Count;
        optionalObjectives.RemoveAll((o) => o.status != ObjectiveStatus.Completed);
        remaining -= optionalObjectives.Count; // Subtract to get the number of bonus objectives the player didn't complete.

        string objectiveText = "";
        foreach (Objective o in optionalObjectives)
        {
            objectiveText += o.name;
            objectiveText += '\n';
        }
        // Populate with the 'X remaining' message (if there are some remaining), or the 'all completed' message
        objectiveText += (remaining > 0) ? string.Format(remainingObjectivesFormat, remaining) : allCompletedMessage;

        remainingObjectives.text = objectiveText;

        #endregion

        // Assign next level
        // Disable 'next level' button interactability if there isn't one
        nextLevelName = objectives.nextLevelName;
        //nextLevelButton.interactable = nextLevelExists;

        //UnityEngine.EventSystems.EventSystem.current.a
    }

    void GoToNextLevel()
    {
        if (nextLevelExists == false)
        {
            QuitToMenu();
            return;
        }
        
        //if (nextLevelExists == false) return;

        // Save game

        LoadingScreen.LoadScene(nextLevelName); // Load next level
    }
    void QuitToMenu()
    {
        // Save game
        
        LoadingScreen.ReturnToMainMenu(); // Load menu
    }
}
