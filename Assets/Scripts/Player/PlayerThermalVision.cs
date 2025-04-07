using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class PlayerThermalVision : StateFunction
{
    public Player player;
    
    [Header("Functionality")]
    [SerializeField] Camera[] cameras;
    public int standardRendererIndex = 0;
    public int thermalRendererIndex = 1;
    public float viewRange = 1000;
    public float thermalViewRange = 50;
    /*
    [Header("Stats")]
    public float aimSwayMultiplier = 3;
    */
    [Header("Animations")]
    public float enableTime = 0.5f;
    public float activateThreshold = 0.8f;
    public GameObject goggleVisuals;
    public UnityEvent<float> onLerp;
    public UnityEvent<bool> onActiveSet;

    bool thermalsActive;
    UniversalAdditionalCameraData[] additionalData;

    float t;

    private void Awake()
    {
        player = GetComponentInParent<Player>();
        
        additionalData = new UniversalAdditionalCameraData[cameras.Length];
        for (int i = 0; i < cameras.Length; i++)
        {
            additionalData[i] = cameras[i].GetComponent<UniversalAdditionalCameraData>();
        }
    }
    private void OnEnable()
    {
        player.weaponHandler.disableADS = true;

        Weapon currentWeapon = player.weaponHandler.CurrentWeapon;
        if (currentWeapon == null) return;
        RangedAttack rangedAttack = currentWeapon.CurrentMode as RangedAttack;
        if (rangedAttack == null) return;

        if (rangedAttack.optics == null) return;
        player.weaponHandler.adsHandler.currentlyAiming = false;
    }
    private void OnDisable()
    {
        LerpEffect(0);
        player.weaponHandler.disableADS = false;
    }
    void Update()
    {
        float lerpTarget = thermalsActive ? 1.0f : 0.0f;
        if (t != lerpTarget)
        {
            t = Mathf.MoveTowards(t, lerpTarget, Time.deltaTime / enableTime);
            LerpEffect(t);
        }
    }
    public override IEnumerator AsyncProcedure()
    {
        thermalsActive = true;
        yield return new WaitUntil(() => t >= 1);
    }
    public override IEnumerator AsyncExit()
    {
        thermalsActive = false;
        yield return new WaitUntil(() => t <= 0);
    }

    void LerpEffect(float t)
    {
        bool active = t >= activateThreshold;
        SetThermalsActive(active);
        onLerp.Invoke(t);
    }
    void SetThermalsActive(bool active)
    {
        int rendererIndex = active ? thermalRendererIndex : standardRendererIndex;
        float range = active ? thermalViewRange : viewRange;
        for (int i = 0; i < cameras.Length; i++)
        {
            // Change forward renderer between standard and thermal vision
            additionalData[i].SetRenderer(rendererIndex);
            cameras[i].farClipPlane = range;
        }

        if (goggleVisuals != null) goggleVisuals.gameObject.SetActive(!active);
        onActiveSet.Invoke(active);
    }
}