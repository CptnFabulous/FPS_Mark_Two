using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GUICompass : MonoBehaviour
{
    public Transform playerViewTransform;
    public Transform objectSpace;

    [Header("GUI")]
    [SerializeField] Transform rotater;
    [SerializeField] Color inFrontColour = Color.white;
    [SerializeField] Color behindColour = Color.clear;

    Graphic[] childRenderers;

    Vector3 forward => objectSpace != null ? objectSpace.forward : Vector3.forward;
    Vector3 up => objectSpace != null ? objectSpace.up : Vector3.up;

    private void Awake()
    {
        childRenderers = rotater.GetComponentsInChildren<Graphic>();
    }
    public void LateUpdate()
    {
        Vector3 playerDirection = Vector3.ProjectOnPlane(playerViewTransform.forward, up);
        float angle = Vector3.SignedAngle(forward, playerDirection, up);
        // 0 is north
        // 90 is east
        // -90 is west
        // 180 or -180 is south
        rotater.localRotation = Quaternion.Euler(0, -angle, 0);

        Vector3 graphicForward = rotater.parent.forward;
        foreach (Graphic child in childRenderers)
        {
            float dot = Vector3.Dot(graphicForward, child.transform.forward);
            bool inFront = dot >= 0;
            child.color = inFront ? inFrontColour : behindColour;
        }
    }
}