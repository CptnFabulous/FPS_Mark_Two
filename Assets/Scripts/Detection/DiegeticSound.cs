using CptnFabulous.MiscUtility;
using CptnFabulous.ObjectPool;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Diegetic Sound", menuName = "ScriptableObjects/Diegetic Sound", order = 1)]
public class DiegeticSound : ScriptableObject
{
    /// <summary>
    /// A private struct used just to check how recently a sound played in the same location.
    /// </summary>
    struct PlayedSound
    {
        public Vector3 checkOriginPoint;
        public float time;

        public PlayedSound(Vector3 point, float time)
        {
            this.checkOriginPoint = point;
            this.time = time;
        }
    }

    [SerializeField] AudioClip[] sounds;
    [SerializeField] UnityEngine.Audio.AudioMixerGroup mixerGroup;
    public float decibels = 80;
    public float range = 10;
    [SerializeField, Range(-3, 3)] float minPitchVariance = 1;
    [SerializeField, Range(-3, 3)] float maxPitchVariance = 1;
    /*
    [SerializeField, Range(0, 1)] float minVolumeVariance = 1;
    [SerializeField, Range(0, 1)] float maxVolumeVariance = 1;
    */

    HashSet<PlayedSound> recentSoundChecks = new HashSet<PlayedSound>();
    static readonly float distanceThresholdSquared = 1f * 1f;
    static readonly float ageThreshold = 0.2f;


    static DiegeticAudioSource _temporarySourcePrefab;

    static DiegeticAudioSource temporarySourcePrefab
    {
        get
        {
            // If prefab is already set up, return it
            if (_temporarySourcePrefab != null) return _temporarySourcePrefab;

            // Create pool parent
            Transform poolParent = new GameObject("Temporary Audio Source parent").transform;
            DontDestroyOnLoad(poolParent.gameObject);
            // Create prefab
            _temporarySourcePrefab = new GameObject("Temporary Audio Source").AddComponent<DiegeticAudioSource>();
            _temporarySourcePrefab.transform.SetParent(poolParent);
            _temporarySourcePrefab.player.playOnAwake = false;
            // Create pool
            ObjectPool.TryCreateObjectPool(_temporarySourcePrefab, poolParent);

            Debug.Log(_temporarySourcePrefab.player);

            return _temporarySourcePrefab;
        }
    }




    
    public void Play(Entity entity) => Play(() => entity.CentreOfMass, entity);
    public void Play(ParticleCollisionInfo pce) => Play(() => pce.particleData.intersection, pce.sourceEntity, pce.strengthRatio);
    [System.Obsolete]
    public void Play(AudioSource source)
    {
        Debug.LogWarning($"Playing {this} from {source} is obsolete, replace with another method.");
        Entity e = source.GetComponentInParent<Entity>();
        Play(() => source.transform.position, e);
    }
    public void Play(Vector3 point, Entity sourceEntity, float multiplier = 1, float minimumVolume = 0)
    {
        Play(() => point, sourceEntity, multiplier, minimumVolume);
    }
    public void Play(System.Func<Vector3> getPosition, Entity sourceEntity, float multiplier = 1, float minimumVolume = 0)
    {
        AudioClip clip = sounds[Random.Range(0, sounds.Length)];
        float volumeForPlayer = multiplier;
        //volumeForPlayer *= Random.Range(minVolumeVariance, maxVolumeVariance);
        volumeForPlayer = Mathf.Max(volumeForPlayer, minimumVolume);

        // Actually play the sound
        PlayOneShot(volumeForPlayer, getPosition);

        // Play diegetic code (assuming there's an entity to check against)
        if (sourceEntity == null) return;

        #region Check if an AI check was done for this sound recently near this point. If so, don't do an AI check until that one has expired.

        // Remove all sounds played before the age threshold.
        // If a sound is still in, then that always means it's new enough that other checks shouldn't be run near that position.
        // This acts as an age check and keeps the hashset length small, at the same time.
        float oldestAcceptableTime = Time.unscaledTime - ageThreshold;
        recentSoundChecks.RemoveWhere((ps) => ps.time < oldestAcceptableTime);

        // If a recent instance of this sound was played near this point, don't do an AI hearing check.
        Vector3 checkOriginPoint = sourceEntity.CentreOfMass;
        foreach (PlayedSound sound in recentSoundChecks)
        {
            // Use square magnitude for lower hardware cost
            float distanceSquared = (sound.checkOriginPoint - checkOriginPoint).sqrMagnitude;
            if (distanceSquared < distanceThresholdSquared) return;
        }

        // Ensure this sound check's position is logged in the hashset.
        recentSoundChecks.Add(new PlayedSound(checkOriginPoint, Time.unscaledTime));

        #endregion

        // TO DO: play text in subtitles, if close enough for the player to hear

        // Do the actual check
        DiegeticAudioListener.DiegeticCheck(this, multiplier, sourceEntity);
        DebugUtility.DrawDebugWireSphere(getPosition.Invoke(), range * multiplier, Color.magenta, 1);
    }



    // I think that for this to work, I need to make the temporary source data work off a DiegeticAudioSource instead.

    void PlayOneShot(float volume, System.Func<Vector3> setPosition)
    {
        // Summon source
        DiegeticAudioSource tempSource = ObjectPool.RequestObject(temporarySourcePrefab);

        // Set random values
        AudioClip clip = sounds[Random.Range(0, sounds.Length)];
        float basePitch = Random.Range(minPitchVariance, maxPitchVariance);
        tempSource.player.clip = clip;
        tempSource.player.outputAudioMixerGroup = mixerGroup;
        tempSource.player.volume = volume;
        tempSource.pitch = basePitch;

        // Start an async function to play the source, and stop once the source expires
        tempSource.StartCoroutine(RunOneShot(tempSource, setPosition));
    }
    static IEnumerator RunOneShot(DiegeticAudioSource tempSource, System.Func<Vector3> setPosition)
    {
        // Play noise, and wait until it stops
        tempSource.Play();
        while (tempSource.isPlaying)
        {
            yield return null;
            tempSource.transform.position = setPosition.Invoke();
        }

        // End temporary audio source, and return it to the pool
        tempSource.Stop();
        ObjectPool.DismissObject(tempSource);
    }
}
