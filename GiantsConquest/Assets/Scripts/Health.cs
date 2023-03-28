using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Health : MonoBehaviour
{
    // Start is called before the first frame update
    int maxHealth ;
    public float currentHealth;
    void Start()
    {
        maxHealth = 100;
        currentHealth = (float)maxHealth;
    }

    public void TakeDamage(float damage){
        currentHealth -= damage;
        if (currentHealth <= 0){
            print("dead");
        }
    }
}
