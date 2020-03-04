using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AngleIndicatorController : MonoBehaviour
{
    public GameObject angleBackground;
    public HandRotator handRotator;
    public int angleOffset = 0;
    public int currentAngle;

    public int minAngle = 0;
    public int maxAngle = 80;

    void Start(){
        handRotator = GameObject.Find("AngleHand").GetComponent<HandRotator>();
        this.transform.RotateAround(angleBackground.transform.position,transform.forward, 20);
    }


    public void ChangeAngle(int diff){
        // Debug.Log(angleOffset);
        angleOffset += diff;
        if (angleOffset < minAngle || angleOffset > maxAngle){
            angleOffset -= diff;
            return;
        }
        Debug.Log(angleOffset);
        this.transform.RotateAround(angleBackground.transform.position,transform.forward, diff);
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        int updateAngle = (int)transform.rotation.eulerAngles.z;
        if (updateAngle == currentAngle)
            return;
        // if (updateAngle >= 180){
        //     updateAngle = 360 - updateAngle;
        // }

        // if (updateAngle < -179){
        //     updateAngle = 180;
        // } else if (updateAngle > 180){
        //     updateAngle = -179;
        // }
        handRotator.SetAngle(updateAngle);
        currentAngle = updateAngle;
    }
}
