using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class CustomInput
{
    /*
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
    */
    public struct Button
    {
        public KeyCode keyboardMapping;
        public ControllerButton controllerMapping;
        public bool Pressed
        {
            get
            {
                return Input.GetKeyDown(keyboardMapping);
            }
        }
        public bool Held
        {
            get
            {
                return Input.GetKey(keyboardMapping);
            }
        }
        public bool Released
        {
            get
            {
                return Input.GetKeyUp(keyboardMapping);
            }
        }
        public Button(KeyCode key, ControllerButton button)
        {
            keyboardMapping = key;
            controllerMapping = button;
        }
    }
    public struct SingleAxis
    {
        public KeyCode minus;
        public KeyCode plus;
        public string analogAxis;

        public float value
        {
            get
            {
                float v = Input.GetAxis(analogAxis);
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
        public SingleAxis(string name)
        {
            minus = KeyCode.None;
            plus = KeyCode.None;
            analogAxis = name;
        }
        public SingleAxis(KeyCode _minus, KeyCode _plus)
        {
            minus = _minus;
            plus = _plus;
            analogAxis = "";
        }
    }
    public struct DualAxis
    {
        public KeyCode minusX;
        public KeyCode plusX;
        public string analogAxisX;
        public KeyCode minusY;
        public KeyCode plusY;
        public string analogAxisY;
        public Vector2 value
        {
            get
            {
                Vector2 value = new Vector2(Input.GetAxis(analogAxisX), Input.GetAxis(analogAxisY));
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

        public DualAxis(string x, string y)
        {
            minusX = KeyCode.None;
            plusX = KeyCode.None;
            minusY = KeyCode.None;
            plusY = KeyCode.None;
            analogAxisX = x;
            analogAxisY = y;
        }
        public DualAxis(KeyCode _minusX, KeyCode _plusX, KeyCode _minusY, KeyCode _plusY)
        {
            minusX = _minusX;
            plusX = _plusX;
            minusY = _minusY;
            plusY = _plusY;
            analogAxisX = "";
            analogAxisY = "";
        }
    }

    public enum AxisInput
    {
        MouseX,
        MouseY,
        MouseScrollWheel,
        LeftStickX,
        LeftStickY,
        RightStickX,
        RightStickY,
        LeftTrigger,
        RightTrigger,
        LeftAndRightTriggers,
        GyroX,
        GyroY,
        GyroZ,
    }
    static readonly string[] axes = new string[]
    {
        "Mouse X",
        "Mouse Y",
        "Mouse ScrollWheel",
        "Left Stick X",
        "Left Stick Y",
        "Right Stick X",
        "Right Stick Y",
        "Left Trigger",
        "Right Trigger",
        "Left And Right Triggers",
        "Gyro X",
        "Gyro Y",
        "Gyro Z",
    };
    public static float GetInput(AxisInput input)
    {
        return Input.GetAxis(axes[(int)input]);
    }


    public enum ControllerButton
    {
        Start,
        Select,
        North,
        South,
        East,
        West,
        DpadUp,
        DpadDown,
        DpadLeft,
        DpadRight,
        LeftBumper,
        LeftTrigger,
        RightBumper,
        RightTrigger,
        LeftStickClick,
        RightStickClick,
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
}
