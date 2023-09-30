using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(RectTransform))]
public class ResourceMeter : MonoBehaviour
{
    [Header("Main meter")]
    public Image currentMeter;
    public Color safeColour = Color.green;
    public Color criticalColour = Color.red;

    [Header("'Previous' meter")]
    public Image previousMeter;
    public float barChangeSpeed = 0.1f;

    [Header("Text display")]
    public Text amount;
    public string decimalFormatting = "0";

    [Header("Additional animations")]
    [Tooltip("Current = 'Current', critical = 'Critical', full = 'Full', depleted = 'Depleted'")]
    public Animator animationController;

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

    public void Refresh(Resource values)
    {
        // Set amount as text
        if (amount != null) amount.text = values.current.ToString(decimalFormatting);
        // Update meter fill and colour
        currentFill = values.current / values.max;
        currentMeter.color = values.isCritical ? criticalColour : safeColour;

        if (animationController != null)
        {
            animationController.SetFloat("Current", values.current);
            animationController.SetBool("Critical", values.isCritical);
            animationController.SetBool("Full", values.isFull);
            animationController.SetBool("Depleted", values.isDepleted);
        }
    }

    private void Awake() => rectTransform = GetComponent<RectTransform>();
    private void OnEnable() => previousFill = currentFill;
    private void LateUpdate()
    {
        if (previousFill != currentFill)
        {
            // If current value is lower, have secondary fill shrink over time. If greater, have it change instantly.
            float fillSpeed = (currentFill < previousFill) ? (Time.deltaTime * barChangeSpeed) : Mathf.Infinity;
            previousFill = Mathf.MoveTowards(previousMeter.fillAmount, currentFill, fillSpeed);
        }
    }
}
