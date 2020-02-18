using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VirtualRigidbodyHandler : MonoBehaviour
{
    public bool isGrounded;
    public Rigidbody2D rigid;
    public Vector2 tVector;
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
        rigid.drag = 50f;        
        rigid.gravityScale = 10f;  
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
    }
    public void MoveLeft(){
        if (Mathf.Abs(transform.rotation.eulerAngles.z) >= 25f){
            MoveMove(-tVector.x*1.5f,tVector.y);
        }else{
            MoveMove(-tVector.x,tVector.y);
        }
    }
    public void MoveRight(){
        if (Mathf.Abs(transform.rotation.eulerAngles.z) >= 25f){
            MoveMove(tVector.x*1.5f,tVector.y);
        }else{
            MoveMove(tVector.x,tVector.y);
        }
    }

}
