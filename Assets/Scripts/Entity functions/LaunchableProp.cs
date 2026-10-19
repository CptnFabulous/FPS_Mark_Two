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

    public Rigidbody rb => ComponentUtility.AutoCache(ref _rb, gameObject);
    public NavMeshObstacle obstacleData
    {
        get
        {
            if (_obstacle != null) return _obstacle;

            // Check for one that's already been added
            if (TryGetComponent(out _obstacle)) return _obstacle;

            _obstacle = gameObject.AddComponent<NavMeshObstacle>();
            return _obstacle;
        }
    }

    private void FixedUpdate()
    {
        obstacleData.enabled = rb.velocity.magnitude < stoppedThreshold;
    }
}
