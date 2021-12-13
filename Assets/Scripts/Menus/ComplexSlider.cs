using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ComplexSlider : MonoBehaviour
{
    public Slider slider;
    public InputField textBox;

    private void Awake()
    {
        slider.onValueChanged.AddListener(UpdateTextBox);
        textBox.onEndEdit.AddListener(UpdateSlider);
        if (slider.wholeNumbers)
        {
            textBox.contentType = InputField.ContentType.IntegerNumber;
        }
        else
        {
            textBox.contentType = InputField.ContentType.DecimalNumber;
        }
    }

    private void OnEnable()
    {
        UpdateTextBox(slider.value);
    }

    void UpdateTextBox(float value)
    {
        textBox.text = value.ToString();
    }

    void UpdateSlider(string text)
    {
        float value = float.Parse(text);
        slider.value = value;
        slider.onValueChanged.Invoke(slider.value);
    }
}
