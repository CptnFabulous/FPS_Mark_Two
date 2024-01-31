using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using static UnityEngine.UI.Image;

public class DiegeticLightSource : MonoBehaviour
{
    public static List<DiegeticLightSource> sourcesInScene => s ??= new List<DiegeticLightSource>();
    static List<DiegeticLightSource> s;

    static float distanceCheckForDirectionalLight = 500;

    Light l;

    Light lightSource => l ??= GetComponent<Light>();

    private void Awake() => sourcesInScene.Add(this);
    private void OnDestroy() => sourcesInScene.Remove(this);

    /// <summary>
    /// How much is an entity being illuminated by a light? Zero means it's not inside the light at all.
    /// </summary>
    /// <param name="targetEntity"></param>
    /// <returns></returns>
    public static float IsIlluminatingEntity(Light lightSource, Entity targetEntity)
    {
        Transform transform = lightSource.transform;
        Vector3 targetPosition = targetEntity.bounds.center;

        Vector3 origin = transform.position;
        Vector3 direction = targetPosition - origin;
        //float range = lightSource.range;

        if (lightSource.type == LightType.Spot)
        {
            direction = MiscFunctions.ClampDirection(direction, transform.forward, lightSource.spotAngle / 2);
        }
        else if (lightSource.type == LightType.Directional)
        {
            direction = transform.forward;
            //range = distanceCheckForDirectionalLight;
            origin = targetPosition + (distanceCheckForDirectionalLight * -transform.forward);
        }

        // Launch a raycast towards the target to see if anything blocks it, or if it's out of range.

        bool lightReachesTarget = AIAction.LineOfSight(origin, origin + direction, lightSource.cullingMask, targetEntity.colliders);
        //bool lightReachesTarget = Physics.Raycast(origin, direction, out RaycastHit rh, range, lightSource.cullingMask) && rh.collider.GetComponentInParent<Entity>() == targetEntity;
        if (lightReachesTarget == false) return 0;

        return lightSource.intensity * MiscFunctions.InverseSquareValueMultiplier(direction.magnitude);
    }
    public static float EntityIllumination(Entity targetEntity)
    {
        float totalIllumination = 0;
        foreach (DiegeticLightSource light in sourcesInScene)
        {
            totalIllumination += IsIlluminatingEntity(light.lightSource, targetEntity);
        }
        return totalIllumination;
    }
    public static bool IsEntityIlluminated(Entity targetEntity, float minIntensity)
    {
        float totalIllumination = 0;
        foreach (DiegeticLightSource light in sourcesInScene)
        {
            totalIllumination += IsIlluminatingEntity(light.lightSource, targetEntity);
            if (totalIllumination >= minIntensity)
            {
                return true;
            }
        }
        return false;
    }
}
