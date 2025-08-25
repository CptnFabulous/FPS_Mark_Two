using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(RectTransform))]
public class ResourceMeter : ResourceDisplay
{
    [Header("Main meter")]
    [SerializeField] Image currentMeter;
    public Color safeColour = Color.green;
    public Color criticalColour = Color.red;

    [Header("'Previous' meter")]
    [SerializeField] Image previousMeter;
    public float barChangeSpeed = 0.1f;

    public RectTransform rectTransform { get; private set; }
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

    private void Awake() => rectTransform = GetComponent<RectTransform>();
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


