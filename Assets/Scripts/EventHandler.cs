using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class EventHandler
{
    //public static event System.Action<AttackMessage> OnAttack;
    public static event System.Action<DamageMessage> OnDamage;
    public static event System.Action<KillMessage> OnKill;
    //public static event System.Action<InteractMessage> OnInteract;
    public static event System.Action<SpawnMessage> OnSpawn;
    /*
    public static void Transmit(AttackMessage message)
    {
        if (OnAttack != null)
        {
            OnAttack.Invoke(message);
        }
    }
    public static void Subscribe(System.Action<AttackMessage> function, bool isAdding)
    {
        if (isAdding)
        {
            OnAttack += function;
        }
        else
        {
            OnAttack -= function;
        }
    }
    */
    public static void Transmit(DamageMessage message)
    {
        if (OnDamage != null)
        {
            OnDamage.Invoke(message);
        }
    }
    public static void Subscribe(System.Action<DamageMessage> function, bool isAdding)
    {
        if (isAdding)
        {
            OnDamage += function;
        }
        else
        {
            OnDamage -= function;
        }
    }

    public static void Transmit(KillMessage message)
    {
        if (OnKill != null)
        {
            OnKill.Invoke(message);
        }
    }
    public static void Subscribe(System.Action<KillMessage> function, bool isAdding)
    {
        if (isAdding)
        {
            OnKill += function;
        }
        else
        {
            OnKill -= function;
        }
    }
    /*
    public static void Transmit(InteractMessage message)
    {
        if (OnInteract != null)
        {
            OnInteract.Invoke(message);
        }
    }
    public static void Subscribe(System.Action<InteractMessage> function, bool isAdding)
    {
        if (isAdding)
        {
            OnInteract += function;
        }
        else
        {
            OnInteract -= function;
        }
    }
    */
    public static void Transmit(SpawnMessage message)
    {
        if (OnSpawn != null)
        {
            OnSpawn.Invoke(message);
        }

    }
    public static void Subscribe(System.Action<SpawnMessage> function, bool isAdding)
    {
        if (isAdding)
        {
            OnSpawn += function;
        }
        else
        {
            OnSpawn -= function;
        }
    }

    
}

public class DamageMessage
{
    //public float time;
    public Entity attacker;
    public Health victim;
    public DamageType method;
    public int amount;

    public DamageMessage(Entity _attacker, Health _victim, DamageType _method, int _amount)
    {
        attacker = _attacker;
        victim = _victim;
        method = _method;
        amount = _amount;
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
/*
public class InteractMessage
{
    public Player player;
    public Interactable interactable;

    public InteractMessage(PlayerHandler _player, Interactable _interactable)
    {
        player = _player;
        interactable = _interactable;
    }
}
*/
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