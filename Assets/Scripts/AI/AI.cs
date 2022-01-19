using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class AI : MonoBehaviour
{
    public Character info;
    public Health healthData;
    public NavMeshAgent agent;
    public Transform viewAxis;
    

    #region Looking and aiming
    /// <summary>
    /// Represents the current direction the AI is looking in.
    /// </summary>
    Quaternion lookRotation;
    /// <summary>
    /// The point in space the AI looks and aims from.
    /// </summary>
    public Vector3 LookOrigin
    {
        get
        {
            return viewAxis.position;
        }
    }
    /// <summary>
    /// The direction the AI is looking in, converted into an easy Vector3 value.
    /// </summary>
    public Vector3 LookForward
    {
        get
        {
            return lookRotation * Vector3.forward;
        }
    }
    /// <summary>
    /// A direction directly up perpendicular to the direction the AI is looking.
    /// </summary>
    public Vector3 LookUp
    {
        get
        {
            return lookRotation * Vector3.up;
        }
    }

    
    /// <summary>
    /// Continuously rotates AI aim over time to look at position value, at a speed of degreesPerSecond
    /// </summary>
    /// <param name="position"></param>
    /// <param name="degreesPerSecond"></param>
    public void RotateLookTowards(Vector3 position, float degreesPerSecond)
    {
        Quaternion correctRotation = Quaternion.LookRotation(position - LookOrigin, transform.up);
        lookRotation = Quaternion.RotateTowards(lookRotation, correctRotation, degreesPerSecond * Time.deltaTime);
    }

    /// <summary>
    /// Is the AI looking close enough to the position to meet the angle threshold?
    /// </summary>
    /// <param name="position"></param>
    /// <param name="threshold"></param>
    /// <returns></returns>
    public bool IsLookingAt(Vector3 position, float threshold)
    {
        return Vector3.Angle(position - LookOrigin, LookForward) <= threshold;
    }

    /// <summary>
    /// Continuously rotates AI aim to return to looking in the direction it is moving.
    /// </summary>
    /// <param name="degreesPerSecond"></param>
    public void ReturnToNeutralLookPosition(float degreesPerSecond)
    {
        RotateLookTowards(LookOrigin + transform.forward, degreesPerSecond);
    }
    /*
    // Rotates AI aim to look at something, in a specified time.
    public IEnumerator LookAtThing(Vector3 position, float lookTime, AnimationCurve lookCurve)
    {
        inLookIENumerator = true;

        float timer = 0;

        Quaternion originalRotation = lookDirectionQuaternion;

        while (timer < 1)
        {
            timer += Time.deltaTime / lookTime;

            lookDirectionQuaternion = Quaternion.Lerp(originalRotation, Quaternion.LookRotation(position, transform.up), lookCurve.Evaluate(timer));

            yield return null;
        }

        inLookIENumerator = false;
        print("Agent is now looking at " + position + ".");
    }
    */
    #endregion


}
