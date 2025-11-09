using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Detection Profile", menuName = "ScriptableObjects/Detection Profile", order = 1)]
public class DetectionProfile : ScriptableObject
{
    [SerializeField] LayerMask _mask = ~0;

    public LayerMask mask => _mask;
}
