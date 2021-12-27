using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GUIButtonPrompt : MonoBehaviour
{
    public Image graphic;
    public Text keyName;
    Vector2Int singleKeyDimensions;

    [Header("Sprites")]
    public Sprite keyBackground;
    public Sprite[] controllerSprites;

    /*
    public void Refresh(CustomInput.Button prompt)
    {
        SetMappingGraphic(prompt.keyboardMapping);
    }

    void SetMappingGraphic(CustomInput.ControllerButton button)
    {
        string path = "Graphics/Controller Buttons/ButtonPrompt_";
        path += button.ToString();
        graphic.sprite = Resources.Load<Sprite>(path);

        Rect r = graphic.rectTransform.rect;
        r.width = singleKeyDimensions.x;
        r.height = singleKeyDimensions.y;
        graphic.SetClipRect(r, true);
        keyName.text = "";
    }
    */
    void SetMappingGraphic(KeyCode key)
    {
        int i;
        for (i = 0; i < slightlyWider.Length; i++)
        {
            if (key == slightlyWider[i])
            {
                singleKeyDimensions.x = Mathf.RoundToInt(singleKeyDimensions.x * 1.5f);
                break;
            }
        }
        for (i = 0; i < wide.Length; i++)
        {
            if (key == wide[i])
            {
                singleKeyDimensions.x = Mathf.RoundToInt(singleKeyDimensions.x * 2.5f);
                break;
            }
        }
        for (i = 0; i < tall.Length; i++)
        {
            if (key == tall[i])
            {
                singleKeyDimensions.y *= 2;
                break;
            }
        }
        if (key == KeyCode.Space)
        {
            singleKeyDimensions *= 6;
        }
        Rect r = graphic.rectTransform.rect;
        r.width = singleKeyDimensions.x;
        r.height = singleKeyDimensions.y;
        graphic.SetClipRect(r, true);
        graphic.sprite = keyBackground;
        keyName.text = key.ToString();//System.Enum.GetName(typeof(KeyCode), key);
    }
    static readonly KeyCode[] slightlyWider = new KeyCode[]
        {
            KeyCode.LeftControl,
            KeyCode.RightControl,
            KeyCode.LeftWindows,
            KeyCode.RightWindows,
            KeyCode.LeftAlt,
            KeyCode.RightAlt,
            KeyCode.AltGr,
            KeyCode.Tab,
            KeyCode.LeftApple,
            KeyCode.RightApple,
        };
    static readonly KeyCode[] wide = new KeyCode[]
    {
            KeyCode.LeftShift,
            KeyCode.RightShift,
            KeyCode.CapsLock,
            KeyCode.Backspace,
            KeyCode.Return,
    };
    static readonly KeyCode[] tall = new KeyCode[]
    {
        KeyCode.KeypadPlus,
        KeyCode.KeypadEnter,
    };
}
