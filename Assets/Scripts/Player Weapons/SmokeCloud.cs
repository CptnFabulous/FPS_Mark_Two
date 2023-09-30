using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SmokeCloud : MonoBehaviour
{
    public ParticleSystem particleSystem;
    
    ParticleSystem.Particle[] particleArray;
    SphereCollider[] colliderArray;

    private void Awake()
    {
        int maxParticles = particleSystem.main.maxParticles;
        particleArray = new ParticleSystem.Particle[maxParticles];
        colliderArray = new SphereCollider[maxParticles];
    }
    void FixedUpdate()
    {
        particleSystem.GetParticles(particleArray);
        for (int i = 0; i < colliderArray.Length; i++)
        {
            // Check how many particles need to have colliders assigned
            bool colliderRequired = i < particleSystem.particleCount;
            ParticleSystem.Particle p = particleArray[i];
            SphereCollider c = colliderArray[i];

            if (colliderRequired && c == null)
            {
                GameObject g = new GameObject("Smoke Collider #" + (i + 1));
                g.transform.parent = particleSystem.transform;
                g.layer = particleSystem.gameObject.layer;
                c = g.AddComponent<SphereCollider>();
                colliderArray[i] = c;
            }

            if (c != null)
            {
                c.gameObject.SetActive(colliderRequired);
                if (colliderRequired)
                {
                    c.radius = p.GetCurrentSize(particleSystem) / 2;
                    c.transform.position = p.position;
                }
            }
        }
    }
}
