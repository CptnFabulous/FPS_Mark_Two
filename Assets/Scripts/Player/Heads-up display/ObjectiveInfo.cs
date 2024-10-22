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
    [SerializeField] string failMessageFormat = "Failed ({0})";
    [SerializeField] string optionalPrefix = "(Optional) ";


    [SerializeField] CanvasGroup canvasGroup;

    Dictionary<Objective, TMP_Text> displays;

    private void Awake()
    {
        displays = new Dictionary<Objective, TMP_Text>();
        objectivePrefab.gameObject.SetActive(false);
    }
    
    private void LateUpdate()
    {
        bool objectivesToShow = ObjectiveHandler.current != null && ObjectiveHandler.current.allObjectives.Count > 0;
        canvasGroup.alpha = objectivesToShow ? 1 : 0;
        if (objectivesToShow == false) return;

        RefreshDisplay();
    }
    private void RefreshDisplay()
    {
        if (ObjectiveHandler.current == null) return;
        RefreshDisplay(ObjectiveHandler.current.activeObjectives);
    }
    public void RefreshDisplay(IList<Objective> objectivesToShow)
    {
        // Check for new objectives. If one is active but a display is not present, add one.
        for (int i = 0; i < objectivesToShow.Count; i++)
        {
            Objective objective = objectivesToShow[i];
            if (displays.ContainsKey(objective) == false)
            {
                // If a display is not present, create a new one
                TMP_Text newDisplay = Instantiate(objectivePrefab, listParent.transform);
                newDisplay.gameObject.SetActive(true);
                displays[objective] = newDisplay;
            }

            TMP_Text display = displays[objective];

            display.transform.SetSiblingIndex(i);

            #region Text
            string task = objective.name;

            string progress = objective.status switch
            {
                ObjectiveStatus.Completed => completedMessage,
                ObjectiveStatus.Failed => string.Format(failMessageFormat, objective.formattedProgress),
                _ => objective.formattedProgress,
            };

            //string progress = objective.status == ObjectiveStatus.Completed ? completedMessage : objective.formattedProgress;

            // Ensure the optional prefix is present if it's optional, and not if it isn't
            bool hasPrefix = task.StartsWith(optionalPrefix);
            if (objective.optional && !hasPrefix)
            {
                task = optionalPrefix + task;
            }
            else if (!objective.optional && hasPrefix)
            {
                task.Remove(0, optionalPrefix.Length);
            }

            display.text = (progress == null) ? task : string.Format(progressFormatting, task, progress);
            #endregion
        }
    }
}
