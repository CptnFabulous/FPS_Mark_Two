using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CptnFabulous.ObjectPool;
using UnityEngine.Rendering.Universal;

[CreateAssetMenu(fileName = "New Impact Effect", menuName = "ScriptableObjects/Impact Effect", order = 0)]
public class ImpactEffect : ScriptableObject
{
    // List of terrain types
    // For each terrain type:
    // Particle effect
    // Impact noise (DiegeticSound)

    public DiegeticSound defaultSound;
    public ParticleSystem defaultImpactEffect;
    //public Sprite defaultDecal;
    public Material defaultDecalMaterial;
    public Vector2 decalSize = new Vector2(0.02f, 0.02f);
    public bool playSoundAtMaxVolume = false;

    //static SpriteRenderer decalPrefab;
    static DecalProjector decalProjector;
    static int maxNumberOfSpawnedEffects = 100;

    public void Play(GameObject surfaceCollider, Entity sourceEntity, Vector3 point, Vector3 impactDirection, Vector3 normal, Vector3 up, float intensity = 1)
    {
        // Determine effects based on surface (currently doesn't have support for multiple sound types)
        ParticleSystem effect = defaultImpactEffect;
        DiegeticSound sound = defaultSound;
        //Sprite decal = defaultDecal;
        Material decalMaterial = defaultDecalMaterial;

        // Instantiate impact effect at surface
        if (effect != null)
        {
            //Vector3 effectDirection = normal;
            Vector3 effectDirection = Vector3.Reflect(impactDirection, normal);
            ParticleSystem effectToSpawn = ObjectPool.RequestObject(effect, true, maxNumberOfSpawnedEffects);
            StickObjectToSurface(effectToSpawn.transform, surfaceCollider.transform, point, effectDirection, up, 0);
            // TO DO: use intensity to determine size of particles
            effectToSpawn.Play();
        }

        // Play sound effect at point
        if (sound != null) sound.Play(point, sourceEntity, intensity, playSoundAtMaxVolume);

        // Stick decal
        bool isNotPlayer = EntityCache<Player>.GetEntity(surfaceCollider) == null;
        /*
        if (decal != null && isNotPlayer)
        {
            if (decalPrefab == null)
            {
                decalPrefab = new GameObject($"Decal Renderer").AddComponent<SpriteRenderer>();
                decalPrefab.gameObject.SetActive(false);
                DontDestroyOnLoad(decalPrefab);
            }

            SpriteRenderer sr = ObjectPool.RequestObject(decalPrefab, true, maxNumberOfSpawnedEffects);
            sr.sprite = decal;
            sr.name = $"Decal ({name})";
            sr.gameObject.layer = surfaceCollider.layer;

            //Vector3 pointToStickDecal = point + (normal.normalized * 0.01f);
            StickObjectToSurface(sr.transform, surfaceCollider.transform, point, normal, up, 0.01f);
        }
        */

        if (decalMaterial != null && isNotPlayer)
        {
            if (decalProjector == null)
            {
                decalProjector = new GameObject("Decal Projector").AddComponent<DecalProjector>();
                decalProjector.gameObject.SetActive(false);
                DontDestroyOnLoad(decalProjector);
            }

            DecalProjector dp = ObjectPool.RequestObject(decalProjector, true, maxNumberOfSpawnedEffects);
            //Vector2 decalSize = decal.pixelsPerUnit * decal.textureRect.size;

            dp.size = new Vector3(decalSize.x, decalSize.y, 1);
            dp.material = decalMaterial;
            dp.name = $"Decal ({name})";
            dp.gameObject.layer = surfaceCollider.layer;

            //Vector3 pointToStickDecal = point + (normal.normalized * 0.01f);
            StickObjectToSurface(dp.transform, surfaceCollider.transform, point, -normal, up, -0.01f);
        }
    }

    public static void StickObjectToSurface(Transform objectToStick, RaycastHit surface, Vector3 worldUp, float distanceOffSurface)
    {
        StickObjectToSurface(objectToStick, surface.transform, surface.point, surface.normal, worldUp, distanceOffSurface);
    }
    public static void StickObjectToSurface(Transform objectToStick, Transform surface, Vector3 point, Vector3 forward, Vector3 up, float distanceOffSurface)
    {
        objectToStick.position = point + (forward * distanceOffSurface);
        objectToStick.rotation = Quaternion.LookRotation(forward, up);
        objectToStick.parent = surface;
    }
}