using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
public class Notification<T>
{
    public float timeOfEvent;
    public T message;

    public static System.Action<Notification<T>> Receivers;

    public static void Transmit(T newMessage)
    {
        Notification<T> newNotification = new Notification<T>();
        newNotification.message = newMessage;
        newNotification.timeOfEvent = Time.time;
        Receivers?.Invoke(newNotification);
    }
}
*/

/// <summary>
/// A system for transmitting data to other functions in the scene, in a decoupled and intuitive manner.
/// </summary>
/// <typeparam name="T"></typeparam>
public static class Notification<T>
{
    /// <summary>
    /// All functions that activate upon a notification being transmitted.
    /// </summary>
    public static System.Action<T> Receivers;
    /// <summary>
    /// Broadcasts T to all functions subscribed in 'Receivers'.
    /// </summary>
    /// <param name="newMessage"></param>
    public static void Transmit(T newMessage)
    {
        Receivers?.Invoke(newMessage);
    }
}

public class DamageMessage
{
    public Entity attacker;
    public Health victim;
    public DamageType method;
    public int damage;
    public bool critical;
    public int stun;
    
    public DamageMessage(Entity _attacker, Health _victim, DamageType _method, int _damage, bool _critical, int _stun)
    {
        attacker = _attacker;
        victim = _victim;
        method = _method;
        damage = _damage;
        critical = _critical;
        stun = _stun;
    }
}
public class KillMessage
{
    public Entity attacker;
    public Health victim;
    public DamageType causeOfDeath;

    public KillMessage(Entity _attacker, Health _victim, DamageType _causeOfDeath)
    {
        attacker = _attacker;
        victim = _victim;
        causeOfDeath = _causeOfDeath;
    }
}
public class InteractionMessage
{
    public Character user;
    public Interactable interactedWith;

    public InteractionMessage(Character _user, Interactable _interactedWith)
    {
        user = _user;
        interactedWith = _interactedWith;
    }
}
public class SpawnMessage
{
    public Entity spawned;
    public Vector3 location;

    public SpawnMessage(Entity _spawned, Vector3 _location)
    {
        spawned = _spawned;
        location = _location;
    }
}