using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AIStatusIcon : MonoBehaviour
{
    [SerializeField] Image graphic;
    [SerializeField] Animator animationController;
    [SerializeField] string trigger = "Display Icon";
    
    public void TriggerAnimation(Sprite newSprite)
    {
        if (newSprite != null)
        {
            graphic.sprite = newSprite;
            animationController.SetTrigger(trigger);
        }
    }
}
