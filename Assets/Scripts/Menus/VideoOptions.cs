using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class VideoOptions : OptionsMenu
{
    [Header("Simple options")]
    public Dropdown fullscreenMode;

    public Dropdown resolutions;
    public Dropdown refreshRates;
    List<List<Resolution>> resolutionsAndRefreshRates;

    public Dropdown graphicsQualityPreset;
    public bool applyExpensiveQualityPresetChanges = true;


    public override void ApplySettings()
    {
        QualitySettings.SetQualityLevel(graphicsQualityPreset.value, applyExpensiveQualityPresetChanges);
        Resolution newResolution = resolutionsAndRefreshRates[resolutions.value][refreshRates.value];
        Screen.SetResolution(newResolution.width, newResolution.height, (FullScreenMode)fullscreenMode.value, newResolution.refreshRate);
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
        
        #region Sort resolutions and refresh rates
        // Create a list of lists, so each distinct resolution has its variants put in its own list
        resolutionsAndRefreshRates = new List<List<Resolution>>();
        // Obtain all available Resolution structs, converted to a list of varying length
        List<Resolution> allResolutions = new List<Resolution>(Screen.resolutions);
        while (allResolutions.Count > 0)
        {
            // Check the first struct in allResolutions and find every one in the list with the same width and height
            List<Resolution> newResolution = allResolutions.FindAll((r) => r.width == allResolutions[0].width && r.height == allResolutions[0].height);
            // Sort list by refresh rates
            newResolution.Sort((a, b) => a.refreshRate.CompareTo(b.refreshRate));
            // Add this list to the tables
            resolutionsAndRefreshRates.Add(newResolution);
            // Remove all entries from allResolutions that were copied to newResolution, using the same check as before.
            allResolutions.RemoveAll((r) => r.width == newResolution[0].width && r.height == newResolution[0].height);
            // This code will loop, check the next 'first' struct, copy all with an identical width and height to a separate list, and delete them from allResolutions.
            // This will keep occurring until allResolutions is empty, and therefore that all entries have been sorted into distinct tables.
        }
        // Sort tables by the resolution in each one
        resolutionsAndRefreshRates.Sort((a, b) => (a[0].width * a[0].height).CompareTo(b[0].width * b[0].height));
        #endregion
        
        #region Find current resolution and refresh rate, update both dropdowns and assign the appropriate values
        FindCorrectResolutionAndRefreshRate(out int resolutionIndex, out int refreshRateIndex);

        resolutions.ClearOptions();
        for (int i = 0; i < resolutionsAndRefreshRates.Count; i++)
        {
            Resolution r = resolutionsAndRefreshRates[i][0];
            Dropdown.OptionData option = new Dropdown.OptionData(r.width + " X " + r.height);
            resolutions.options.Add(option);
        }

        UpdateRefreshRateDropdown(resolutionIndex);

        resolutions.SetValueWithoutNotify(resolutionIndex);
        refreshRates.SetValueWithoutNotify(refreshRateIndex);
        #endregion
    }
    void FindCorrectResolutionAndRefreshRate(out int resolutionIndex, out int refreshRateIndex)
    {
        resolutionIndex = -1;
        refreshRateIndex = -1;
        Resolution current = Screen.currentResolution;
        for (int i = 0; i <  resolutionsAndRefreshRates.Count; i++)
        {
            List<Resolution> appropriate = resolutionsAndRefreshRates[i];
            Resolution r = appropriate[0];
            if (current.width == r.width && current.height == r.height)
            {
                resolutionIndex = i;
                for (int f = 0; f < appropriate.Count; f++)
                {
                    if (current.refreshRate == appropriate[f].refreshRate)
                    {
                        refreshRateIndex = f;
                        return;
                    }
                }
                break;
            }
        }
    }
    void UpdateRefreshRateDropdown(int resolutionIndex)
    {
        refreshRates.ClearOptions();
        List<Resolution> correct = resolutionsAndRefreshRates[resolutionIndex];
        for (int i = 0; i < correct.Count; i++)
        {
            Resolution r = correct[i];
            Dropdown.OptionData option = new Dropdown.OptionData(r.refreshRate + "Hz");
            refreshRates.options.Add(option);
        }
        FindCorrectResolutionAndRefreshRate(out resolutionIndex, out int refreshRateIndex);
        refreshRates.SetValueWithoutNotify(refreshRateIndex);
    }



    public override void SetupOptions()
    {
        AddValueChangedEvent(resolutions);
        resolutions.onValueChanged.AddListener(UpdateRefreshRateDropdown);
        AddValueChangedEvent(refreshRates);
        AddValueChangedEvent(fullscreenMode);
        AddValueChangedEvent(graphicsQualityPreset);
    }



    public static bool ResolutionsMatch(Resolution lhs, Resolution rhs)
    {
        return lhs.width == rhs.width && lhs.height == rhs.height;
    }
    public static bool RefreshRatesMatch(Resolution lhs, Resolution rhs)
    {
        return lhs.refreshRate == rhs.refreshRate;
    }

}
