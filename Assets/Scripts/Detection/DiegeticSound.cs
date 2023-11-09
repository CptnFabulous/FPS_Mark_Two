using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Random Sound Player", menuName = "ScriptableObjects/Random Sound Player", order = 0)]
public class DiegeticSound : ScriptableObject
{
    public AudioClip[] sounds;
    [Range(-3, 3)]
    public float minPitchVariance = 1;
    [Range(-3, 3)]
    public float maxPitchVariance = 1;
    [Range(0, 1)]
    public float minVolumeVariance = 1;
    [Range(0, 1)]
    public float maxVolumeVariance = 1;
    public float delay;

    public void Play(AudioSource source)
    {
        if (delay <= 0)
        {
            PlayWithoutDelay(source);
            return;
        }
        MonoBehaviour behaviourToRunFrom = source.GetComponent<MonoBehaviour>();
        behaviourToRunFrom.StartCoroutine(DelayPlay(source));
    }

    public void PlayWithoutDelay(AudioSource source)
    {
        source.pitch = Random.Range(minPitchVariance, maxPitchVariance);
        source.volume = Random.Range(minVolumeVariance, maxVolumeVariance);

        int index = Random.Range(0, sounds.Length - 1);

        //Debug.Log("Playing sound clip " + sounds[index].name);
        source.PlayOneShot(sounds[index]);
    }

    public void PlayWithoutSource(Transform positionTransform)
    {
        PlayWithoutSource(positionTransform.position, 1);
    }
    public void PlayWithoutSource(Vector3 origin, float playerVolume)
    {
        int index = Random.Range(0, sounds.Length);
        float volume = Random.Range(minVolumeVariance, maxVolumeVariance) * playerVolume;
        AudioSource.PlayClipAtPoint(sounds[index], origin, volume);
    }


    

    IEnumerator DelayPlay(AudioSource source)
    {
        yield return new WaitForSeconds(delay);
        PlayWithoutDelay(source);
    }
}
