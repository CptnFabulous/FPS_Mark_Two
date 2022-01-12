using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;

public class BindingOption : MonoBehaviour//, ISelectHandler, IPointerEnterHandler
{
    public Text name;
    public GUIButtonPrompt shownBinding;
    public Button buttonToRebind;
    public RectTransform rectTransform;




    InputAction action;
    int bindingIndex;
    string newBindingPath;
    ControlOptions baseMenu;

    InputBinding binding
    {
        get
        {
            return action.bindings[bindingIndex];
        }
    }
    static readonly List<char> bindingPunctuationToAddSpaces = new List<char>
    {
        '&'
    };
    static readonly List<char> bindingPunctuationToRemove = new List<char>
    {
        ';'
    };

    public void SetupBinding(InputAction newAction, int newBindingIndex, ControlOptions menu)
    {
        action = newAction;
        bindingIndex = newBindingIndex;
        baseMenu = menu;

        string displayBindingGroups = MiscFunctions.FormatNameForPresentation(binding.groups, bindingPunctuationToAddSpaces, bindingPunctuationToRemove);

        if (binding.isPartOfComposite)
        {
            string displayName = MiscFunctions.FormatNameForPresentation(binding.name);
            name.text = displayName + " (" + displayBindingGroups + ")";
        }
        else
        {
            name.text = displayBindingGroups;
        }
        
        shownBinding.enabled = false;
    }

    public void Refresh()
    {
        newBindingPath = binding.effectivePath;
        shownBinding.Refresh(binding);
    }
}
