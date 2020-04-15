using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletController : MonoBehaviour
{
    [Header("FOR DEBUGGING")]
    // public List<Vector3> positionList;
    public bool justFired = true;
    // public Rigidbody2D rigidBody;
    public int time;
    public float currentT = 0f;
    public float velorX;
    public float velorY;
    public bool isGrounded; 
    public bool isFired;
    //public bool isDestined; // if explode position is pre-calculated
    public GameController gameController;
    public LayerMask groundMask;
    public LayerMask backgroundMask;
    public Vector3 initTransform;
    [Header("SETTINGS")]
	public float groundSensitivity = 0.1f;
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

    public void SetBall(Sprite spr){
        this.gameObject.GetComponent<SpriteRenderer>().sprite = spr;
    }
    public void Fire(float vx, float vy){
        Debug.Log("Bullet is firing with - vx: "+vx +" vy: "+vy);
        velorX = vx;
        velorY = vy;
        // maxX = this.transform.position.x + vx;
        // maxY = this.transform.position.y + vy;
        initTransform = this.transform.position;// +  new Vector3(0f,0.1f,0f);
        // transform.Translate(new Vector3(vx/10,vy/10,0f));
        //rigidBody.simulated = true;
        // if (rigidBody == null)
            // FindRigidBody();
        isFired = true;
        justFired = true;
        // this.rigidBody.AddForce(new Vector2(velorX*66,velorY*66));
        // Debug.Log("[BULLET] moving speed "+rigidBody.velocity);
    } 
    

    private Vector3 targetPos;    
    public void SetTarget(Vector3 targ){
        targetPos = targ;

        isFired = true;
        initTransform = this.transform.position;
    }
    
    public void Fire(int time, float vx, float vy){
        // StartCoroutine(TimedDestroy((float)time/1000));
        this.Fire(vx,vy);
    }
    IEnumerator TimedDestroy(float secs){
        // Debug.Log("Timed Destroy: "+ secs);
        yield return new WaitForSeconds(secs);
        Destroy(this.gameObject);
    }
    
    private float dT = 0.02f;
    // private float totalTime = 0f;
    private float aX;
    private float aY;
    void FixedUpdate(){
        if (this.transform.position.y <= -20f || this.transform.position.x <= -20f){
            Destroy(this.gameObject);
        }
        if (isFired){
            // totalTime += 0.02f;
            aX = 2 * velorX / 10f;
            aY = (70f - 2 * velorY) / 10f;
            velorX = velorX - aX * dT;
            velorY = velorY - aY * dT;
            initTransform.x = initTransform.x + velorX * dT;
            initTransform.y = initTransform.y + velorY * dT;
            this.transform.position = initTransform;
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
    
    // void FindRigidBody(){
    //     rigidBody = this.gameObject.GetComponent<Rigidbody2D>();
    //     rigidBody.simulated = true;
    // }

    // Start is called before the first frame update
    void Start()
    {
        // FindRigidBody();
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
            GameController.GetInstance().BombExplodeAt(this.transform.position);
            //rigidBody.simulated = false;
        }
    }
}
