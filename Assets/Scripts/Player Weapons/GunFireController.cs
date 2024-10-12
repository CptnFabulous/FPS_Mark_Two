using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GunFireController : MonoBehaviour
{
    public float roundsPerMinute = 600;
    public int minBurst = 0;
    public int maxBurst = 1;
    public float burstCooldown = 0f;
    public float messageDelay = 1;

    public float ShotDelay => 60 / roundsPerMinute;
    public bool CanBurst(int numberOfShots) => numberOfShots < maxBurst || maxBurst <= 0;
    public bool WillBurst(int numberOfShots) => numberOfShots > 0 && numberOfShots < minBurst;
}