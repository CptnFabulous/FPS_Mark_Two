using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DiegeticAudioSource : MonoBehaviour
{
    public AudioSource source;
    
    // Update is called once per frame
    void Update()
    {
        // TO DO: have data on diegetic sound, and periodically send out checks
        source.pitch = Time.timeScale;
    }
}
