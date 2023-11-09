using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ObjectiveInfo : MonoBehaviour
{
    [SerializeField] TMP_Text objectivePrefab;
    [SerializeField] LayoutGroup listParent;
    [SerializeField] string progressFormatting = "{0}: {1}";
    [SerializeField] string completedMessage = "Completed";
    [SerializeField] string optionalPrefix = "(Optional) ";

    Dictionary<Objective, TMP_Text> displays;

    private void Awake()
    {
        displays = new Dictionary<Objective, TMP_Text>();
        objectivePrefab.gameObject.SetActive(false);
        //ObjectiveHandler.onObjectiveUpdated += (_) => RefreshDisplay();
    }
    private void OnEnable()
    {
        Debug.Log("Initial refresh");
        //RefreshDisplay();
    }
    private void Start()
    {
        //RefreshDisplay();
    }
    private void LateUpdate()
    {
        RefreshDisplay();
    }
    private void RefreshDisplay()
    {
        Debug.Log("Checking if objective handler exists");

        if (ObjectiveHandler.current == null) return;

        Debug.Log("Refreshing display");

        // Check for new objectives. If one is active but a display is not present, add one.
        for (int i = 0; i < ObjectiveHandler.current.activeObjectives.Count; i++)
        {
            Objective objective = ObjectiveHandler.current.activeObjectives[i];
            if (displays.ContainsKey(objective) == false)
            {
                // If a display is not present, create a new one
                TMP_Text newDisplay = Instantiate(objectivePrefab, listParent.transform);
                newDisplay.gameObject.SetActive(true);
                displays[objective] = newDisplay;
            }

            displays[objective].transform.SetSiblingIndex(i);
        }

        // Update all objective displays
        foreach (Objective objective in displays.Keys)
        {
            TMP_Text display = displays[objective];
            string task = objective.name;
            string progress = objective.status == ObjectiveStatus.Completed ? completedMessage : objective.formattedProgress;

            // Ensure the optional prefix is present if it's optional, and not if it isn't
            bool hasPrefix = task.StartsWith(optionalPrefix);
            if (objective.optional && hasPrefix == false)
            {
                task = optionalPrefix + task;
            }
            else if (objective.optional == false && hasPrefix)
            {
                task.Remove(0, optionalPrefix.Length);
            }

            display.text = (progress == null) ? task : string.Format(progressFormatting, task, progress);
        }
    }
}
