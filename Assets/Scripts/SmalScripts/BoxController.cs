using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoxController : MonoBehaviour
{
    //disabled by default
    public DelayedSelfDestroy dsd;
    void Start(){
        dsd = this.gameObject.GetComponent<DelayedSelfDestroy>();
        dsd.SetStarted(false);
    }

    void OnCollisionEnter2D(Collision2D col){
        Debug.Log("Trigger collider2d enter");
        gameObject.GetComponent<Animator>().SetBool("isCollided",true);
        dsd.SetStarted(true);
    }
    
    void OnTriggerEnter2D(Collider2D col){
        Debug.Log("Trigger collider2d enter");
        gameObject.GetComponent<Animator>().SetBool("isCollided",true);
        dsd.SetStarted(true);
    }
}
