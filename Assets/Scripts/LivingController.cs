using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LivingController : BaseObjectController
{
	public LayerMask groundLayer;
	public bool isGrounded;
	protected Animator anim;
    protected Rigidbody2D rigidBody;
    public Animator effectDisplay;
    public Animator[] equipAnimators;
    public SpriteRenderer propIcon;
    public HealthBarSprite healthBar;
    [Header("GROUND")]
    //public float groundSensitivity;
    public Vector2 groundSensitivity;
    
    [Header("MOVEMENT")]
    public bool isHeadingRight;
	protected bool sentFinishMove;
    protected bool sentGravityNormalize;
	public bool isMoving;
    public List<Vector3> positionList;
    public Vector3 targetPosition;
    
	protected void Start(){   
		try{
			anim = this.gameObject.GetComponent<Animator>();
		}catch(Exception e){
			Debug.Log("No Animator, e: " + e.ToString());
		}
		try{
			rigidBody = this.GetComponent<Rigidbody2D>();
		}catch(Exception e){
			Debug.Log("No Rigidbody, e: " + e.ToString());
		}
        isMoving = false;
		try{
            groundLayer = LayerMask.GetMask("Ground");
		}catch(Exception e){
			Debug.Log("No ground e: " + e.ToString());
		}
        if (propIcon == null)
            propIcon = this.FindChildObject("PropDisplay").GetComponent<SpriteRenderer>();

        if (effectDisplay == null)
            effectDisplay = this.FindChildObject("EffectDisplay").GetComponent<Animator>();

        if (healthBar == null)
            healthBar = this.FindChildObject("HealthInfo").GetComponent<HealthBarSprite>();
        targetPosition = this.transform.position;
        isHeadingRight = true;
        if (rigidBody != null)
            rigidBody.simulated = true;
        sentGravityNormalize = false;
        sentFinishMove = false;
        rigidBody.drag = 100f;
        // Debug.Log("Init transform: " + this.transform.position);
        //StartCoroutine(WaitAndInit(0.1f));
	}
    
    //Update heathbar
    public void UpdateHealthBar(int colorCode, float percent){
        if (healthBar == null)
            healthBar = this.FindChildObject("ForeHealth").GetComponent<HealthBarSprite>();
        if (colorCode == 1)
            healthBar.ChangeToBlue();
        else
            healthBar.ChangeToRed();
        healthBar.UpdateHealthBar(percent);
    }

    public void DisplayPropIcon(Texture propTexture){
        propIcon.sprite = Sprite.Create((Texture2D)propTexture,new Rect(0f,0f,propTexture.width,propTexture.height),
                                                            new Vector2(0.5f, 0.5f), 100.0f);
        propIcon.gameObject.SetActive(true);
    }

    public void PlayEffect(string effectName){
        if (effectDisplay == null){
            Debug.Log("No effectDisplay.");
            return;
        }
        effectDisplay.Play(effectName,0,0f);
    }

    IEnumerator WaitAndInit(float wait){
        yield return new WaitForSeconds(wait);
        //groundSensitivity = 0.2f;
    }

    public void RotateLiving(){   
        try{
            this.transform.Rotate(0f, 180f, 0f, Space.Self);
            Debug.Log("Rotate Living");
            //rigidBody.MoveRotation(180f);
            isHeadingRight = !isHeadingRight;
        }catch (Exception e){
            Debug.Log("Cannot get transform to rotate: "+ e.ToString());
        }
        if (propIcon != null)
            propIcon.transform.Rotate(0.0f, 180.0f, 0.0f, Space.Self);
        rigidBody.simulated = true;
    }

    private void execMove(float tx, float ty){
        // Debug.Log("[move]thread item count "+PendingActionCount());
        // Debug.Log("real time: "+Time.realtimeSinceStartup);
        //rigidBody.simulated = false;
        //this.transform.Translate(new Vector3(tx,ty,0f));
        //rigidBody.simulated = true;
        // if (rigidBody != null){
        //     rigidBody.MovePosition(new Vector2(transform.position.x + tx, transform.position.y + ty));
        //     return;
        // }
        this.transform.Translate(tx,ty,0f);
    }


    public void execMoveStop(){
        // Debug.Log("[stop]thread item count "+PendingActionCount());
        // Debug.Log("real time: "+Time.realtimeSinceStartup);
        anim.SetBool("isMoving",false);
    }

    //Move to position in Unity unit
    public void MoveTo(Vector3 pos, byte dir){
        MoveTo(pos);
        // if ((dir < 180 && !isHeadingRight)
        //     || (dir > 180 && isHeadingRight)){
        //     this.RotateLiving();
        // }
    }
    public void MoveTo(Vector3 pos){
        MoveTo(500,pos);
    }

    public void PlayAnimation(string AnimName){
        anim.Play(AnimName,0,0f);
        if (equipAnimators.Length > 0){
            PlayEquipAnimation(AnimName);
        }
    }


    public void PlayOneTimeEquipAnimation(string animName){
        PlayEquipAnimation(animName);
        StartCoroutine(DelayedStopEquipAnimation(1.5f));
    }

    public void PlayEquipAnimation(string animName){
        foreach(Animator anima in equipAnimators){
            anima.Play(animName,0,0f);
        }
    }

    IEnumerator DelayedStopEquipAnimation(float time){
        yield return new WaitForSeconds(time);
        StopEquipAnimation();
    }

    public void StopEquipAnimation(){
        foreach(Animator anima in equipAnimators){
            anima.Play("PlayerIdle",0,0f);
        }
    }



    //Move to position in Unity unit
    public void MoveTo(int milisec, Vector3 pos){
        // if (anim != null){
        //    anim.SetBool("isMoving",true);
        // }
        // if (equipAnimators.Length > 0){
        //     PlayEquipAnimation("PlayerMove");
        // }
        if ((pos - this.transform.position).x > 0){
            if (!isHeadingRight)
                RotateLiving();
        }
        else{
            if (isHeadingRight)
                RotateLiving();
        }
        if (Vector3.Distance(pos,this.transform.position)>=1.5f){
            
        }
        // Debug.Log("living is moving to: " + pos.ToString());
        targetPosition = pos;
        isMoving = true;
        sentFinishMove = false;
        Debug.Log("Moving to "+pos);
    }

    
    public virtual void Teleport(float time, Vector3 pos){
        //System.Threading.Thread.Sleep(speedTime);
        //staticBullet.SetActive(false);
        StartCoroutine(WaitAndTele(time,pos));
    }

    IEnumerator WaitAndTele(float time, Vector3 pos){
        yield return new WaitForSeconds(time);
        rigidBody.simulated = false;
        isMoving = false;
        this.transform.position = pos;
        SetFallRigidbody();
    }

    

    private void OnCollisionEnter2D(Collision2D other) {
        isGrounded = true;
        SetRestRigidbody();
    }

    private void OnCollisionExit2D(Collision2D other) {
        isGrounded = false;  
        rigidBody.drag = 50f;        
        rigidBody.gravityScale = 10f;  
    }

    protected void CheckGrounded() {
        if (rigidBody != null){
            return;
        }
        //check if bullet is grounded
        Vector2 topLeft = new Vector2(transform.position.x - groundSensitivity.x, transform.position.y - groundSensitivity.y);
        Vector2 botRight = new Vector2(transform.position.x + groundSensitivity.x, transform.position.y + groundSensitivity.y);

        isGrounded = Physics2D.OverlapArea(topLeft,botRight,groundLayer);   
        // isGrounded = !(Mathf.Abs(rigidBody.velocity.y) > 0.3f);
        
        //isGrounded = Physics2D.OverlapCircle(this.transform.position, groundSensitivity, groundLayer);

        // Vector2 position = transform.position;
        // Vector2 direction = Vector2.down;
        // float distance = groundSensitivity;
        // Debug.DrawRay(position, direction, Color.green);

        // RaycastHit2D hit = Physics2D.Raycast(position, direction, distance, groundLayer);
        // if (hit.collider != null) {
        //     isGrounded = true;
        // }
        // isGrounded = false;
    }

    protected void sendFinishMove(){
        sentFinishMove = true;
    }

    protected void Update(){
        // Debug.Log("Update transform: " + this.transform.position);
        if (isMoving){
            SetMovableRigidbody();
            if (isGrounded){
                // Debug.Log("Moving living to Target: " + targetPosition);
                Vector3 pos = Vector3.MoveTowards(this.transform.position,targetPosition,2f*Time.deltaTime) - transform.position;
                //Debug.Log("MoveTo Pos: " +pos.ToString());
                // rigidBody.MovePosition(pos);
                pos.x = (isHeadingRight)?(pos.x):(-pos.x);
                this.transform.Translate(pos.x,0.002f,0f);
                // Debug.Log(Mathf.Abs(transform.position.x -targetPosition.x));
                if (this.transform.position == targetPosition || Mathf.Abs(transform.position.x -targetPosition.x) <= 0.01f){
                    if (Mathf.Abs(transform.position.y -targetPosition.y) >= 0.02f){
                        this.transform.position = targetPosition;
                    }
                    Debug.Log("MoveTo Finished");
                    // StopEquipAnimation();
                    sentFinishMove = true;
                    isMoving = false;
                }
            }
            else {
            //    Debug.Log("isMoving but not grounded");
            }
        } else {
            if (!sentFinishMove){
                Debug.Log("Disable moving and send finish move");
                this.transform.position = targetPosition;
                SetRestRigidbody();
                if (anim != null)
                    anim.SetBool("isMoving",false);
                sentFinishMove = true;
            }
        }
    }

    protected void SetMovableRigidbody(){
        rigidBody.constraints = RigidbodyConstraints2D.None;
        rigidBody.drag = 0f;
        rigidBody.angularDrag = 0f;
        rigidBody.gravityScale = 2f;
    }
    protected void SetRestRigidbody(){
        rigidBody.constraints = RigidbodyConstraints2D.FreezePositionY;
        rigidBody.drag = 100f;
        rigidBody.angularDrag = 0f;
        rigidBody.gravityScale = 0.2f;
    }
    protected void SetFallRigidbody(){
        rigidBody.constraints = RigidbodyConstraints2D.FreezePositionX | RigidbodyConstraints2D.FreezeRotation;
        rigidBody.drag = 1f;
        rigidBody.gravityScale = 2f;
    }

    private bool sentGroundedFunc;
    protected void FixedUpdate(){
        // CheckGrounded();
        if (!isGrounded){
            Vector3 rot = transform.rotation.eulerAngles;
            // if (rot.z != 0f){
            //     transform.Rotate(new Vector3(0f,0f,-rot.z),Space.Self);
            // }
            this.transform.eulerAngles = new Vector3(0f,this.transform.eulerAngles.y,0f);
            this.SetFallRigidbody();
            sentGravityNormalize = false;
            sentGroundedFunc = true;
        }
        else{
            if(!sentGravityNormalize){
                //groundSensitivity = 0.15f;
                rigidBody.constraints = RigidbodyConstraints2D.None;
                SetRestRigidbody();
                sentGravityNormalize = true;
                sentGroundedFunc = false;
            }
        }
    }
}
