using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class RadialMenu : MonoBehaviour
{
    [SerializeField] Image optionPrefab;
    [SerializeField] bool optionRotationsMatchAngle;
    //public float angleOffset;
    [SerializeField] RectTransform selectorAxis;

    public UnityEvent<int> onValueChanged;
    public UnityEvent<int> onValueConfirmed;



    CanvasGroup elements;
    RectTransform[] options = new RectTransform[0];
    List<RectTransform> visualElements = new List<RectTransform>();

    int cachedIndex; // Don't edit this directly except from inside 'value' setter
    Vector2 cursorDirection;

    public bool active { get; private set; }
    public int value
    {
        get => cachedIndex;
        set
        {
            // Should I put this code in InputDirection() instead?
            // I might also want to set up EnterMenu() to use InputDirection() as well since there's a lot of overlap.
            
            value = Mathf.Clamp(value, 0, options.Length - 1);
            //value = MiscFunctions.InverseClamp(value, 0, options.Length - 1);
            if (cachedIndex == value) return;// Only perform updating code if value is changed

            cachedIndex = value;
            onValueChanged.Invoke(cachedIndex);
        }
    }
    public bool optionsPresent => options != null && options.Length > 0;
    float radius => Vector2.Distance(optionPrefab.rectTransform.anchoredPosition, Vector2.zero);
    float segmentSize => optionsPresent ? (360 / options.Length) : 360;
    
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

    #region Setup
    /// <summary>
    /// Populates the radial menu with a series of options.
    /// </summary>
    /// <param name="icons"></param>
    public void Refresh(Sprite[] icons)
    {
        Clear();

        options = new RectTransform[icons.Length];
        for (int i = 0; i < options.Length; i++)
        {
            Image newOption = Instantiate(optionPrefab);
            newOption.sprite = icons[i];
            AddVisualEffect(newOption.rectTransform, i, 1, optionRotationsMatchAngle);
            options[i] = newOption.rectTransform;
        }
    }
    /// <summary>
    /// Adds another visual element to the radial menu.
    /// </summary>
    /// <param name="objectTransform">The visual element to show.</param>
    /// <param name="orderIndex">Corresponds with the index of each option, but as a float so you can put things before or after.</param>
    /// <param name="distance"></param>
    /// <param name="preserveAngle"></param>
    public void AddVisualEffect(RectTransform objectTransform, float orderIndex, float distance, bool preserveAngle, bool addBehind = false)
    {
        // Establishes rotation relative to centre, and position to spawn object in
        Quaternion rotation = Quaternion.Euler(0, 0, -segmentSize * orderIndex);
        Vector3 position = radius * distance * (rotation * Vector3.up);

        // Parents the object transform and sets up its position and rotation
        objectTransform.SetParent(transform);
        if (addBehind)
        {
            objectTransform.SetAsFirstSibling();
        }
        objectTransform.anchoredPosition = position; // Sets position
        objectTransform.localRotation = preserveAngle ? rotation : Quaternion.identity; // If preserveAngle is false, object retains an upright rotation

        // Activates the visual and ensures the radial menu recognises it.
        visualElements.Add(objectTransform);
        objectTransform.gameObject.SetActive(true);
    }
    void Clear()
    {
        // Clear options and destroy all instantiated visuals
        // Implementing code to save and repurpose current ones might theoretically improve performance, but I'll wait until it actually causes problems.
        foreach (RectTransform visual in visualElements) Destroy(visual.gameObject);
        visualElements.Clear();

        options = new RectTransform[0]; // Set length to zero
    }
    #endregion


    #region Controls
    /// <summary>
    /// Inputs a 2D vector based off a mouse or analog stick input, to update the selection angle
    /// </summary>
    /// <param name="value"></param>
    public void InputDirection(Vector2 value)
    {
        if (!active) return;

        // Input mouse/analog stick movement
        cursorDirection += value;
        if (cursorDirection.magnitude > 1)
        {
            cursorDirection.Normalize();
        }

        // Calculate a 0-360 degree angle based off the vector
        float selectionAngle = Vector2.SignedAngle(cursorDirection, Vector2.up);
        if (selectionAngle < 0) selectionAngle += 360;

        // Calculate the correct index for the angle
        int valueToSet = Mathf.RoundToInt(selectionAngle / segmentSize);
        if (valueToSet >= options.Length) valueToSet = 0;
        value = valueToSet;

    /// <summary>
    /// Opens the radial menu and updates it to the current selection.
    /// </summary>
    /// <param name="index"></param>
    public void EnterMenu(int newIndex)
    {
        if (optionsPresent == false) return;

        // Force-update the index
        cachedIndex = newIndex;
        onValueChanged.Invoke(cachedIndex);
        
        cursorDirection = Vector2.zero;
        selectorAxis.localRotation = Quaternion.Euler(0, 0, -segmentSize * index);
        SetActiveState(true);
    }
    /// <summary>
    /// Closes the menu and applies the selection.
    /// </summary>
    public void ExitMenu()
    {
        SetActiveState(false);
        onValueConfirmed.Invoke(value);
    }
    #endregion

    #region Optional functions
    public void PositionHighlight(RectTransform selectionHighlight)
    {
        selectionHighlight.anchoredPosition = options[value].anchoredPosition;
        selectionHighlight.rotation = options[value].rotation;
    }
    #endregion
}
