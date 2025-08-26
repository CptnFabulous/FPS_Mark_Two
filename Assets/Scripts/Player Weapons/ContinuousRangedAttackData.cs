using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class ContinuousRangedAttackData : RangedAttackFiringData
{
    [Header("Muzzle alignment")]
    [SerializeField] LayerMask _hitDetection = ~0;
    public float muzzleRotationShiftPerSecond = 30;


    public UnityEvent<bool> onActiveSet;
    public UnityEvent onEnable;
    public UnityEvent onDisable;


    public override LayerMask hitDetection => _hitDetection;

    public override int damage => 0;

    private void OnEnable()
    {
        onActiveSet.Invoke(true);
        onEnable.Invoke();
    }
    private void OnDisable()
    {
        onActiveSet.Invoke(false);
        onDisable.Invoke();
    }

    void Update()
    {
        if (user == null) return;

        Vector3 origin = user.LookTransform.position;
        Vector3 aimDirection = user.aimDirection;
        Vector3 worldUp = user.LookTransform.up;


        Vector3 castDirection = Quaternion.LookRotation(aimDirection, worldUp) * Vector3.forward;

        WeaponUtility.CalculateObjectLaunch(origin, muzzle.position, castDirection, range, hitDetection, user.colliders, out Vector3 launchDirection, out Vector3 hitPoint, out RaycastHit rh, out bool behindMuzzle);

        Quaternion desiredMuzzleRotation = Quaternion.LookRotation(launchDirection, worldUp);
        muzzle.rotation = Quaternion.RotateTowards(muzzle.rotation, desiredMuzzleRotation, muzzleRotationShiftPerSecond * Time.deltaTime);
        //muzzle.forward = Vector3.RotateTowards(muzzle.forward, launchDirection, muzzleRotationShiftPerSecond * Mathf.Deg2Rad * Time.deltaTime, 0);

        // TO DO: add recoil if necessary
    }




    public override void Shoot() { }
}