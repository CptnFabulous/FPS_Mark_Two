using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(RectTransform))]
public class ResourceMeter : ResourceDisplay
{
    [Header("Main meter")]
    [SerializeField] Image currentMeter;
    [SerializeField] Color _safeColour = Color.green;
    [SerializeField] Color _criticalColour = Color.red;

    [Header("'Previous' meter")]
    [SerializeField] Image previousMeter;
    public float barChangeSpeed = 0.1f;

    public override Color safeColour
    {
        get => _safeColour;
        set => _safeColour = value;
    }
    public override Color criticalColour
    {
        get => _criticalColour;
        set => _criticalColour = value;
    }
    float currentFill
    {
        get => currentMeter.fillAmount;
        set => currentMeter.fillAmount = value;
    }
    float previousFill
    {
        get => previousMeter.fillAmount;
        set => previousMeter.fillAmount = value;
    }

    protected override void Refresh(Resource values)
    {
        // Update meter fill and colour
        currentFill = values.current / values.max;
        currentMeter.color = values.isCritical ? criticalColour : safeColour;

        base.Refresh(values);
    }

    private void OnEnable() => previousFill = currentFill;
    protected override void LateUpdate()
    {
        base.LateUpdate();

        if (previousFill == currentFill) return;

        // If current value is lower, have secondary fill shrink over time. If greater, have it change instantly.
        float fillSpeed = (currentFill < previousFill) ? (Time.deltaTime * barChangeSpeed) : Mathf.Infinity;
        previousFill = Mathf.MoveTowards(previousMeter.fillAmount, currentFill, fillSpeed);
    }
}


