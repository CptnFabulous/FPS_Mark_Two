using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DamageMessage
{
    public Entity attacker;
    public Entity weaponUsed;
    public Health victim;
    public DamageType method;
    public int damage;
    public bool critical;
    public int stun;
    public Vector3 direction;
    
    public DamageMessage(Entity _attacker, Entity weaponUsed, Health _victim, DamageType _method, int _damage, bool _critical, int _stun, Vector3 direction)
    {
        attacker = _attacker;
        this.weaponUsed = weaponUsed;
        victim = _victim;
        method = _method;
        damage = _damage;
        critical = _critical;
        stun = _stun;
        this.direction = direction;
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