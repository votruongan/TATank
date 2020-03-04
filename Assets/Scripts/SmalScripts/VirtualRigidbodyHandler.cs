using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VirtualRigidbodyHandler : MonoBehaviour
{
    public MainPlayerController mpc;
    public bool isGrounded;
    public Rigidbody2D rigid;
    public Vector2 tVector;
    public VirtualRigidbodySensor upSensor;
    public VirtualRigidbodySensor rightSensor;
    public VirtualRigidbodySensor leftSensor;
    public bool isMoving;
    public bool isHeadingRight;
    // Start is called before the first frame update
    void Start()
    {
        rigid = this.gameObject.GetComponent<Rigidbody2D>();
        isGrounded = false;
    }

    private void OnCollisionEnter2D(Collision2D other) {
        isGrounded = true;
        SetRestRigidbody();
    }

    private void OnCollisionExit2D(Collision2D other) {
        isGrounded = false;  
        rigid.drag = 0.5f;        
        rigid.gravityScale = 10f;  
    }

    public Vector3 GetFinePosition(){
        Vector3 adjust = new Vector3(0f,0f,0f);
        if (upSensor.isActivated){
            // adjust.x = 0.1f;
            adjust.y = -0.1f;
        }
        if (rightSensor.isActivated){
            // adjust.x = 0.1f;
            adjust.y = -0.1f;
        }
        if (leftSensor.isActivated){
            // adjust.x = 0.1f;
            adjust.y = -0.1f;
        }
        return this.transform.position + adjust;
    }

    public void SetMovingRigidbody(){
        rigid.drag = 10f;
    }
    public void SetRestRigidbody(){
        rigid.drag = 500f;     
        rigid.gravityScale = 1f; 
        if (Mathf.Abs(transform.rotation.eulerAngles.z) >= 70f){
            transform.Rotate(-transform.rotation.eulerAngles,Space.Self);
        }
    }

    public void MoveMove(float dx, float dy){
        rigid.MovePosition(new Vector2 (this.transform.position.x + dx,this.transform.position.y + dy));
        // isGrounded = false;
        // if (isGrounded){
        //     WaitAndRefresh(dx, dy);
        // }
    }
    IEnumerator WaitAndRefresh(float dx, float dy){
        while (!this.mpc.isMoving){
            yield return null;
        }
        this.transform.position = mpc.transform.position + new Vector3(dx,dy,0f);
    }
    public void MoveMoveLeft(){
        if (Mathf.Abs(transform.rotation.eulerAngles.z) >= 25f){
            MoveMove(-tVector.x*1.5f,tVector.y);
        }else{
            MoveMove(-tVector.x,tVector.y);
        }
        // MoveMove(-0.2f,0.1f);
    }

    public void MoveMoveRight(){
        if (Mathf.Abs(transform.rotation.eulerAngles.z) >= 25f){
            MoveMove(tVector.x*1.5f,tVector.y);
        }else{
            MoveMove(tVector.x,tVector.y);
        }
        // MoveMove(0.2f,0.1f);
    }

    private void FixedUpdate() {
        if (isMoving){
            if (isHeadingRight){
                MoveMoveRight();
            } else {
                MoveMoveLeft();
            }
        }
    }

    public void StopMove(){
        isMoving = false;
        this.transform.position = mpc.transform.position;
    }

    public void MoveLeft(){
        isHeadingRight = false;
        isMoving = true;        
    }

    public void MoveRight(){
        isHeadingRight = true;
        isMoving = true;
    }

}
