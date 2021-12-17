using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class VideoOptions : OptionsMenu
{
    [Header("Simple options")]
    public Dropdown fullscreenMode;
    public Dropdown graphicsQualityPreset;
    public Dropdown resolutions;
    public SliderWithField refreshRateTarget;

    List<Resolution> resolutionStructs;
    public bool applyExpensiveQualityPresetChanges = true;

    public override void ApplySettings()
    {
        QualitySettings.SetQualityLevel(graphicsQualityPreset.value, applyExpensiveQualityPresetChanges);
        Resolution r = resolutionStructs[resolutions.value];
        Screen.SetResolution(r.width, r.height, (FullScreenMode)fullscreenMode.value, Mathf.RoundToInt(refreshRateTarget.slider.value));
    }
    public override void ObtainCurrentValues()
    {
        #region Fullscreen
        string[] fullScreenOptions = new string[]
        {
            "Exclusive Fullscreen",
            "Fullscreen Windowed",
            "Maximised Windowed",
            "Windowed",
        };//System.Enum.GetNames(typeof(FullScreenMode));
        fullscreenMode.ClearOptions();
        fullscreenMode.AddOptions(new List<string>(fullScreenOptions));
        fullscreenMode.value = (int)Screen.fullScreenMode;
        fullscreenMode.RefreshShownValue();
        #endregion

        #region Simple graphics quality
        graphicsQualityPreset.ClearOptions();
        graphicsQualityPreset.AddOptions(new List<string>(QualitySettings.names));
        graphicsQualityPreset.value = QualitySettings.GetQualityLevel();
        graphicsQualityPreset.RefreshShownValue();
        #endregion

        #region Setup refresh rate slider
        List<Resolution> allResolutions = new List<Resolution>(Screen.resolutions);
        allResolutions.Sort((lhs, rhs) => lhs.refreshRate.CompareTo(rhs.refreshRate));
        refreshRateTarget.slider.minValue = allResolutions[0].refreshRate;
        refreshRateTarget.slider.maxValue = allResolutions[allResolutions.Count - 1].refreshRate;
        refreshRateTarget.slider.value = Screen.currentResolution.refreshRate;

        refreshRateTarget.slider.interactable = Screen.fullScreenMode == FullScreenMode.ExclusiveFullScreen;
        refreshRateTarget.textBox.interactable = refreshRateTarget.slider.interactable;
        #endregion

        #region Setup resolution list
        resolutionStructs = new List<Resolution>();
        while (allResolutions.Count > 0)
        {
            Resolution res = allResolutions[0];
            resolutionStructs.Add(res);
            allResolutions.RemoveAll((r) => r.width == res.width && r.height == res.height);
        }
        resolutionStructs.Sort((a, b) => (a.width * a.height).CompareTo(b.width * b.height));
        #endregion

        #region Setup resolution dropdown
        int currentResolutionIndex = 0;
        resolutions.ClearOptions();
        for (int i = 0; i < resolutionStructs.Count; i++)
        {
            Resolution r = resolutionStructs[i];
            string optionText = r.width + " X " + r.height;
            resolutions.options.Add(new Dropdown.OptionData(optionText));

            if (r.width == Screen.width && r.height == Screen.height)
            {
                //Debug.Log("Current resolution is " + currentScreenData);
                currentResolutionIndex = i;
            }
        }
        resolutions.value = currentResolutionIndex;
        resolutions.RefreshShownValue();
        #endregion
    }
    public override void SetupOptions()
    {
        AddValueChangedEvent(resolutions);
        AddValueChangedEvent(refreshRateTarget.slider);
        refreshRateTarget.slider.wholeNumbers = true;
        AddValueChangedEvent(fullscreenMode);
        AddValueChangedEvent(graphicsQualityPreset);
    }
}
