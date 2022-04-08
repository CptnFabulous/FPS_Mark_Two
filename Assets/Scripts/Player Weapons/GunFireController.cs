using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GunFireController : MonoBehaviour
{
    public float roundsPerMinute = 600;
    public int maxBurst = 1;
    public float messageDelay = 1;

    public float ShotDelay => 60 / roundsPerMinute;
    public bool CanBurst(int numberOfShots) => numberOfShots < maxBurst || maxBurst <= 0;
    public bool InBurst { get; private set; }
    

    
    

    public IEnumerator Fire(RangedAttack mode)
    {
        InBurst = true;
        int shotsInBurst = 0;

        // last message time is set to negative infinity so that the message is always ran on the first shot
        float timeOfLastMessage = Mathf.NegativeInfinity;

        while (CanBurst(shotsInBurst) && mode.PrimaryHeld && mode.CanShoot())
        {
            // Transmit telegraph message to AI, if it's the first shot or enough time has passed since the previous message transmission
            if (Time.time - timeOfLastMessage > messageDelay)
            {
                DamageEffect projectileEffect = mode.stats.projectilePrefab.damageEffect;
                int damage = projectileEffect != null ? projectileEffect.baseDamage : int.MaxValue;
                float spread = mode.stats.shotSpread + mode.User.standingAccuracy;

                DirectionalAttackMessage newMessage = new DirectionalAttackMessage(mode.User.controller, damage, mode.User.aimAxis.position, mode.User.AimDirection, mode.stats.range, spread, mode.stats.projectilePrefab.detection);
                Notification<AttackMessage>.Transmit(newMessage);

                timeOfLastMessage = Time.time; // Resets time
            }

            // Fire shot and increment burst timer
            mode.SingleShot();
            shotsInBurst++;
            
            yield return new WaitForSeconds(ShotDelay);
        }

        yield return new WaitWhile(() => mode.PrimaryHeld);

        InBurst = false;
    }
}