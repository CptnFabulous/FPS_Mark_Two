using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class OffhandSelectorHUD : MonoBehaviour
{
    public RadialMenu radialMenu;

    [Header("Info on selected firing mode")]
    //public Image weaponImage;
    //public Text weaponName;
    public Text firingModeName;
    public Text ammoCapacity;
    public Image ammoIcon;

    OffhandAttackHandler handler;

    private void Awake()
    {
        radialMenu.onValueChanged.AddListener(DisplayInfoOnSelectedMode);
    }

    public void PopulateMenu(OffhandAttackHandler handler)
    {
        this.handler = handler;

        List<Sprite> icons = new List<Sprite>();
        foreach (WeaponMode m in handler.abilities)
        {
            icons.Add(m.icon);
        }
        radialMenu.Refresh(icons.ToArray(), CalculateIndex);


        // Update resource values
        for (int i = 0; i < radialMenu.options.Count; i++)
        {
            int index = i;
            radialMenu.options[index].resourceDisplay.obtainValues = () => GetResourceData(index);
        }
    }

    int CalculateIndex() => handler.abilities.IndexOf(handler.currentAbility);
    Resource GetResourceData(int index) => handler.abilities[index].displayedResource;
    void DisplayInfoOnSelectedMode(int index)
    {
        WeaponMode mode = handler.abilities[index];

        firingModeName.text = mode.name;
        //weaponName.text = mode.attachedTo.name;

        if (mode is RangedAttack r)
        {
            AmmunitionType ammoType = r.stats.ammoType;
            ammoIcon.sprite = (ammoType != null) ? ammoType.icon : null;
        }

        ammoCapacity.text = mode.hudInfo;
    }
}