using CptnFabulous.MiscUtility;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyHealthMeter : MonoBehaviour
{
    public ResourceMeter healthMeter;
    public RectTransform parentRectTransform;
    public CanvasGroup canvasGroup;
    public float observationRange = 50;
    public LayerMask relevantThingDetection = ~0;

    Player p;

    Player player => ComponentUtility.AutoCache(ref p, gameObject, ComponentGetType.InParent);
    Camera camera => player.movement.lookControls.worldViewCamera;


    private void LateUpdate()
    {
        float detectionRange = observationRange;
        if (player.weapons.CurrentWeapon != null)
        {
            RangedAttack attack = player.weapons.CurrentWeapon.CurrentMode as RangedAttack;
            if (attack != null)
            {
                detectionRange = attack.stats.range;
            }
        }

        bool thingObserved = RelevantThingObserved(detectionRange, out RaycastHit observedObject);
        Entity observedEnemy = null;
        Hitbox h = null;
        if (thingObserved)
        {
            h = observedObject.collider.GetComponent<Hitbox>();
            if (h != null) observedEnemy = h.attachedTo;
        }

        bool shouldShow = observedEnemy != null && observedEnemy.health != null && observedEnemy.health.IsAlive;
        canvasGroup.alpha = shouldShow ? 1 : 0;
        if (shouldShow == false) return;

        Health enemyHealth = observedEnemy.health;
        Bounds entityBounds = observedEnemy.bounds;
        Vector3 meterPosition = entityBounds.center + (camera.transform.up * entityBounds.extents.magnitude);

        Vector3 meterScreenPosition = camera.WorldToScreenPoint(meterPosition);
        Vector3 canvasPosition = TransformUtility.ScreenToAnchoredPosition(meterScreenPosition, healthMeter.rectTransform, parentRectTransform);

        healthMeter.safeColour = h.resistances.healthMeterColour;
        healthMeter.criticalColour = h.resistances.healthMeterCriticalColour;
        healthMeter.rectTransform.anchoredPosition = canvasPosition;
        healthMeter.obtainValues = () => enemyHealth.data;
    }

    public bool RelevantThingObserved(float range, out RaycastHit observedObject)
    {
        Debug.DrawRay(camera.transform.position, range * camera.transform.forward);
        return Physics.Raycast(camera.transform.position, camera.transform.forward, out observedObject, range, camera.cullingMask);
    }
}
