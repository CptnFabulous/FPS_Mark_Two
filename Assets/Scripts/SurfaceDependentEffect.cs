using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class SurfaceDependentEffect : MonoBehaviour
{
    /*
    [System.Serializable]
    public class SurfaceEvent : UnityEvent<Material>
    {
        public string name;
    }
    */
    public ParticleSystem particles;


    //public UnityEvent<Material>[] events;


    public void Invoke(RaycastHit rh)
    {
        Material terrainMaterial = rh.collider.GetComponent<Renderer>().sharedMaterial;
        particles.GetComponent<ParticleSystemRenderer>().material = terrainMaterial;
        particles.Play();
    }
}
