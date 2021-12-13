using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FieldOfView : MonoBehaviour
{


    public Collider testCollider;
    public LayerMask mask = ~0;

    float lastTime;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Time.time - lastTime > 3)
        {
            if (AI.ComplexLineOfSightCheck(transform.position, testCollider, 0.25f, out RaycastHit hit, mask))
            {
                Debug.DrawRay(testCollider.bounds.center, Vector3.up * 5, Color.magenta, 3);
            }
            
            lastTime = Time.time;
        }
    }
}
