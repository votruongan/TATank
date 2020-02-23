using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VirtualRigidbodySensor : MonoBehaviour
{
    public bool isActivated = false;
    

    private void OnTriggerEnter2D(Collider2D other) {
        isActivated = true;
    }

    private void OnTriggerExit2D(Collider2D other) {
        isActivated = false;
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
