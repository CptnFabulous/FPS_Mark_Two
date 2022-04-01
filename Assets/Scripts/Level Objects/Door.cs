using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Door : MonoBehaviour
{
    [SerializeField] bool isOpen;
    public UnityEvent onOpen;
    public UnityEvent onClose;

    public bool IsOpen
    {
        get
        {
            return isOpen;
        }
        set
        {
            if (value == isOpen)
            {
                return;
            }
            SetOpen(value);
        }
    }
    private void OnValidate()
    {
        SetOpen(isOpen);
    }
    void SetOpen(bool open)
    {
        isOpen = open;
        if (isOpen)
        {
            onOpen.Invoke();
        }
        else
        {
            onClose.Invoke();
        }
    }
}
