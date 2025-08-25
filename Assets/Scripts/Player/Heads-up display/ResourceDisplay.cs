using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public abstract class ResourceDisplay : MonoBehaviour
{
    [Header("Text display")]
    [SerializeField] Text amount;
    public string decimalFormatting = "0";

    [Header("Additional animations")]
    [SerializeField] Animator animationController;
    [SerializeField] string current = "Current";
    [SerializeField] string critical = "Critical";
    [SerializeField] string full = "Full";
    [SerializeField] string depleted = "Depleted";

    public System.Func<Resource> obtainValues;
    Resource lastObtainedValues;

    protected virtual void LateUpdate()
    {
        if (obtainValues == null) return;
        Resource currentValues = obtainValues.Invoke();
        if (currentValues != lastObtainedValues) Refresh(currentValues);
    }

    protected virtual void Refresh(Resource values)
    {
        // Set amount as text
        if (amount != null) amount.text = values.current.ToString(decimalFormatting);

        if (animationController != null)
        {
            animationController.SetFloat(current, values.current);
            animationController.SetBool(critical, values.isCritical);
            animationController.SetBool(full, values.isFull);
            animationController.SetBool(depleted, values.isDepleted);
        }
    }
}