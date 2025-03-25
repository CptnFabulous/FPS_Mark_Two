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
    [SerializeField] Vector3 rotationOffset = new Vector3(0, 0, 90);
    [SerializeField] Vector3 rotationOffsetIfUpsideDown = new Vector3(180, 0, 0);

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

        radialMenu.onValueChanged.AddListener(DisplayInfoOnSelectedMode);
    }

    public void Refresh(WeaponHandler newHandler)
    {
        handler = newHandler;
        
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
                float weaponGraphicOrder = modeIndex + ((numberOfModes - 1) * 0.5f);
                //Debug.Log(weaponGraphicOrder);
                radialMenu.AddVisualEffect(weaponGraphic.rectTransform, weaponGraphicOrder, graphicDistanceFromCentre, rotateWeaponGraphics);
                weaponGraphic.rectTransform.SetSiblingIndex(weaponGraphicPrefab.rectTransform.GetSiblingIndex() + 1);

                // Apply extra rotation offsets
                weaponGraphic.rectTransform.localRotation *= Quaternion.Euler(rotationOffset);
                if (Vector3.Dot(weaponGraphic.transform.up, radialMenu.transform.up) < 0)
                {
                    weaponGraphic.rectTransform.localRotation *= Quaternion.Euler(rotationOffsetIfUpsideDown);
                }
            }
            
            // Set up dividers
            if (dividerPrefab != null)
            {
                RectTransform divider = Instantiate(dividerPrefab, radialMenu.transform);
                float dividerOrder = modeIndex - 0.5f;
                radialMenu.AddVisualEffect(divider, dividerOrder, 1, rotateDividers);
            }
            
            modeIndex += numberOfModes;
        }
        #endregion
    }

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
}
