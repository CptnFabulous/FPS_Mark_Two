using CptnFabulous.MiscUtility;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class LaunchableProp : MonoBehaviour
{
    float stoppedThreshold = 0.25f;

    Rigidbody _rb;
    NavMeshObstacle _obstacle;
    bool obstacleCheckPerformed = false;

    public Rigidbody rb => ComponentUtility.AutoCache(ref _rb, gameObject);
    public NavMeshObstacle obstacleData
    {
        get
        {
            // If a check has already been performed, return that value
            if (obstacleCheckPerformed) return _obstacle;
            // Do a check to see what's there
            _obstacle = GetComponent<NavMeshObstacle>();
            obstacleCheckPerformed = true;
            return _obstacle;
        }
    }

    private void FixedUpdate()
    {
        // Disable obstacle data if object has been launched through the air
        if (obstacleData != null)
        {
            obstacleData.enabled = rb.velocity.magnitude < stoppedThreshold;
        }
    }
}
