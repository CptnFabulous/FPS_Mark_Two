using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

public class HeadsUpDisplay : MonoBehaviour
{
    [Header("General")]
    public Player controller;
    public new Camera camera;
    public AudioSource soundPlayer;
    public void PlayAudioClip(AudioClip clip)
    {
        soundPlayer.PlayOneShot(clip);
    }
    public void PlayAudioClip(DiegeticSound soundEffect)
    {
        soundEffect.Play(soundPlayer);
    }

    [Header("Damage")]
    public UnityEvent damageEffects;
    //public UnityEvent criticalEffects;
    public UnityEvent killEffects;
    public void CheckToPlayDamageEffects(DamageMessage message)
    {
        // Don't play effects for self-damage or self-healing
        if (message.victim == controller.health) return;

        if (message.method == DamageType.Healing) return;
        if (message.method == DamageType.DeletionByGame) return;

        // Don't play effects if another enemy was directly dealing the damage
        // I need to update this later to detect if an indirect form of damage was still caused by the player (so it can appropriately reward the player for actually setting up said kills)
        Entity attacker = message.attacker;
        if (attacker is Character c && c != controller && message.method != DamageType.PhysicsImpact) return;
        //if (attacker != controller) return;

        damageEffects.Invoke();
    }

    private void Awake()
    {
        Notification<DamageMessage>.Receivers += CheckToPlayDamageEffects;
    }
}
