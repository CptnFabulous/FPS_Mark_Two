using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AITargetingBeam : MonoBehaviour
{
    public LayerMask beamCollision = ~0;
    public float minLength = 0.5f;
    public float maxLength = 10f;

    LineRenderer beam;
    Vector3[] points = new Vector3[2];


    private void Awake()
    {
        beam = GetComponent<LineRenderer>();
        beam.useWorldSpace = true;
        //beam.startColor = attackData.aiming.ai.character.affiliation.colour;
        enabled = false;
    }

    private void OnDisable()
    {
        beam.enabled = false;
    }

    private void LateUpdate()
    {
        bool raycastHit = Physics.Raycast(transform.position, transform.forward, out RaycastHit rh, maxLength, beamCollision);
        beam.enabled = raycastHit == false || rh.distance > minLength;
        if (beam.enabled)
        {
            points[0] = raycastHit ? rh.point : (maxLength * transform.forward) + transform.position;
            points[1] = (minLength * transform.forward) + transform.position;
            beam.SetPositions(points);
        }
    }
}
