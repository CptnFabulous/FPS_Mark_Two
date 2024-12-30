using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class ObjectHeat : MonoBehaviour
{
    public float degreesCelsius;

    Renderer[] _renderers;

    public Renderer[] renderers => MiscFunctions.GetImmediateComponentsInChildren(this, ref _renderers);












    private void OnEnable() => _active.Add(this);
    private void OnDisable() => _active.Remove(this);


    static List<ObjectHeat> _active = new List<ObjectHeat>();

    public static IReadOnlyList<ObjectHeat> activeHeatSources => _active;









    /// <summary>
    /// TO DO: replace this with something assigned from a singleton, maybe based on the object's position
    /// </summary>
    public static float ambientHeat => 20;
    /// <summary>
    /// TO DO: replace this with something assigned from a singleton
    /// </summary>
    public static float maxHeat => 40;
    /// <summary>
    /// TO DO: replace this with something assigned from a singleton
    /// </summary>
    public static float minHeat => 0;
}