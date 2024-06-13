//using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using UnityEngine;
using UnityEngine.Audio;

[CreateAssetMenu(fileName = "New Diegetic Sound", menuName = "ScriptableObjects/Diegetic Sound", order = 1)]
public class DiegeticSound : ScriptableObject
{
    [SerializeField] AudioClip[] sounds;
    //[SerializeField] AudioMixerGroup mixerGroup;
    [SerializeField] float decibels = 80;
    [SerializeField, Range(-3, 3)] float minPitchVariance = 1;
    [SerializeField, Range(-3, 3)] float maxPitchVariance = 1;
    /*
    [SerializeField, Range(0, 1)] float minVolumeVariance = 1;
    [SerializeField, Range(0, 1)] float maxVolumeVariance = 1;
    [SerializeField] float delay;
    */

    public void Play(AudioSource source) => Play(source, 1);
    public void Play(AudioSource source, float multiplier)
    {
        Play(source.transform.position, source.GetComponentInParent<Entity>(), source, multiplier);
    }
    public void Play(Vector3 point, Entity sourceEntity, float multiplier = 1, bool playAudioAtMaxVolume = false)
    {
        AudioSource source = null;
        if (sourceEntity != null)
        {
            source = sourceEntity.GetComponentInChildren<AudioSource>();
        }
        Play(point, sourceEntity, source, multiplier, playAudioAtMaxVolume);
    }

    public void Play(Vector3 point, Entity sourceEntity, AudioSource source, float multiplier = 1, bool playAudioAtMaxVolume = false)
    {
        AudioClip clip = sounds[Random.Range(0, sounds.Length)];
        float volumeForPlayer = /*Random.Range(minVolumeVariance, maxVolumeVariance) * */multiplier;
        volumeForPlayer = playAudioAtMaxVolume ? 1 : volumeForPlayer;
        if (source != null)
        {
            source.pitch = Random.Range(minPitchVariance, maxPitchVariance);
            source.volume = volumeForPlayer;
            source.PlayOneShot(clip);
        }
        else
        {
            AudioSource.PlayClipAtPoint(clip, point, volumeForPlayer);
        }

        // TO DO: play text in subtitles, if close enough for the player to hear

        // Play diegetic code (assuming there's an entity to check against)
        if (sourceEntity == null) return;
        DiegeticAudioListener.DiegeticCheck(this, decibels * multiplier, sourceEntity);

    }
}
