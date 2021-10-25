using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    public float weight = 1;
    public float diameter = 0.05f;
    public float velocity = 100;
    public LayerMask detection = ~0;

    RaycastHit thingHit;
    
    
    
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        float detectionLength = velocity * Time.deltaTime;
        if (Physics.SphereCast(transform.position, diameter, transform.forward, out thingHit, detectionLength, detection))
        {
            OnHit(thingHit);
        }
        else
        {
            Move();
        }
    }

    private void Move()
    {
        
    }

    public void OnHit(RaycastHit thingHit)
    {

    }
}
