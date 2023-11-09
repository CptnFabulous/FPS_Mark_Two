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
    
    
    
    

    List<Objective> _all;
    Player _p;

    public List<Objective> allObjectives => _all ??= new List<Objective>(GetComponentsInChildren<Objective>());
    public List<Objective> activeObjectives => allObjectives.FindAll((o) => o.status != ObjectiveStatus.Inactive);
    public Player targetPlayer => _p ??= FindObjectOfType<Player>();


    private void Awake()
    {
        foreach (Objective o in allObjectives)
        {
            o.serialisedProgress = "";
        }
    }
    private void Update()
    {
        /*
        TO DO: check if all mandatory objectives have been completed.
        If so, run the necessary end-of-level actions e.g.:
        * Open 'level complete' menu
        * Switch player to 'in menus' and disable their controls
        * Set timescale to zero just to be safe
        */
    }

    bool AllObjectivesCompleted()
    {
        foreach (Objective o in allObjectives)
        {
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
