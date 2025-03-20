using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SmokeCloud : MonoBehaviour
{
    [SerializeField] ParticleSystem particleSystem;

    ParticleSystem.Particle[] particleArray;
    SphereCollider[] colliderArray;

    static Queue<SphereCollider> colliderPool; // Contains all inactive colliders
    static Transform poolParent; // The parent for all colliders, so smoke plumes with rigidbody physics don't behave weirdly

    public bool emitting
    {
        get => particleSystem.isEmitting;
        set
        {
            if (value == emitting) return;

            if (value)
            {
                particleSystem.Play();
                enabled = true;
            }
            else
            {
                particleSystem.Stop();
            }
        }
    }
    int collisionLayer => particleSystem.gameObject.layer;

    private void Awake()
    {
        int maxParticles = particleSystem.main.maxParticles;
        particleArray = new ParticleSystem.Particle[maxParticles];
        colliderArray = new SphereCollider[maxParticles];
        
        ParticleSystem.MainModule main = particleSystem.main;
        main.playOnAwake = false;
        //main.startColor = Color.green;

        //ParticleSystem.CollisionModule collision = particleSystem.collision;
        //collision.collidesWith = MiscFunctions.GetPhysicsLayerMask(collisionLayer);

        // Set up object pool handlers (runs here to minimise the amount of checks)
        if (colliderPool == null) colliderPool = new Queue<SphereCollider>();
        if (poolParent == null)
        {
            poolParent = new GameObject("SmokeCloud collider pool parent").transform;
            Object.DontDestroyOnLoad(poolParent);
        }
    }
    void FixedUpdate()
    {
        if (emitting == false && particleSystem.particleCount <= 0)
        {
            Debug.Log($"Disabling {this} as there is currently no smoke to manage");
            enabled = false;
            return;
        }

        // Obtains the current particle data, to assign the colliders to
        particleSystem.GetParticles(particleArray);
        for (int i = 0; i < colliderArray.Length; i++)
        {
            SphereCollider c = colliderArray[i];

            // If we've accounted for all the particles, place all excess colliders back in the pool
            if (i >= particleSystem.particleCount)
            {
                #region Remove excess collider from array and proceed to next entry
                if (c != null)
                {
                    c.gameObject.SetActive(false);
                    colliderPool.Enqueue(c);
                    colliderArray[i] = null;
                }
                continue;
                #endregion
            }
            else if (c == null) // If a collider is required but not assigned, get one
            {
                #region Get a collider
                // Get one from the pool. If there aren't any more, create one.
                if (colliderPool.Count > 0)
                {
                    c = colliderPool.Dequeue();
                }
                else
                {
                    GameObject g = new GameObject("Smoke Collider");
                    g.transform.parent = poolParent;
                    c = g.AddComponent<SphereCollider>();
                }

                colliderArray[i] = c;
                c.gameObject.layer = collisionLayer;
                c.gameObject.SetActive(true);
                #endregion
            }

            // Assign collider variables
            ParticleSystem.Particle p = particleArray[i];
            c.radius = p.GetCurrentSize(particleSystem) / 2;
            c.transform.position = p.position;
        }
    }
}
