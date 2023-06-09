using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class RadialMenu : MonoBehaviour
{
    public float mouseSensitivity = 0.25f;

    public UnityEvent<int> onValueChanged;
    public UnityEvent<int> onValueConfirmed;

    [Header("Cosmetics")]
    [SerializeField] RectTransform selectorAxis;
    [SerializeField] Image optionPrefab;
    [SerializeField] bool optionRotationsMatchAngle;

    CanvasGroup elements;
    RectTransform[] options = new RectTransform[0];
    List<RectTransform> visualElements = new List<RectTransform>();

    int cachedIndex; // Don't edit this directly except from inside 'value' setter
    Vector2 cursorDirection;
    HeadsUpDisplay ph;

    public bool menuIsOpen { get; private set; }
    public int value
    {
        get => cachedIndex;
        set
        {
            // Should I put this code in InputDirection() instead?
            // I might also want to set up EnterMenu() to use InputDirection() as well since there's a lot of overlap.
            
            value = Mathf.Clamp(value, 0, options.Length - 1);
            if (cachedIndex == value) return;// Only perform updating code if value is changed

            cachedIndex = value;
            onValueChanged.Invoke(cachedIndex);
        }
    }
    public int numberOfOptions => options.Length;
    public bool optionsPresent => options != null && options.Length > 0;
    float radius => Vector2.Distance(optionPrefab.rectTransform.anchoredPosition, Vector2.zero);
    float segmentSize => optionsPresent ? (360 / options.Length) : 360;
    HeadsUpDisplay parentHUD => ph ??= GetComponentInParent<HeadsUpDisplay>();

    #region Setup
    private void Awake()
    {
        elements = GetComponent<CanvasGroup>();
        optionPrefab.gameObject.SetActive(false);
        SetActiveState(false);
    }
    void SetActiveState(bool enabled)
    {
        menuIsOpen = enabled;
        elements.alpha = menuIsOpen ? 1 : 0;
        elements.interactable = menuIsOpen;
        elements.blocksRaycasts = menuIsOpen;
    }
    void Clear()
    {
        // Clear options and destroy all instantiated visuals
        // Implementing code to save and repurpose current ones might theoretically improve performance, but I'll wait until it actually causes problems.
        foreach (RectTransform visual in visualElements) Destroy(visual.gameObject);
        visualElements.Clear();

        options = new RectTransform[0]; // Set length to zero
    }
    void SetSelectorAngle(float angle) => selectorAxis.localRotation = Quaternion.Euler(0, 0, -angle);
    #endregion

    #region Populating menu
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
            Image newOption = Instantiate(optionPrefab, transform);
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
    public void AddVisualEffect(RectTransform objectTransform, float orderIndex, float distance, bool preserveAngle)
    {
        // Establishes rotation relative to centre, and position to spawn object in
        Quaternion rotation = Quaternion.Euler(0, 0, -segmentSize * orderIndex);
        Vector3 position = radius * distance * (rotation * Vector3.up);

        // Parents the object transform and sets up its position and rotation
        objectTransform.SetParent(transform);
        objectTransform.anchoredPosition = position; // Sets position
        objectTransform.localRotation = preserveAngle ? rotation : Quaternion.identity; // If preserveAngle is false, object retains an upright rotation

        // Activates the visual and ensures the radial menu recognises it.
        visualElements.Add(objectTransform);
        objectTransform.gameObject.SetActive(true);
    }
    #endregion

    #region Controls
    /// <summary>
    /// Inputs a 2D vector based off a mouse or analog stick input, to update the selection angle
    /// </summary>
    /// <param name="inputVector"></param>
    public void InputDirection(Vector2 inputVector, bool usingMouse)
    {
        if (!menuIsOpen) return;

        // Input mouse/analog stick movement
        if (usingMouse)
        {
            cursorDirection += inputVector * mouseSensitivity;
        }
        else
        {
            cursorDirection = inputVector;
        }
        if (cursorDirection.magnitude > 1) cursorDirection.Normalize();

        // Calculate a 0-360 degree angle based off the vector
        float selectionAngle = Vector2.SignedAngle(cursorDirection, Vector2.up);
        if (selectionAngle < 0) selectionAngle += 360;

        // Calculate the correct index for the angle
        int valueToSet = Mathf.RoundToInt(selectionAngle / segmentSize);
        if (valueToSet >= options.Length) valueToSet = 0;
        value = valueToSet;

        SetSelectorAngle(selectionAngle);
    }
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
        SetSelectorAngle(segmentSize * cachedIndex);
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

    #region Cosmetic functions
    public void PositionHighlight(RectTransform selectionHighlight)
    {
        selectionHighlight.anchoredPosition = options[value].anchoredPosition;
        selectionHighlight.rotation = options[value].rotation;
    }
    public void RotateToIndexAngle(Transform toRotate)
    {
        float angle = value * segmentSize;
        toRotate.localRotation = Quaternion.Euler(0, 0, -angle);
    }
    public void PlayOneShotAnimation(Animation animation) => animation.Play();
    public void PlaySoundEffect(AudioClip clip) => parentHUD.PlayAudioClip(clip);
    #endregion
}