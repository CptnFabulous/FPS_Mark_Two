using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AITargetingBeam : MonoBehaviour
{
    public AIGunAttack attachedTo;

    [Header("Beam visuals")]
    public LineRenderer beam;
    public float minLength = 0.5f;
    public float maxLength = 10f;

    LayerMask hitDetection => attachedTo.weapon.attackMask;
    
    private void Awake()
    {
        //beam.startColor = attackData.aiming.ai.character.affiliation.colour;
        beam.useWorldSpace = false;
        beam.SetPosition(0, minLength * Vector3.forward);
    }

    private void LateUpdate()
    {
        // Launch a simple raycast to check roughly what the attack will hit
        Ray ray = new Ray(transform.position, transform.forward);
        bool raycastHit = AIAction.RaycastWithExceptions(ray, out RaycastHit rh, maxLength, hitDetection, attachedTo.rootAI.colliders);

        // Get the distance that the beam travels (use full length if beam hits nothing)
        float distance = raycastHit ? rh.distance : maxLength;

        // Disable beam if distance is too short to bother rendering
        beam.enabled = distance > minLength;
        if (beam.enabled == false) return;

        // Set beam length
        beam.SetPosition(1, distance * Vector3.forward);
    }



    // These two functions may need to be replaced at some point. I'm not exactly sure how to handle playing and stopping, but I know I may want more elaborate animations.
    public void Play()
    {
        gameObject.SetActive(true);
    }
    public void Stop()
    {
        gameObject.SetActive(false);
    }
}
