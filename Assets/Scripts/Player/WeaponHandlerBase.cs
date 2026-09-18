using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public abstract class WeaponHandlerBase : MonoBehaviour
{
    [SerializeField] List<Weapon> weapons;
    [SerializeField] bool sortByOrderIndex = false;

    [Header("References")]
    [SerializeField] SingleInput selectorMenuInput;
    [SerializeField] MultiRadialMenu menu;
    [SerializeField] int selectorMenuIndex;
    [SerializeField] Transform holdingSocket;

    protected int selectionIndex;

    List<WeaponMode> _allModes = new List<WeaponMode>();

    public IReadOnlyList<Weapon> allWeapons => weapons;
    public IReadOnlyList<WeaponMode> allModes => _allModes;

    protected virtual void Awake()
    {
        if (menu != null && selectorMenuInput != null)
        {
            selectorMenuInput.onActionPerformed.AddListener((ctx) => menu.ProcessSingleMenuInput(selectorMenuIndex, ctx));
            menu.menus[selectorMenuIndex].onValueConfirmed.AddListener((index) => SwitchMode(index));
        }

        
    }
    private void Start()
    {
        // TO DO: pre-emptively add all weapons that are children of holding socket
        weapons.Clear();
        foreach (Weapon w in holdingSocket.GetComponentsInChildren<Weapon>(true))
        {
            TryAdd(w, false);
        }

        Refresh();

        // Switch to first mode
        SwitchMode(selectionIndex);
    }

    public abstract void SwitchMode(int index);

    #region Adding and removing

    public bool CanAdd(Weapon newWeapon)
    {
        if (newWeapon == null) return false;
        if (weapons.Contains(newWeapon)) return false;

        // If a duplicate weapon of the same type already exists, don't add a new one
        string properName = newWeapon.parentEntity.properName;
        if (allWeapons.FirstOrDefault((w) => w.parentEntity.properName == properName)) return false;

        return true;
    }
    public bool TryAdd(Weapon newWeapon, bool autoSwitch = true)
    {
        if (CanAdd(newWeapon) == false) return false;

        weapons.Add(newWeapon);

        // Assign proper transform values
        newWeapon.transform.SetParent(holdingSocket);
        newWeapon.transform.localPosition = Vector3.zero;
        newWeapon.transform.localRotation = Quaternion.identity;
        // Pre-emptively disable weapon object so that switching and setup can play properly
        newWeapon.gameObject.SetActive(false);

        Refresh();

        // TO DO: switch to this new weapon
        if (autoSwitch)
        {
            SwitchMode(IndexOfMode(newWeapon.CurrentMode));
        }

        return true;
    }
    public bool Remove(Weapon w)
    {
        bool removed = weapons.Remove(w);
        if (!removed) return false;

        // TO DO: spawn dropped item prefab, if necessary?

        // TO DO: cancel all routines, if they aren't done automatically by destroying the object?
        Destroy(w);

        Refresh();
        return true;
    }

    protected virtual void Refresh()
    {
        // Cull null entries
        weapons.RemoveAll(x => x == null);

        // Sort all weapons
        if (sortByOrderIndex)
        {
            weapons.Sort((a, b) => a.indexOrder.CompareTo(b.indexOrder));

            for (int i = 0; i < weapons.Count; i++)
            {
                weapons[i].transform.SetSiblingIndex(i);
            }
        }

        // Create easy list of all modes
        _allModes.Clear();
        foreach (Weapon w in allWeapons) _allModes.AddRange(w.modes);

        // TO DO: refresh selector

        // TO DO: figure out if the current weapon and mode are still valid. If not, switch to another one.
    }

    #endregion

    public int IndexOfMode(WeaponMode item) => _allModes.IndexOf(item);


}