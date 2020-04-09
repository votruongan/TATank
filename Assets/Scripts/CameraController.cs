using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class CameraController : MonoBehaviour
{
    [Header("SETTINGS")]
    public bool isStatic;
    public float speed = 0.1f;
    public Vector3 cameraOffset;
    [Header("FOR DEBUGGING")]
    public bool isLockTo = false;
    public GameObject lockToObject;
    public bool isMove = false;
    public Vector3 moveTarget;
    public Vector3 moveSpeed;
    public Vector2 lastMousePos;
    public Vector3 clampMax;
    public Vector3 clampMin;
     
    Vector3 tmpV3;
    bool isShaking;
    public void UpdateClamp(Vector3 bottomLeftPos, Vector3 topRightPos){
        // Vector3 halfDimension = center - bottomLeftPos;
        // clampMin = bottomLeftPos + cameraOffset;
        // clampMin += new Vector3 (4.5f,2.5f,0f);
        // clampMax = center + halfDimension - cameraOffset;
        // clampMax -= new Vector3 (4.5f,2.5f,0f);
        Debug.Log("Updating Clamp: min: " + bottomLeftPos + " - max: " + topRightPos);
        Vector3 offset = new Vector3 (7.3f,4.1f,0f);
        clampMax = topRightPos - offset;
        clampMin = bottomLeftPos + offset;
        if (clampMax.x < clampMin.x){
            clampMax.x = clampMin.x;
        }
        if (clampMax.y < clampMin.y){
            clampMax.y = clampMin.y;
        }
    }
    // Move to target pos in about 20 frame
    public void TranslateTo(Vector3 pos){
        moveSpeed = pos - this.transform.position;
        moveSpeed.x = moveSpeed.x/20f;
        moveSpeed.y = moveSpeed.y/20f;
        moveSpeed.z = 0f;
        this.TranslateTo(pos,moveSpeed);
    }
    // Move to target pos with a speed
    public void TranslateTo(Vector3 pos, Vector3 mSpeed){
        // isLockTo = false;
        moveTarget = pos;
        moveSpeed = mSpeed; 
        isMove = true;
        // Debug.Log("Translating to " + pos + " at " + mSpeed);
            // StartCoroutine(ExecTranslate());
    }

    // IEnumerator ExecTranslate(){
    //     while (isMove)
    //     {
    //         if (Vector3.Distance(transform.position,moveTarget) < 0.05f){
    //             isMove = false;
    //             break;
    //         }
    //         BaseTranslateTo(moveSpeed.x, moveSpeed.y);
    //         // Debug.Log(" Cam pos : " + transform.position);
    //         yield return new WaitForSeconds(0.02f);
    //     }
    // }
    void BaseTranslateTo(float vx, float vy){
        // Debug.Log(" Basetranslate : vx:" + vx + " vy:" + vy);
        tmpV3 = this.transform.position;
        tmpV3.x = Mathf.Clamp(tmpV3.x + vx,clampMin.x,clampMax.x);
        tmpV3.y = Mathf.Clamp(tmpV3.y + vy,clampMin.y,clampMax.y);
        transform.position = tmpV3;
    }
    void NoClampTranslateTo(float vx, float vy){
        // Debug.Log(" Basetranslate : vx:" + vx + " vy:" + vy);
        tmpV3 = this.transform.position;
        tmpV3.x += vx;
        tmpV3.y += vy;
        transform.position = tmpV3;
    }

    public void Shake(){
        isShaking = true;
        StartCoroutine(ExecShake());
    }

    IEnumerator ExecShake(){
        NoClampTranslateTo(-0.1f,0.2f);
        yield return new WaitForSeconds(0.01f);
        NoClampTranslateTo(0.2f,-0.4f);
        yield return new WaitForSeconds(0.01f);
        NoClampTranslateTo(-0.1f,0.2f);
        isShaking = false;
    }

    public void LockTo(Transform pos){
        this.TranslateTo(pos.position);
        isLockTo = true;
        lockToObject = pos.gameObject;
    }

//Move camera
    void FixedUpdate() {
        if (isStatic || isShaking){
            return;
        }
        if (isLockTo){
            if (lockToObject == null){
                isLockTo = false;
                isMove = false;
                moveSpeed = Vector3.zero;               
                return;
            }
            TranslateTo(lockToObject.transform.position);
            // this.transform.position = lockToObject.transform.position;
        }
        // Debug.Log("checking ismove: " + isMove);
        if (isMove){
            float dist = Vector3.Distance(transform.position,moveTarget);
            // Debug.Log(dist);
            if (dist < 0.086f){
                isMove = false;
                return;
            }
            BaseTranslateTo(moveSpeed.x, moveSpeed.y);
            // Debug.Log(" Cam pos : " + transform.position);
            // return;
        }
        //Windows, MacOS, Linux
#if UNITY_STANDALONE || UNITY_EDITOR
        // Debug.Log("Unity Standalone");
        if (Input.GetMouseButtonDown(0)) {
            // Check if the mouse was clicked over a UI element
            if (EventSystem.current.IsPointerOverGameObject())
            {
                return;
            }
            // Debug.Log("GetMouse");
            isLockTo = false;
            isMove = false;
            lastMousePos = Input.mousePosition;
            return;
        }
        if (Input.GetMouseButtonUp(0)) {
            // Debug.Log("ReleaseMouse");
        }

        if (Input.GetMouseButton(0)) {
            // Debug.Log("OnMouse");    
            Vector2 mousePos = Input.mousePosition;
            // calculate the difference in prev and post mouse position
            mousePos -= lastMousePos;
            BaseTranslateTo(-mousePos.x*speed,-mousePos.y*speed);
            // transform.Translate(-mousePos.x*speed,-mousePos.y*speed,0);
            lastMousePos = mousePos + lastMousePos;
        }
// Mobile devices
#else 
        // Debug.Log("Unity Mobile devices");
        if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Moved) {
            // Check if the mouse was clicked over a UI element
            if (EventSystem.current.IsPointerOverGameObject())
            {
                return;
            }
            isLockTo = false;
            isMove = false;
            Vector2 touchDeltaPosition = Input.GetTouch(0).deltaPosition;
            BaseTranslateTo(-touchDeltaPosition.x*speed,-touchDeltaPosition.y*speed);
            // transform.Translate(-touchDeltaPosition.x * speed, -touchDeltaPosition.y * speed, 0);
            isLockTo = false;
        }
#endif
    }
    
    IEnumerator TimedTest(){
        yield return new WaitForSeconds(0.2f);
        Shake();
    }

    void Start(){
    	lastMousePos = Input.mousePosition;
        isMove = false;
        // StartCoroutine(TimedTest());
    }
}
