using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ICharacterLookController
{
    public bool active { get; set; }
    //public Transform lookTransform { get; }
    //public Vector3 lookDirection { get; }
    public Quaternion lookRotation { get; set; }
}