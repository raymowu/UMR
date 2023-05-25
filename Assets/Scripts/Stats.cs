using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Stats : MonoBehaviour
{
    public float health;
    public float damage;
    public float attackSpeed;

    public void TakeDamage(GameObject target, float damage)
    {
        target.GetComponent<Stats>().health -= damage;

        if (target.GetComponent<Stats>().health <= 0)
        {
            // Handle death

        }
    }
}
