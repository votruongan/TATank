using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    [Header("SETTINGS")]
    public bool isStatic;
    public float speed = 0.1f;
    public Vector3 cameraOffset;
    [Header("FOR DEBUGGING")]
    public bool isLockTo;
    public Vector2 lastMousePos;
    public GameObject lockToObject;
    public Vector3 clampMax;
    public Vector3 clampMin;
    public void UpdateClamp(Vector3 center, Vector3 bottomLeftPos){
        Vector3 halfDimension = center - bottomLeftPos;
        clampMin = bottomLeftPos + cameraOffset;
        clampMin += new Vector3 (4.5f,2.5f,0f);
        clampMax = center + halfDimension - cameraOffset;
        clampMax -= new Vector3 (4.5f,2.5f,0f);
    }

    public void TranslateTo(Vector3 pos){
        isLockTo = false;
        Vector3 diff = pos - this.transform.position;
        this.transform.Translate(diff);
        transform.position = new Vector3(
            Mathf.Clamp(transform.position.x, clampMin.x, clampMax.x),
            Mathf.Clamp(transform.position.y, clampMin.y, clampMax.y),
            0f);
    }

    public void LockTo(Transform pos){
        this.TranslateTo(pos.position);
        isLockTo = true;
        lockToObject = pos.gameObject;
    }

//Move camera
    void FixedUpdate() {
        if (isStatic){
            return;
        }
        if (isLockTo){
            if (lockToObject == null){
                isLockTo = false;
                return;
            }
            this.transform.position = lockToObject.transform.position;
        }
        //Windows, MacOS, Linux
#if UNITY_STANDALONE 
        if (Input.GetMouseButtonDown(0)) {
            isLockTo = false;
            lastMousePos = Input.mousePosition;
            return;
        }

        if (Input.GetMouseButton(0)) {
            Vector2 mousePos = Input.mousePosition;
            // calculate the difference in prev and post mouse position
            mousePos -= lastMousePos;
            transform.Translate(-mousePos.x*speed,-mousePos.y*speed,0);
            lastMousePos = mousePos + lastMousePos;
        }
// Mobile devices
#else 
        if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Moved) {
            Vector2 touchDeltaPosition = Input.GetTouch(0).deltaPosition;
            transform.Translate(-touchDeltaPosition.x * speed, -touchDeltaPosition.y * speed, 0);
            isLockTo = false;
        }
#endif
    }



    void Start(){
    	lastMousePos = Input.mousePosition;
    }
}
