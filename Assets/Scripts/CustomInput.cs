using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class CustomInput
{
    public struct Button
    {
        string inputName;

        public bool Pressed
        {
            get
            {
                return Input.GetButtonDown(inputName);
            }
        }
        public bool Held
        {
            get
            {
                return Input.GetButton(inputName);
            }
        }
        public bool Released
        {
            get
            {
                return Input.GetButtonUp(inputName);
            }
        }

        public Button(string name)
        {
            inputName = name;
        }
    }
    public struct Key
    {
        public KeyCode mapping;
        public bool Pressed
        {
            get
            {
                return Input.GetKeyDown(mapping);
            }
        }
        public bool Held
        {
            get
            {
                return Input.GetKey(mapping);
            }
        }
        public bool Released
        {
            get
            {
                return Input.GetKeyUp(mapping);
            }
        }
    }
    public struct SingleAxis
    {
        public KeyCode minus;
        public KeyCode plus;
        public float value
        {
            get
            {
                float v = 0;
                if (Input.GetKey(minus))
                {
                    v -= 1;
                }
                if (Input.GetKey(plus))
                {
                    v += 1;
                }
                return v;
            }
        }
    }
    public struct DualAxis
    {
        public KeyCode minusX;
        public KeyCode plusX;
        public KeyCode minusY;
        public KeyCode plusY;
        public Vector2 value
        {
            get
            {
                Vector2 value = Vector2.zero;
                if (Input.GetKey(minusX))
                {
                    value.x -= 1;
                }
                if (Input.GetKey(plusX))
                {
                    value.x += 1;
                }
                if (Input.GetKey(minusY))
                {
                    value.y -= 1;
                }
                if (Input.GetKey(plusY))
                {
                    value.y += 1;
                }
                return value;
            }
        }
    }

    public static bool SetPlayerAbilityState(bool currentState, Button button, bool toggle)
    {
        if (toggle == false)
        {
            currentState = button.Held;
        }
        else if (button.Pressed)
        {
            currentState = !currentState;
        }

        return currentState;
    }
    public static bool SetPlayerAbilityState(bool currentState, Key button, bool toggle)
    {
        if (toggle == false)
        {
            currentState = button.Held;
        }
        else if (button.Pressed)
        {
            currentState = !currentState;
        }

        return currentState;
    }
}