using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CptnFabulous.ObjectPool;

public class SmokeCloud : MonoBehaviour
{
    static List<SmokeCloud> _activeClouds = new List<SmokeCloud>();

    public static IReadOnlyList<SmokeCloud> activeClouds => _activeClouds;
    
    [SerializeField] ParticleSystem particleSystem;
    public SmokeParticleDensityController densityControllerPrefab;
    [HideInInspector, NonSerialized] public ParticleSystem.Particle[] particleArray;

    SphereCollider[] colliderArray;
    [HideInInspector, NonSerialized] public Vector3[] particleOffsetResolvers;

    static SphereCollider smokeColliderPrefab;
    static Transform activeSmokeCloudParent;
    static SmokeParticleDensityController densityControllerSingleton;

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
    public ParticleSystem particleEmitter => particleSystem;
    //public ParticleSystem.Particle[] particles => particleArray;
    public int numberOfParticles => particleSystem.particleCount;
    int collisionLayer => particleSystem.gameObject.layer;

    private void Awake()
    {
        // Check that a density controller exists. If not, spawn one
        if (densityControllerSingleton == null)
        {
            densityControllerSingleton = FindAnyObjectByType<SmokeParticleDensityController>();
        }
        if (densityControllerSingleton == null)
        {
            densityControllerSingleton = Instantiate(densityControllerPrefab);
            DontDestroyOnLoad(densityControllerSingleton);
        }

        // Set up particle arrays
        int maxParticles = particleSystem.main.maxParticles;
        particleArray = new ParticleSystem.Particle[maxParticles];
        colliderArray = new SphereCollider[maxParticles];
        particleOffsetResolvers = new Vector3[maxParticles];
        
        ParticleSystem.MainModule main = particleSystem.main;
        main.playOnAwake = false;
        //ParticleSystem.CollisionModule collision = particleSystem.collision;
        //collision.collidesWith = MiscFunctions.GetPhysicsLayerMask(collisionLayer);
    }
    void FixedUpdate()
    {
        if (emitting == false && particleSystem.particleCount <= 0)
        {
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
                ObjectPool.DismissObject(c);
                colliderArray[i] = null;
                continue;
            }

            // If a collider is required but not assigned, get one
            if (c == null)
            {
                c = SpawnParticleCollider();
                colliderArray[i] = c;
                c.gameObject.layer = collisionLayer;
            }

            // Assign collider variables
            ParticleSystem.Particle p = particleArray[i];
            c.radius = p.GetCurrentSize(particleSystem) / 2;
            c.transform.position = p.position;
        }
    }
    private void OnDisable()
    {
        // Ensure all colliders are deactivated and returned to the pool
        for (int i = 0; i < colliderArray.Length; i++)
        {
            ObjectPool.DismissObject(colliderArray[i]);
            colliderArray[i] = null;
        }
    }

    static SphereCollider SpawnParticleCollider()
    {
        if (smokeColliderPrefab == null)
        {
            smokeColliderPrefab = new GameObject("Smoke Collider").AddComponent<SphereCollider>();
            Object.DontDestroyOnLoad(smokeColliderPrefab);
        }
        if (activeSmokeCloudParent == null)
        {
            activeSmokeCloudParent = new GameObject("Active Smoke Cloud Parent").transform;
            Object.DontDestroyOnLoad(activeSmokeCloudParent);
        }

        SphereCollider c = ObjectPool.RequestObject(smokeColliderPrefab);
        // Contains active particle colliders so they behave consistently
        // (regardless of what crazy stuff the parent particle system is doing)
        c.transform.SetParent(activeSmokeCloudParent);
        return c;
    }
}