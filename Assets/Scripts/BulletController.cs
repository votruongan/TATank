using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletController : MonoBehaviour
{
    [Header("FOR DEBUGGING")]
    public List<Vector3> positionList;
    public bool justFired = true;
    public Rigidbody2D rigidBody;
    public int time;
    public float currentT = 0f;
    public float g = 9.8f; //g accelerator
    public float velorX;
    public float velorY;
    public float maxX;
    public float maxY;
    public bool isGrounded; 
    public bool isFired;    
    //public bool isDestined; // if explode position is pre-calculated
    public GameController gameController;
    public LayerMask groundMask;
    public LayerMask backgroundMask;
    public Vector3 initTransform;
    public Vector3 explodePosition;
    [Header("SETTINGS")]
    public Vector3 bulletOffset;
	public float groundSensitivity = 0.1f;
    public float dT = 0.02f;
    public float liveTime = 0f;
    public float timer = 0f;

    //Vx = (force * cos(a)) / 5	
    //Vy = (force * sin(a)) / 5 

    //move the bullet to the current position
    private void execFire(int time){
        // //make bullet move in 20
        // for (int i = 0; i < 20; i++)
        // {
            
        // }
        //this.transform.position = new Vector3(verlorX,verlorY,0f);   
    }
    public void Fire(float vx, float vy){
        // Debug.Log("vx: "+vx +" vy: "+vy);
        velorX = vx;
        velorY = vy;
        maxX = this.transform.position.x + vx;
        maxY = this.transform.position.y + vy;
        initTransform = this.transform.position +  new Vector3(0f,0.1f,0f);
        // transform.Translate(new Vector3(vx/10,vy/10,0f));
        //rigidBody.simulated = true;
        if (rigidBody == null)
            FindRigidBody();
        isFired = true;
        justFired = true;
        this.rigidBody.AddForce(new Vector2(velorX*66,velorY*66));
        // Debug.Log("[BULLET] moving speed "+rigidBody.velocity);
    }
    // private bool isHeadingRight;
    private Vector3 targetPos;
    // private Vector3 middlePos;
    public void SetTarget(Vector3 targ){
        targetPos = targ;
        // middlePos = Vector3.zero;
        // middlePos.x = targetPos.x + this.transform.position.x;
        // middlePos.y = this.transform.position.y + velorY;
        // if (this.transform.position.x - targetPos.x < 0){
        //     isHeadingRight = true;
        // } else {
        //     isHeadingRight = false;
        // }
        isFired = true;
        initTransform = this.transform.position;
    }
    // private float moveUnit = 0.01f;
    private float tanBallAngle;
    public void Fire(int time, float vx, float vy){
        //isDestined = false;
        initTransform = this.transform.position;
        velorX = vx;
        velorY = vy;
        // transform.Translate(new Vector3(vx/10,vy/10,0f));
        isFired = true;
        // float dx;
        // float dy;
        // int times = (int) ((float)time/20);
        // for (int i = 0; i < times/2; i++)
        // {
            
        // }
        tanBallAngle = vy/vx;
        StartCoroutine(TimedDestroy(time/1000));
        // this.rigidBody.AddForce(new Vector2(velorX*75,velorY*75));
    }
    IEnumerator TimedDestroy(float secs){
        Debug.Log("Timed Destroy: "+ secs);
        yield return new WaitForSeconds(secs);
        Destroy(this.gameObject);
    }
    // private bool passMidCheckpoint = false;
    private float totalTime = 0f;
    void FixedUpdate(){
        if (this.transform.position.y <= -20f || this.transform.position.x <= -20f){
            Destroy(this.gameObject);
        }
        if (isFired){
            totalTime += 0.02f;
            float xPos = initTransform.x + velorX * totalTime;
            float yPos = initTransform.y + velorY * totalTime - 0.5f * 9.81f * totalTime * totalTime;
            this.transform.position = new Vector3(xPos, yPos , 0f);
            //move the bullet with diagonal 
            // Debug.Log("[BULLET] moving speed "+rigidBody.velocity);
            //isFired = false;
            // if (!passMidCheckpoint){
            //     this.transform.Translate(new Vector3(moveUnit,moveUnit*tanBallAngle));
            //     if (this.transform.position.x - targetPos.x < moveUnit){
            //         passMidCheckpoint = true;
            //     }
            // } else {
            //     this.transform.Translate(new Vector3(moveUnit,moveUnit*tanBallAngle - 2 * moveUnit * tanBallAngle));
            // }
            //this.transform.Translate(new Vector3(velorX*dT,velorY*dT,0f));
            // Debug.Log("[BULLET] this pos - x "+transform.position.x.ToString()+" - y "+transform.position.y.ToString());
        }
        //check if bullet is grounded
        Vector2 topLeft = new Vector2(transform.position.x - groundSensitivity, transform.position.y - groundSensitivity);
        Vector2 botRight = new Vector2(transform.position.x + groundSensitivity, transform.position.y + groundSensitivity);

        isGrounded = Physics2D.OverlapArea(topLeft,botRight,groundMask);   
        // check if object could be viewable by camera  
        //isGrounded = !(Physics2D.OverlapArea(topLeft,botRight,backgroundMask)); 
        if (isGrounded){
            return;
        }
    }
    
    void FindRigidBody(){
        rigidBody = this.gameObject.GetComponent<Rigidbody2D>();
        rigidBody.simulated = true;
    }

    // Start is called before the first frame update
    void Start()
    {
        FindRigidBody();
    	//gameController = GameObject.Find("GameController").GetComponent<GameController>();
        groundMask = LayerMask.GetMask("Ground"); 
        backgroundMask = LayerMask.GetMask("Background");
        //isFired = false;
        // isDestined = false;
        //velorX = 0f;
       // velorY = 0f;
    }

    // Update is called once per frame
    void Update()
    {
        if (justFired){
            timer += Time.deltaTime;
            if (timer > 0.04f){
                justFired = false;
            }
            return;
        }
    	//if bullet contact to ground
        if (isGrounded && isFired){
            // if (!isDestined){
            //     gameController.BulletExplodeAt(this.transform.position);
            // }
            Destroy(this.gameObject);
            //rigidBody.simulated = false;
        }
        // if (this.transform.position.y >= maxX){
        //     //Debug.Log("Over X. ");
        // }
        // if (this.transform.position.y >= maxY){
        //    // Debug.Log("Over Y. ");
        // }
    }
}
