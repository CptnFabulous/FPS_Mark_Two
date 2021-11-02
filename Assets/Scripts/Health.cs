using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Health : MonoBehaviour
{
    public Resource data = new Resource(100, 100, 20);
    Hitbox[] hitboxes;
    public UnityEvent<int> onDamage;
    public UnityEvent<int> onHeal;
    public UnityEvent<int> onDeath;
    public bool allowPosthumousDamage;
    public bool IsAlive
    {
        get
        {
            return data.current > 0;
        }
    }

    private void Awake()
    {
        hitboxes = GetComponentsInChildren<Hitbox>();
        for (int i = 0; i < hitboxes.Length; i++)
        {
            hitboxes[i].sourceHealth = this;
        }
        onHeal.Invoke(0);
    }

    public void Damage(int amount, Entity attacker)
    {
        if (IsAlive == false && allowPosthumousDamage == false)
        {
            return;
        }
        
        data.current -= amount;
        if (data.current <= 0)
        {
            onDeath.Invoke(amount);
        }
        else if (amount < 0)
        {
            onHeal.Invoke(amount);
        }
        else
        {
            onDamage.Invoke(amount);
        }
    }
}
