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
}