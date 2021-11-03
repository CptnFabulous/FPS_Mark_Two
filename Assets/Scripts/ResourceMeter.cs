using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ResourceMeter : MonoBehaviour
{
    public Text amount;
    public Image currentMeter;
    public Image previousMeter;
    public float barChangeSpeed = 0.1f;
    public Color safeColour = Color.green;
    public Color criticalColour = Color.red;
    IEnumerator transition;

    public void Refresh(Resource values)
    {
        amount.text = values.current.ToString();
        currentMeter.fillAmount = values.current / values.max;
        if (values.isCritical)
        {
            currentMeter.color = criticalColour;
        }
        else
        {
            currentMeter.color = safeColour;
        }

        if (transition == null)
        {
            transition = Transition();
            StartCoroutine(transition);
        }
    }

    public IEnumerator Transition()
    {
        while (previousMeter.fillAmount != currentMeter.fillAmount)
        {
            if (previousMeter.fillAmount > currentMeter.fillAmount)
            {
                previousMeter.fillAmount -= Time.deltaTime * barChangeSpeed;
            }
            else
            {
                previousMeter.fillAmount = currentMeter.fillAmount;
            }
            yield return null;
        }
        transition = null;
    }
}
