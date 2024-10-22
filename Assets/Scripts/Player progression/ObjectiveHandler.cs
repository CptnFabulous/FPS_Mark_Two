using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
public class ObjectiveHandler : MonoBehaviour
{
    public struct LevelProgressData
    {
        public int progressIndex { get; private set; }
        public string data { get; private set; }
        public LevelProgressData[] subObjectives { get; private set; }

        public LevelProgressData(ObjectiveHandler handler)
        {
            throw new System.NotImplementedException();
        }

    }

    static ObjectiveHandler _instance;
    public static ObjectiveHandler current
    {
        get
        {
            if (_instance == null || _instance.gameObject.scene != SceneManager.GetActiveScene())
            {
                _instance = FindObjectOfType<ObjectiveHandler>();
            }
            return _instance;
        }
    }
    //public static System.Action<Objective> onObjectiveUpdated;

    public LevelCompletionScreen completionScreen;
    public string nextLevelName;

    List<Objective> _all;
    Player _p;
    bool levelCompleted;

    public List<Objective> allObjectives => _all ??= new List<Objective>(GetComponentsInChildren<Objective>());
    public List<Objective> activeObjectives => allObjectives.FindAll((o) => o.status != ObjectiveStatus.Inactive);
    public Player targetPlayer => _p ??= FindObjectOfType<Player>();


    private void Start()
    {
        levelCompleted = false;
        foreach (Objective o in allObjectives)
        {
            o.serialisedProgress = "";
        }
        completionScreen.gameObject.SetActive(false);
    }
    private void Update()
    {
        if (levelCompleted == false)
        {
            bool readyToComplete = RequiredObjectivesCompleted();
            if (readyToComplete)
            {
                levelCompleted = true;
                StartCoroutine(completionScreen.EndLevel());
            }
        }
    }

    bool RequiredObjectivesCompleted()
    {
        foreach (Objective o in allObjectives)
        {
            // Ignore optional objectives
            if (o.optional) continue;
            // Return false if a mandatory objective has yet to be completed
            if (o.status != ObjectiveStatus.Completed) return false;
        }
        return true;
    }
    /*
    public LevelProgressData GetLevelProgress() => new LevelProgressData(this);
    public void LoadData(LevelProgressData progress)
    {

    }
    */
}
