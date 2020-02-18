using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BackgroundImageAnimation : MonoBehaviour
{
    public float speed;
    public float horizontalAmplitude;
    public float verticalAmplitude;

    private int dirVer = -1;
    private int dirHor = -1;
    private float movedVer;
    private float movedHor;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        Vector3 newPos = this.transform.position;
        newPos.y += dirVer * speed;
        newPos.x += dirHor * speed;
        movedHor += speed;
        movedVer += speed;
        this.transform.position = newPos;

        if (movedHor >= horizontalAmplitude){
            dirHor = dirHor * -1;
            movedHor = 0f;
        }
        
        if (movedVer >= verticalAmplitude){
            dirVer = dirVer * -1;
            movedVer = 0f;
        }
    }
}
