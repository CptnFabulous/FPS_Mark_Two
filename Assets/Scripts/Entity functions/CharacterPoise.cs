using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterPoise : MonoBehaviour
{
    public Resource poise = new Resource(10, 10, 2);
    public float restoreTime = 3;
    [SerializeField] Health health;
    [SerializeField] StateController stateController;
    [SerializeField] StateFunction stunState;

    void Awake()
    {
        health.onDamage.AddListener((dm) => WearDown(dm.stun));
    }
    void Update()
    {
        poise.Increment(poise.max / restoreTime * Time.deltaTime);
    }

    public void WearDown(int stunValue)
    {
        // Do nothing if already staggered
        if (stateController.currentState == stunState) return;

        // Reduce poise
        poise.Increment(-stunValue);
        if (poise.current <= 0)
        {
            // If poise is depleted, stagger enemy
            stateController.SwitchToState(stunState);
        }
    }

}
