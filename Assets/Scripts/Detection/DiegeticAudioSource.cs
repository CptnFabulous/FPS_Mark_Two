using CptnFabulous.MiscUtility;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class DiegeticAudioSource : MonoBehaviour
{
    [SerializeField] Entity sourceEntity;

    [Range(0, 1)] public float volume = 1;
    [Range(-3, 3)] public float pitch = 1;

    AudioSource _player;

    public AudioSource player => gameObject.Cache(ref _player, ComponentGetType.AddIfAbsent);
    public bool isPlaying => player.isPlaying;

    void Update()
    {
        player.pitch = pitch * Time.timeScale;

        // TO DO: if playing, periodically send out diegetic checks
    }

    /*
    private void OnDrawGizmos()
    {
        if (isPlaying == false) return;
        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(transform.position, 0.05f);
        //float radius = 
        //Gizmos.DrawWireSphere(transform.position, );
    }
    */

    /*
    public void SetClip(DiegeticSound sound)
    {
        player.clip = clip;
        player.volume = volume;
        player.outputAudioMixerGroup = sound.mixerGroup;
    }
    */

    public void Play() => player.Play();
    public void Pause() => player.Pause();
    public void Stop() => player.Stop();
}
