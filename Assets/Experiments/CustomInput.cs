using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class CustomInput
{
    

    public struct Button
    {
        public KeyCode mapping;
        public bool pressed
        {
            get
            {
                return Input.GetKeyDown(mapping);
            }
        }
        public bool held
        {
            get
            {
                return Input.GetKey(mapping);
            }
        }
        public bool released
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
}
