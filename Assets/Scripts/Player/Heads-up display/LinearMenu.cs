using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class LinearMenu : MonoBehaviour
{
    [SerializeField] RectTransform[] options;
    [SerializeField] RectTransform highlight;
    public UnityEvent<int> onValueChanged;

    public void SetOption(int index)
    {
        highlight.position = options[index].position;
        highlight.rotation = options[index].rotation;
        onValueChanged.Invoke(index);
    }
}