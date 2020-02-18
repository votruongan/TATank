using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HandRotator : MonoBehaviour
{
    public Transform anchor;
    public Vector3 currentAngles;
    public Text angleDisplay;
    public int currentAngle;
    public GameObject angleIndicator;
    public GameObject angleBackground;
    public void Start(){
        currentAngle = 0;
    }
    public void InverseAngle(){
        // Debug.Log("Inverse Angle Called");
        if (currentAngle >= 0){
            SetAngle(180 - currentAngle);
        } else {
            SetAngle(currentAngle + 90);
        }
    }
    public void SetAngle(bool isHeadingRight,int targetAngle){
        //Target run from -90 to 90
        int target = (targetAngle > 90)?90:targetAngle;
        target = (targetAngle < -90)?-90:targetAngle;
        if (!isHeadingRight){
            if (target < 0){
                target = -180 - target;
            } else {
                target = 180-target;
            }
        }     
        this.SetAngle(target);
    }
    public void SetAngle(int target){
        // targetAngle = target;
        if (target < 179){
            target = 360+target;
        }
        if (target > 180){
            target = target - 360;
        }
        if (angleDisplay == null)
            return;
        // Quaternion quaRot = new Quaternion();
        // quaRot.eulerAngles = new Vector3(0f,0f,targetAngle);
        // transform.rotation = quaRot;
        // float angleDiff = Mathf.DeltaAngle(target,currentAngle);
        // //Debug.Log(angleDiff+ " " + target + " " + currentAngle);
        // //angleDiff = whichSide(target,currentAngle,counterAngle(currentAngle))*angleDiff;
        // currentAngle = target;
        // transform.RotateAround(anchor.position,transform.forward,-angleDiff);

        // if (angleDisplay != null)
        //     angleDisplay.text = displayAngle.ToString();
        transform.RotateAround(anchor.position,transform.forward,-currentAngle);
        currentAngle = target;
        transform.RotateAround(anchor.position,transform.forward,target);
        int displayAngle = currentAngle;
        if (Mathf.Abs(currentAngle) > 90){
            displayAngle = 180 - Mathf.Abs(currentAngle);
            displayAngle = ((currentAngle >= 0)?1:-1)* displayAngle;
        }
        angleDisplay.text = displayAngle.ToString();
    }

    public void ReloadAngleIndicator(){
        angleIndicator = GameObject.Find("AngleIndicator");
        angleBackground = GameObject.Find("AngleBackground");
    }


    int whichSide(int angle, int main, int counter){
        if (main >= 0){
            if (angle < main && angle > counter){
                return -1;
            }
        } else {
            if (angle < main ||(angle > counter)){
                return -1;
            }
        }
        return 1;
    }

    int counterAngle(int angle){
        if (angle > 0){
            return angle - 180;
        }
        return angle + 180;
    }

    void FixedUpdate()
    {
        // currentAngle = transform.rotation.eulerAngles;
    }
}
