using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AIStatusIcon : MonoBehaviour
{
    [SerializeField] Image graphic;
    [SerializeField] Animator animationController;
    [SerializeField] string trigger = "Display Icon";
    
    private void LateUpdate()
    {
        // I wanted something better than this, to account for there potentially being more than one camera
        Transform ct = Camera.main.transform;
        transform.rotation = Quaternion.LookRotation(transform.position - ct.position, ct.up);
    }
    
    public void TriggerAnimation(Sprite newSprite)
    {
        Debug.Log($"{this}: switching sprite to {newSprite}");
        if (newSprite != null)
        {
            graphic.sprite = newSprite;
            animationController.SetTrigger(trigger);
        }
    }
}
