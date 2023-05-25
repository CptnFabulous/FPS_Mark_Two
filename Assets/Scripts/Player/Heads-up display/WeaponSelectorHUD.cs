using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class WeaponSelectorHUD : MonoBehaviour
{
    [SerializeField] RadialMenu radialMenu;

    [Header("Dividers")]
    [SerializeField] RectTransform dividerPrefab;
    [SerializeField] bool rotateDividers;

    [Header("Weapon graphics")]
    [SerializeField] Image weaponGraphicPrefab;
    //[SerializeField] float weaponGraphicRotationOffset;
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
        dividerPrefab.gameObject.SetActive(false);
        weaponGraphicPrefab?.gameObject.SetActive(false);
    }

    public void Setup(WeaponHandler newHandler)
    {
        handler = newHandler;
        SetupRadialMenu();
    }
    void SetupRadialMenu()
    {
        #region Add options
        List<Sprite> icons = new List<Sprite>();
        foreach (Weapon w in handler.equippedWeapons) // Get all the icons from all the firing modes
        {
            foreach (WeaponMode m in w.modes)
            {
                icons.Add(m.icon);
            }
        }
        radialMenu.Refresh(icons.ToArray());
        #endregion

        #region Add dividers and weapon icons
        int modeIndex = 0; // An index representing the number of modes so far in the foreach loop
        foreach (Weapon w in handler.equippedWeapons)
        {
            int numberOfModes = w.modes.Length;

            if (weaponGraphicPrefab != null) // Set up graphics for each weapon (if prefab is present)
            {
                // WIP: Positions are bugged out for some reason
                Image weaponGraphic = Instantiate(weaponGraphicPrefab, radialMenu.transform);
                weaponGraphic.sprite = w.hudGraphic;
                float weaponGraphicOrder = modeIndex + ((float)numberOfModes * 0.5f);
                radialMenu.AddVisualEffect(weaponGraphic.rectTransform, weaponGraphicOrder, 1, rotateWeaponGraphics, true);
            }
            
            // Set up dividers
            RectTransform divider = Instantiate(dividerPrefab, radialMenu.transform);
            float dividerOrder = modeIndex - 0.5f;
            radialMenu.AddVisualEffect(divider, dividerOrder, 1, rotateDividers);

            modeIndex += numberOfModes;
        }
        #endregion

        radialMenu.onValueChanged.RemoveAllListeners();
        radialMenu.onValueChanged.AddListener(DisplayInfoOnSelectedMode);
    }








    void DisplayInfoOnSelectedMode(int index)
    {
        handler.GetWeaponAndModeFromSelector(index, out int weaponIndex, out int firingModeIndex);
        WeaponMode mode = handler.equippedWeapons[weaponIndex].modes[firingModeIndex];
        if (mode is RangedAttack r)
        {
            DisplayRangedAttackInfo(r);
        }
    }
    void DisplayRangedAttackInfo(RangedAttack mode)
    {
        firingModeName.text = mode.name;
        weaponName.text = mode.attachedTo.name;

        AmmunitionType ammoType = mode.stats.ammoType;
        ammoIcon.sprite = (ammoType != null) ? ammoType.icon : null;

        // If weapon consumes ammo, show reserve
        bool consumesAmmo = mode.stats.ConsumesAmmo;
        if (consumesAmmo)
        {
            int ammoCurrent = (int)mode.attachedTo.user.ammo.GetValues(ammoType).current;

            if (mode.magazine != null) // If magazine is present, change ammo bar to show reserve excluding magazine amount
            {
                int magazineCurrent = (int)mode.magazine.ammo.current;
                ammoCapacity.text = magazineCurrent + "/" + (ammoCurrent - magazineCurrent);
            }
            else
            {
                ammoCapacity.text = ammoCurrent.ToString();
            }
        }
        else
        {
            ammoCapacity.text = "INFINITE";
        }
    }
}
