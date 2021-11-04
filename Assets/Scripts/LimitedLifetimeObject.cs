using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LimitedLifetimeObject : MonoBehaviour
{
    public float lifetime = 20;
    float timer;
    // Update is called once per frame
    void Update()
    {
        timer += Time.deltaTime;
        if (timer > lifetime)
        {
            Destroy(gameObject);
        }
    }
}
