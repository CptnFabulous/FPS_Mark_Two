using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public abstract class OptionsMenu : MonoBehaviour
{
    [Header("General elements")]
    public Button apply;
    public Button revert;
    MenuWindow attachedWindow;

    public virtual void Awake()
    {
        attachedWindow = GetComponent<MenuWindow>();

        apply.onClick.AddListener(() => StartCoroutine(Apply()));
        revert.onClick.AddListener(() => Refresh());
        SetupOptions();
    }
    void OnEnable()
    {
        Refresh();
    }
    IEnumerator Apply()
    {
        ApplySettings();
        // Due to some weird shenanigans, the video options won't register the new settings until I wait two frames. Even just waiting one frame didn't work.
        yield return new WaitForEndOfFrame();
        yield return new WaitForEndOfFrame();
        Debug.Log("Settings applied and frame has passed, refreshing now");
        Refresh();
    }

    void Refresh()
    {
        ObtainCurrentValues();
        //Debug.Log("Reverting apply and revert buttons on frame " + Time.frameCount);
        apply.interactable = false;
        revert.interactable = false;
        // Set currently selected option to the default, since pressing the apply or revert buttons will disable them, making it impossible to switch away manually.
        UnityEngine.EventSystems.EventSystem.current.SetSelectedGameObject(attachedWindow.firstSelection.gameObject);
    }
    public void OnOptionsChanged()
    {
        apply.interactable = true;
        revert.interactable = true;
    }
    
    /// <summary>
    /// Add listeners to each option to run OnOptionsChanged()
    /// </summary>
    public abstract void SetupOptions();

    /// <summary>
    /// Obtain reference to whatever settings are being altered, and refresh menu options to match
    /// </summary>
    public abstract void ObtainCurrentValues();

    /// <summary>
    /// Apply the values in the menu options to the appropriate setting variables
    /// </summary>
    public abstract void ApplySettings();

    public void AddValueChangedEvent(Slider interactable)
    {
        interactable.onValueChanged.AddListener((_) => OnOptionsChanged());
    }
    public void AddValueChangedEvent(Toggle interactable)
    {
        interactable.onValueChanged.AddListener((_) => OnOptionsChanged());
    }
    public void AddValueChangedEvent(Dropdown interactable)
    {
        interactable.onValueChanged.AddListener((_) => OnOptionsChanged());
    }
    public void AddValueChangedEvent(InputField interactable)
    {
        interactable.onValueChanged.AddListener((_) => OnOptionsChanged());
    }
    public void AddValueChangedEvent(Button interactable)
    {
        interactable.onClick.AddListener(OnOptionsChanged);
    }


    /// <summary>
    /// Update a slider's value to represent a scaled sensitivity value. Some sensitivity values can be very large or small numbers, so this converts them to a slider range designed for better UX
    /// </summary>
    /// <param name="value"></param>
    /// <param name="absoluteMaxValue"></param>
    /// <returns></returns>
    public static float SliderValueToSensitivity(Slider value, float maxValue)
    {
        return value.value / value.maxValue * maxValue;
    }

    /// <summary>
    /// Converts a slider value from the slider's range (optimised for UX) to a value of similar scale but usable for its intended purpose.
    /// </summary>
    /// <param name="value"></param>
    /// <param name="currentValue"></param>
    /// <param name="maxValue"></param>
    public static void SensitivityToSliderValue(Slider value, float currentValue, float maxValue)
    {
        // Update the options to reflect the current value.
        float valueToShow = currentValue / maxValue * value.maxValue; // Turns original value into a -1 to 1 range
        value.value = Mathf.Clamp(valueToShow, value.minValue, value.maxValue); // Clamp value and update slider accordingly
        value.onValueChanged.Invoke(value.value);
    }
}
