using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.InputSystem;

public class RadialMenu : MonoBehaviour
{
    [Header("Stats")]
    public Image[] options;
    public UnityEvent<int> onValueConfirmed;

    public int Value
    {
        get
        {
            return index;
        }
        set
        {
            value = MiscFunctions.InverseClamp(value, 0, options.Length - 1);
            if (index != value) // Only perform updating code if value is changed
            {
                index = value;
                onValueChanged.Invoke(index);
            }
        }
    }
    public bool OptionsPresent
    {
        get
        {
            return options.Length > 0;
        }
    }
    
    [Header("Setup")]
    public Image optionPrefab;
    public bool rotateOptions;
    //public float angleOffset;
    public RectTransform selectorAxis;
    public UnityEvent<int> onValueChanged;

    CanvasGroup elements;
    public bool active { get; private set; }
    int index;
    Vector2 cursorDirection;
    float SegmentSize
    {
        get
        {
            return 360 / options.Length;
        }
    }
    float SelectionAngle
    {
        get
        {
            float f = Vector2.SignedAngle(cursorDirection, Vector2.up);
            if (f < 0)
            {
                f += 360;
            }
            return f;
        }
    }

    private void Awake()
    {
        elements = GetComponent<CanvasGroup>();
        optionPrefab.gameObject.SetActive(false);
        SetActiveState(false);
    }
    void SetActiveState(bool enabled)
    {
        active = enabled;
        elements.alpha = active ? 1 : 0;
        elements.interactable = active;
        elements.blocksRaycasts = active;
    }

    #region Updating and input
    /// <summary>
    /// Populates the radial menu with a series of options
    /// </summary>
    /// <param name="icons"></param>
    public void Refresh(Sprite[] icons)
    {
        // Destroy existing options icons
        // Implementing code to save and repurpose current ones might theoretically improve performance, but I'll wait until it actually causes problems.
        for (int i = 0; i < options.Length; i++)
        {
            if (options[i] != null)
            {
                Destroy(options[i].gameObject);
            }
        }

        // New array of options icons
        options = new Image[icons.Length];
        for (int i = 0; i < options.Length; i++)
        {
            // Establishes rotation relative to centre, and position to spawn object in
            Quaternion rotation = Quaternion.Euler(0, 0, -SegmentSize * i);
            Vector3 position = rotation * Vector3.up * Vector2.Distance(optionPrefab.rectTransform.anchoredPosition, Vector2.zero);
            options[i] = Instantiate(optionPrefab, transform); // Spawns icon
            options[i].gameObject.SetActive(true); // Enables icon since the prefab is hidden
            options[i].sprite = icons[i]; // Assigns sprite
            options[i].rectTransform.anchoredPosition = position; // Sets position
            // If rotateOptions is enabled, rotates icon around centre axis, otherwise sets to zero
            options[i].rectTransform.localRotation = rotateOptions ? rotation : Quaternion.identity;
        }
    }
    /// <summary>
    /// Inputs a 2D vector based off a mouse or analog stick input, to update the selection angle
    /// </summary>
    /// <param name="value"></param>
    public void InputDirection(Vector2 value)
    {
        if (!active)
        {
            return;
        }
        // Input mouse/analog stick movement
        cursorDirection += value;
        if (cursorDirection.magnitude > 1)
        {
            cursorDirection.Normalize();
        }

        Value = Mathf.RoundToInt(SelectionAngle / SegmentSize);
        selectorAxis.localRotation = Quaternion.Euler(0, 0, -SelectionAngle);
    }

    /// <summary>
    /// Opens the radial menu and updates it to the current selection.
    /// </summary>
    /// <param name="index"></param>
    public void EnterMenu(int newIndex)
    {
        if (options.Length <= 0)
        {
            return;
        }

        index = newIndex;
        onValueChanged.Invoke(index);
        
        cursorDirection = Vector2.zero;
        selectorAxis.localRotation = Quaternion.Euler(0, 0, -SegmentSize * index);
        SetActiveState(true);
    }
    /// <summary>
    /// Closes the menu and applies the selection.
    /// </summary>
    public void ExitMenu()
    {
        SetActiveState(false);
        onValueConfirmed.Invoke(Value);
    }
    #endregion

    #region Optional functions
    public void PositionHighlight(RectTransform selectionHighlight)
    {
        selectionHighlight.anchoredPosition = options[Value].rectTransform.anchoredPosition;
        selectionHighlight.rotation = options[Value].rectTransform.rotation;
    }
    #endregion
}
