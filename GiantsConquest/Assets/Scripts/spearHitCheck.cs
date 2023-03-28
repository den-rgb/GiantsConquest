using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class spearHitCheck : MonoBehaviour
{
    // Start is called before the first frame update
    GameObject player;
    GameObject parentObject;
    Animator anim;
    void Start()
    {
        Transform currentTransform = transform;
        while (currentTransform.parent != null) {
            currentTransform = currentTransform.parent;
        }
        parentObject = currentTransform.gameObject;
        anim = parentObject.GetComponent<Animator>();
    }
    void OnTriggerEnter(Collider other) {
        print(other.gameObject.name);
        print(other.gameObject.tag);
        if (other.gameObject.tag == "Player") {
            if(anim.GetCurrentAnimatorStateInfo(0).IsName("knight_attack"))
            other.gameObject.GetComponent<Health>().TakeDamage(10);
            print("hit object");
            print(other.gameObject.GetComponent<Health>().currentHealth);
        }
    }

    // void OnTriggerStay(Collider other) {
    //     print(other.gameObject.name);
    //     print(other.gameObject.tag);
    //     if (other.gameObject.tag == "Player") {
    //         if(anim.GetCurrentAnimatorStateInfo(0).IsName("knight_attack"))
    //         other.gameObject.GetComponent<Health>().currentHealth -= 10;
    //         other.gameObject.GetComponent<Health>().TakeDamage(10);
    //         print("hit object2");
    //         print(other.gameObject.GetComponent<Health>().currentHealth);
    //     }
    // }

}
