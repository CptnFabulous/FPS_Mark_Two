using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class WeaponModeInfo : MonoBehaviour
{
    [SerializeField] ResourceDisplay meter;
    [SerializeField] Text ammoText;
    [SerializeField] Text weaponModeName;
    [SerializeField] Image weaponModeIcon;

    WeaponMode _m;
    public WeaponMode mode
    {
        get => _m;
        set
        {
            _m = value;

            bool active = _m != null;
            enabled = active;
            gameObject.SetActive(active);
        }
    }

    private void Awake()
    {
        if (meter != null) meter.obtainValues += () => mode.displayedResource;
    }
    private void LateUpdate()
    {
        if (mode == null) return;

        // Display name and icon
        if (weaponModeName != null) weaponModeName.text = mode.name;
        if (weaponModeIcon != null) weaponModeIcon.sprite = mode.icon;

        string info = mode.hudInfo;
        ammoText.text = info;
        ammoText.gameObject.SetActive(info != null);
    }
}