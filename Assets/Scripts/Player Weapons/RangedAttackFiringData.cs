using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class RangedAttackFiringData : MonoBehaviour
{
    public float sway = 0.2f;
    public float range = 300;
    public Transform muzzle;

    [Header("Ammunition")]
    public AmmunitionType ammoType;
    public int ammoPerShot = 1;
    //public bool ConsumesAmmo => ammoType != null && ammoPerShot > 0;


    [Header("Recoil")]
    public float recoilMagnitude = 2;
    public AnimationCurve recoilCurve;
    public float recoilTime = 0.5f;
    protected static float recoilSwaySpeed = 10; // I'm not going to bother making this an editable value because it'll probably be exactly the same.
    // (I might take the last 3 of these values and make them values in WeaponHandler instead, since these properties most likely won't change from different guns)


    [HideInInspector, System.NonSerialized] public Character user;

    public abstract LayerMask hitDetection { get; }
    public abstract int damage { get; }
    public virtual float spread => user.weaponHandler.aimSwayAngle;
    public abstract void Shoot();
    //public abstract void TrySendMessage(RangedAttack r, ref float timeOfLastMessage);


    protected void ApplyRecoil()
    {
        if (recoilMagnitude <= 0) return;
        if ((user is Player player) == false) return;

        MovementController playerMovement = player.movement;

        float time = Time.time * recoilSwaySpeed;
        float x = Mathf.PerlinNoise(time, 0);
        float y = Mathf.PerlinNoise(0, time);
        x = Mathf.Lerp(-1, 1, x);
        //y = Mathf.Lerp(-1, 1, y);
        Vector2 recoilDirection = new Vector2(x, y).normalized;
        recoilDirection *= recoilMagnitude;

        playerMovement.StartCoroutine(playerMovement.lookControls.recoilController.AddRecoilOverTime(recoilDirection, recoilTime, recoilCurve));
    }

    public void TrySendMessage(RangedAttack r, ref float timeOfLastMessage)
    {
        Character User = r.User;
        // Transmit telegraph message to AI, if it's the first shot or enough time has passed since the previous message transmission
        if (Time.time - timeOfLastMessage <= r.controls.messageDelay) return;

        //int damage = projectilePrefab.damageStats.damage;
        //float spread = shotSpread + User.weaponHandler.aimSwayAngle;

        DirectionalAttackMessage newMessage = new DirectionalAttackMessage(User, damage, User.LookTransform.position, User.aimDirection, range, spread, hitDetection);
        Notification<AttackMessage>.Transmit(newMessage);

        timeOfLastMessage = Time.time; // Resets time
    }
}