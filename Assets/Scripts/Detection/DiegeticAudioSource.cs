using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Audio;

public abstract class DiegeticAudioSource : MonoBehaviour
{
    [SerializeField] protected float decibels;
    [SerializeField] protected AudioMixerGroup mixerGroup;

    Entity _source;

    public Entity source => _source ??= GetComponentInParent<Entity>();

}
