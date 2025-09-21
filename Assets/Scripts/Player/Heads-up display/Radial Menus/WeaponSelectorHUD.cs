using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class WeaponSelectorHUD : MonoBehaviour
{
    [SerializeField] RadialMenu radialMenu;
    
    /*
    [Header("Dividers")]
    [SerializeField] RectTransform dividerPrefab;
    [SerializeField] bool rotateDividers;
    */
    
    [Header("Weapon graphics")]
    [SerializeField] Image weaponGraphicPrefab;
    [SerializeField] RadialMenuSlice weaponIconSlicePrefab;
    [SerializeField] bool rotateWeaponGraphics;
    [SerializeField] float graphicDistanceFromCentre = 0.5f;

    [Header("Info on selected firing mode")]
    //public Image weaponImage;
    public Text weaponName;
    public Text firingModeName;
    public Text ammoCapacity;
    public Image ammoIcon;

    WeaponHandler handler;

    private void Awake()
    {
        //if (dividerPrefab != null) dividerPrefab.gameObject.SetActive(false);
        if (weaponGraphicPrefab != null) weaponGraphicPrefab.gameObject.SetActive(false);
        if (weaponIconSlicePrefab != null) weaponIconSlicePrefab.gameObject.SetActive(false);

        radialMenu.onValueChanged.AddListener(DisplayInfoOnSelectedMode);
    }

    public void Refresh(WeaponHandler newHandler)
    {
        handler = newHandler;

        #region Add options

        // Get all the icons from all the firing modes, and put them in a list
        List<Sprite> icons = new List<Sprite>();
        foreach (Weapon w in handler.equippedWeapons)
        {
            foreach (WeaponMode m in w.modes) icons.Add(m.icon);
        }
        radialMenu.Refresh(icons.ToArray(), CalculateIndex);

        // Update resource values
        for (int i = 0; i < radialMenu.options.Count; i++)
        {
            int index = i;
            radialMenu.options[index].resourceDisplay.obtainValues = () => GetResourceData(index);
        }

        #endregion

        #region Add dividers and weapon icons

        int modeIndex = 0; // An index representing the number of modes so far in the foreach loop
        foreach (Weapon w in handler.equippedWeapons)
        {
            int numberOfModes = w.modes.Length;

            // Create slice showing the weapon type under the firing modes.
            if (weaponIconSlicePrefab != null)
            {
                RadialMenuSlice newSlice = Instantiate(weaponIconSlicePrefab, radialMenu.transform);
                newSlice.sprite = w.hudGraphic;

                float weaponGraphicOrder = modeIndex + ((numberOfModes - 1) * 0.5f);
                radialMenu.AddSegment(newSlice, weaponGraphicOrder);
                newSlice.UpdateSegmentSize(radialMenu.segmentSize * numberOfModes);
            }
            
            /*
            // Set up dividers
            if (dividerPrefab != null)
            {
                RectTransform divider = Instantiate(dividerPrefab, radialMenu.transform);
                float dividerOrder = modeIndex - 0.5f;
                radialMenu.AddVisualEffect(divider, dividerOrder, 1, rotateDividers);
            }
            */
            
            modeIndex += numberOfModes;
        }

        #endregion
    }

    int CalculateIndex() => handler.SelectorIndexFromWeaponAndMode(handler.equippedWeaponIndex, handler.CurrentWeapon.currentModeIndex);
    void DisplayInfoOnSelectedMode(int index)
    {
        handler.GetWeaponAndModeFromSelector(index, out int weaponIndex, out int firingModeIndex);
        WeaponMode mode = handler.equippedWeapons[weaponIndex].modes[firingModeIndex];

        firingModeName.text = mode.name;
        weaponName.text = mode.attachedTo.name;

        if (mode is RangedAttack r)
        {
            AmmunitionType ammoType = r.stats.ammoType;
            ammoIcon.sprite = (ammoType != null) ? ammoType.icon : null;
        }

        ammoCapacity.text = mode.hudInfo;
    }

    Resource GetResourceData(int index)
    {
        handler.GetWeaponAndModeFromSelector(index, out int weaponIndex, out int firingModeIndex);
        WeaponMode mode = handler.equippedWeapons[weaponIndex].modes[firingModeIndex];
        return mode.displayedResource;
    }
}