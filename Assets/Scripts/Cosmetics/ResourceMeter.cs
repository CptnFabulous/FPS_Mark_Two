using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(RectTransform))]
public class ResourceMeter : MonoBehaviour
{
    public Image currentMeter;
    public Image previousMeter;
    public float barChangeSpeed = 0.1f;
    public Color safeColour = Color.green;
    public Color criticalColour = Color.red;
    public Text amount;

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

    private void Awake() => rectTransform = GetComponent<RectTransform>();
    private void OnEnable() => previousFill = currentFill;

    public void Refresh(Resource values)
    {
        if (amount != null)
        {
            amount.text = values.current.ToString();
        }

        currentFill = values.current / values.max;
        currentMeter.color = values.isCritical ? criticalColour : safeColour;
    }

    private void LateUpdate()
    {
        if (previousFill != currentFill)
        {
            bool lowerThan = currentFill < previousFill;
            float fillSpeed = lowerThan ? Time.deltaTime * barChangeSpeed : Mathf.Infinity;
            previousFill = Mathf.MoveTowards(previousMeter.fillAmount, currentFill, fillSpeed);
        }
    }
}
