﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MainPlayerController : PlayerController
{
    [Header("FOR DEBUG")]
    public bool isOnTurn;
    public float timer;
    public bool startTimer;
    public bool isTakePower;
    public int fireBuff = 0;
    public bool isBeginMove;
    public VirtualRigidbodyHandler virtualRigidBody;
    public GameObject indicatorRotator;
    public GameObject angleBackground;
    public PlayerFightInfoDisplay fightInfoDisplay;
    public UIController uiController;

	[Header("UISignal")]
    public bool isUpArrowDown;
    public bool isDownArrowDown;
    public bool isLeftArrowDown;
    public bool isRightArrowDown;

	[Header("MOVING")]
    public GameObject virtualRigidbodyPrefab;
    public float movePacketDelay = 0.6f;
	public Vector2 moveSlowMultiply;
    public float tx = 1.5f;
    public float ty = 1.5f;
    
	[Header("FIRE CONTROL")]
    public bool isFiring;
    public int angleMax;
    public int angleMin;
    public int firePower;
    public int fireAngle;
    public Slider powerIndicator;
    public Slider prevPowerIndicator;
    public HandRotator handRotator;
	public float powerSpeed = 0.02f;
	public float power;
    public float zRotation;

    // Start is called before the first frame update
    void Start()
    {
        base.Start();
        isOnTurn = false;
        if (powerIndicator == null)
            try{
                powerIndicator = GameObject.Find("PowerIndicator").GetComponent<Slider>();
            } catch (Exception e){

            }
        if (prevPowerIndicator == null)
            try{
                prevPowerIndicator = GameObject.Find("PreviousPowerIndicator").GetComponent<Slider>();
            } catch (Exception e){
                
            }
        // angleIndicatorAnchor = transform.GetChild(2).GetComponent<Transform>().position;
        // angleIndicator = transform.GetChild(1).GetComponent<Transform>();
        if (handRotator == null)
            try{
                handRotator = GameObject.Find("AngleHand").GetComponent<HandRotator>();
            } catch (Exception e){
                
            }
        // try {
        //     virtualRigidBody = GameObject.Find("VirtualRigidbody").GetComponent<VirtualRigidbodyHandler>();
        // } catch(Exception e){
        //     Debug.Log("Err: "+ e);
        // }
        indicatorRotator = GameObject.Find("AngleIndicator");
        angleBackground = GameObject.Find("AngleBackground");
        fightInfoDisplay = GameObject.Find("PlayerFightInfo").GetComponent<PlayerFightInfoDisplay>();
        if (virtualRigidBody != null){
            Debug.Log("VRB != NULL");
            Destroy(virtualRigidBody.gameObject);
        } else {
        }
        GameObject vrb = Instantiate(virtualRigidbodyPrefab,this.transform.position,Quaternion.identity);
        virtualRigidBody = vrb.GetComponent<VirtualRigidbodyHandler>();

        isHeadingRight = (this.transform.rotation.eulerAngles.x < 179f);
        moveSlowMultiply  = new Vector2(0.8f,0f);
        fireAngle = 20;
        handRotator.SetAngle(fireAngle);
        firePower = 0;  
        timer = 0f;
        // rigidBody.simulated = true;
        //anim = transform.GetChild(1).gameObject.GetComponent<Animator>();
    }
    
    public void GetTurn(bool isMainTurn){
        isOnTurn = isMainTurn;
    }


    private void ChangeAngle(int change){
        fireAngle += change;
        if (fireAngle >= angleMax || fireAngle <= angleMin){
            fireAngle -= change;
            return;
        }
        if (fireAngle < -179){
            fireAngle = 180;
        } else if (fireAngle > 180){
            fireAngle = -179;
        }
        handRotator.SetAngle(fireAngle);
        UpdateAngleIndicator();
    }

    private void UpdateAngleIndicator(){
        indicatorRotator.transform.RotateAround(angleBackground.transform.position,transform.forward,
        -indicatorRotator.transform.rotation.eulerAngles.z);
        indicatorRotator.transform.RotateAround(angleBackground.transform.position,transform.forward,fireAngle);
    }

    private void execTranslate(){
        //transform.Translate(moveSpeed.x,0.01f,0.0f);
        //Debug.Log("moving with " + transform.right);
        //rigidBody.drag = 8f;

        if (!isGrounded)
        {
            // this.SetRestRigidbody();
            return;
        }
        // this.SetMovableRigidbody();
        // Vector3 force = new Vector3(1.0f,0f,0f);
        // rigidBody.AddForce(Vector3.Scale(transform.right,force));
        Vector3 virtualRbdPos = virtualRigidBody.transform.position;
        
        if (isGrounded){
            if (isHeadingRight){
                virtualRigidBody.MoveRight();
            } else {
                virtualRigidBody.MoveLeft();
            } 
            // if (Vector2.Distance(virtualRbdPos, this.transform.position) >= 0.5f){
            //     Debug.Log("Go Go Go vrb's position: "+ virtualRbdPos);
            //     this.MoveTo(virtualRbdPos);
            // }
        }
    }

    IEnumerator MoveExecution(){  
        Debug.Log("MoveExecution isBeginMove: "+isBeginMove);
        while (isBeginMove){
            yield return new WaitForSeconds(0.18f);
            // check virtual rigidbody rotation and act accordingly
            // if (Mathf.Abs(virtualRigidBody.gameObject.transform.rotation.eulerAngles.z) > 90f){
            //     this.MoveTo(virtualRigidBody.transform.position+new Vector3(0f,-0.1f,0f));
            //     continue;
            // }
            // if (Mathf.Abs(virtualRigidBody.gameObject.transform.rotation.eulerAngles.z) > 150f){
            //     this.MoveTo(virtualRigidBody.transform.position+new Vector3(0f,-0.1f,0f));
            //     continue;
            // }
    this.MoveTo(virtualRigidBody.transform.position);
        }
    }



    //Update every 0.02s
    void FixedUpdate(){
        base.FixedUpdate();
        if (!isOnTurn){
            return;
        }
    	// Left/Right Arrow
    	if (Input.GetKey(KeyCode.RightArrow)|| isRightArrowDown){
            if (!isBeginMove)
                BeginMove();
            if (!isHeadingRight){
                RotateLiving();
            }
            //MoveTo(new Vector3(this.transform.position.x + moveSpeed.x,this.transform.position.y,0f));
            execTranslate();
    	}

    	if (Input.GetKey(KeyCode.LeftArrow) || isLeftArrowDown){
            if (!isBeginMove)
                BeginMove();
            if (isHeadingRight){
                RotateLiving();
            }
            //MoveTo(new Vector3(this.transform.position.x + moveSpeed.x,this.transform.position.y,0f));
            execTranslate();
    	}

        // Up/Down Arrow, adjust angle
        if (Input.GetKey(KeyCode.UpArrow) || isUpArrowDown){
            this.ChangeAngle(1);
        }

        if (Input.GetKey(KeyCode.DownArrow) || isDownArrowDown){
            this.ChangeAngle(-1);
        }

    	// Spacebar pressed -> calculating the force
    	if (Input.GetKey(KeyCode.Space)){
            TakePower();
    	}
    }
    public void BeginMove(){
        Debug.Log("BeginMove");
        anim.SetBool("isFiring",false);
        anim.SetBool("isFired",false);
        anim.SetBool("isMoving",true);
        this.SetMovableRigidbody();
        isBeginMove = true;
        startTimer = true;
        StartCoroutine(MoveExecution());
        virtualRigidBody.SetMovingRigidbody();
    }
    public void BeginTakePower(){
        isFiring = true;
        anim.SetBool("isMoving",false);
        anim.SetBool("isFiring",true);
        PlayEquipAnimation("isFiring");
    }

    public void TakePower(){
        if (!isOnTurn){
            powerIndicator.value = 0.0f;
        }

        if (!isTakePower){
            isTakePower = true;
            return;
        }
        power += powerSpeed;
        if (power > 1f){
            powerSpeed = -powerSpeed;
        }
        if (power < 0f){
            powerSpeed = -powerSpeed;
        }
        powerIndicator.value = power;
        isTakePower = false;
    }

    public void FinishMove(){
        Debug.Log("FinishMove");
        isBeginMove = false;
        this.anim.SetBool("isMoving",false);
        this.PlayAnimation("PlayerIdle");
        this.StopEquipAnimation();
        startTimer = false;
        timer = 0f;
        virtualRigidBody.SetRestRigidbody();
        this.SetRestRigidbody();
        gameController.MainPlayerMove(this.transform.position.x,this.transform.position.y,isHeadingRight);
    }

    public void RotateLiving(){
        base.RotateLiving();
    }

    public void ReleasePower(){
        if (power > 1f){
            power = 1f;
        }
        if (power < 0f){
            power = 0f;
        }
        powerIndicator.value = power;
        prevPowerIndicator.value = power;
        firePower = (int)((float) power * 100);
        //Vx = (force * cos(a)) / 5 
        float vx = (float)firePower * Mathf.Cos((float)fireAngle)/5;
        float vy = (float)firePower * Mathf.Sin((float)fireAngle)/5;
        // this.Fire(vx,vy);
        //Vy = (force * sin(a) - gt) / 5 
        //bulletController.Fire(vx,vy);
        // bulletRigidBody.AddForce(new Vector2(power*100,power*100));
        // gameController.MainPlayerFire(firePower,fireAngle);
        // anim.SetBool("isFiring",false);
        // anim.SetBool("isFired",true);
        // anim.Play("PlayerFired",0,0f);
        // bulletJoint.enabled = false;
        // bulletRigidBody.gameObject.SendMessage("Fired");
        // power = 0f;
        StartCoroutine(FireExecution());
        powerIndicator.value = 0f;
    }

    IEnumerator FireExecution(){    
        for (int i = 0; i < fireBuff + 1; i++){
            firePower = (int)((float) power * 100);
            gameController.MainPlayerFire(firePower,fireAngle + (int)this.transform.rotation.eulerAngles.z);
            yield return new WaitForSeconds(1.0f);
        }
        fireBuff = 0;
        power = 0f;
    }

    public void UsingFightingProp(string name, int id){
        this.UsingFightingProp(name);
        gameController.SendUsingFightingProp(id);
        switch (name){
            case "X2":
                fireBuff = 2;
                break;
            case "X1":
                fireBuff = 1;
                break;
        }
    }
     
    IEnumerator DelayedUpdatePlayerInfo(int time){
        yield return new WaitForSeconds(time/1000);
        this.UpdatePlayerInfo(this.info);

    }
    public override void Damaged(int time, int damage, bool critical, int remainingBlood){
        base.Damaged(time,damage,critical,remainingBlood);
        StartCoroutine(DelayedUpdatePlayerInfo(time));
    }

    public override void SetPlayerInfo(PlayerInfo inf, GameController gc = null){
        this.originalInfo = inf.Clone();
        fightInfoDisplay.original = this.originalInfo.Clone();
        base.SetPlayerInfo(inf, gc);
    }
    public override void UpdatePlayerInfo(PlayerInfo inf){
        base.UpdatePlayerInfo(inf);
        // Debug.Log("MainPlayer");
        fightInfoDisplay.FightInfoDisplay(inf.energy,inf.blood,inf.dander);
    }


    // Update every frame
    void Update()
    {        
        base.Update();

        if (!isOnTurn){
            return;
        }

        int target = fireAngle + (int)this.transform.position.z;
        UpdateAngleIndicator();
        handRotator.SetAngle(isHeadingRight, target);
        if (base.isMoving){
        }

        if (startTimer){    
            timer += Time.deltaTime;
        }
        if (timer >= movePacketDelay){
            timer = 0;
            gameController.MainPlayerMove(this.transform.position.x,this.transform.position.y,this.isHeadingRight);
        }
        //check if is moving
        if (Input.GetKeyDown(KeyCode.LeftArrow)||Input.GetKeyDown(KeyCode.RightArrow))
        {
            this.anim.SetBool("isFired",false);
            this.anim.SetBool("isMoving",true);
            this.PlayEquipAnimation("PlayerMove");
        }

        if (Input.GetKeyUp(KeyCode.LeftArrow)||Input.GetKeyUp(KeyCode.RightArrow))
        {
            FinishMove();
        }

    	if (Input.GetKeyDown(KeyCode.Space)){
            BeginTakePower();
    	}

    	// Release the power
    	if (Input.GetKeyUp(KeyCode.Space)){
            ReleasePower();
    	}
    }

}