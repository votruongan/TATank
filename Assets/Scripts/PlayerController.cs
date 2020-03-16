using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerController : LivingController
{
    [Header("FOR DEBUG")]
    public GameController gameController;
    public byte speedTime;

    [Header("FOR ASSIGNMENT")]
    public GameObject movingBullet;

    [Header("DATA")]
    public PlayerInfo info;
    public PlayerInfo originalInfo;

    [Header("SETTINGS")]
    public GameObject staticBullet;
    public GameObject bulletPrefab;
    public GameObject normalDamagePrefab;
    public GameObject criticalDamagePrefab;
	[Header("GROUND")]
	protected FixedJoint2D bulletJoint;

    // Start is called before the first frame update
    protected void Start()
    {
        base.Start();
        if (gameController == null)
            gameController = GameObject.Find("GameController").GetComponent<GameController>();

        bulletJoint = this.GetComponent<FixedJoint2D>();

        if (staticBullet == null)
            staticBullet = this.FindChildObject("StaticBullet");
            
        anim = transform.GetChild(1).gameObject.GetComponent<Animator>();
        if (info != null)
            if (info.direction > 180){
                this.RotateLiving();
            }
    }

    public void ChangeClothes(string name){
        
    }


    public void UsingFightingProp(string propString){
        if ((!anim.GetBool("IsMoving")&&!anim.GetBool("IsFiring")&&!anim.GetBool("IsFired"))){
            this.PlayAnimation("PlayerHappy");  
            StartCoroutine(WaitAndSetIdle(1.5f));
        }
        this.PlayEffect("EffectBuff");
        Texture propTexture = GameObject.Find("Prop_"+propString+"_Button").transform.GetChild(0).gameObject.GetComponent<RawImage>().texture;
        this.DisplayPropIcon(propTexture);
    }
    IEnumerator WaitAndSetIdle(float time){
        yield return new WaitForSeconds(time);
        this.PlayAnimation("PlayerIdle");
    }
    public void AddFireTag(byte time){
        speedTime = time;
    }
    protected void FixedUpdate() {
        base.FixedUpdate();
    }

    public void Teleport(float time, Vector3 pos){
        //System.Threading.Thread.Sleep(speedTime);
        //staticBullet.SetActive(false);
        StartCoroutine(WaitAndTele(time,pos));
    }

    IEnumerator WaitAndTele(float time, Vector3 pos){
        yield return new WaitForSeconds(time);
        rigidBody.simulated = false;
        isMoving = false;
        this.transform.position = pos;
        rigidBody.simulated = true;
    }

    public void Fire(int time, float vx, float vy, Vector3 target){
        //System.Threading.Thread.Sleep(speedTime);
        //staticBullet.SetActive(false);
        Fire(vx,vy, target);
    }

    public void Fire(float vx, float vy, Vector3 target){
        //staticBullet.SetActive(false);
        StartCoroutine(ExecFire(vx,vy,target));
    }
    IEnumerator ExecFire(float vx, float vy, Vector3 target){
        while (this.isMoving)
        {
            yield return new WaitForSeconds(0.02f);
        }
        movingBullet = Instantiate(bulletPrefab,this.transform.position,Quaternion.identity);
        BulletController bCtrl = movingBullet.GetComponent<BulletController>();
        bCtrl.Fire(vx, vy);
        bCtrl.SetTarget(target);
    }


    IEnumerator WaitAndDamage(float time, int damage, bool critical, int remainingBlood){
        yield return new WaitForSeconds(time);
        this.PlayAnimation("PlayerCry");
        GameObject damPanel = null;
        DamagePanelController damPanelController = null;
        if (critical){
           damPanel = Instantiate(criticalDamagePrefab,this.transform.position,Quaternion.identity);
        } else {
           damPanel = Instantiate(normalDamagePrefab,this.transform.position,Quaternion.identity);
        }
        if (damPanel != null){
            damPanelController = damPanel.GetComponent<DamagePanelController>();
            damPanelController.SetDamage(damage);
        }
        UpdateHealthBar(info.team,(float)remainingBlood/originalInfo.blood);
    }
    public virtual void Damaged(int time, int damage, bool critical, int remainingBlood){
        StartCoroutine(WaitAndDamage(time/1000, damage, critical, remainingBlood));
        Debug.Log("damaging, isCritical: "+critical+ " remaining blood: "+ remainingBlood);
        this.info.blood = remainingBlood;
    }

    public virtual void GetTurn(bool isTurn){
        if (isTurn)
            healthBar.getTurnSprite.gameObject.SetActive(true);
        else
            healthBar.getTurnSprite.gameObject.SetActive(false);
    }

    public virtual void SetPlayerInfo(PlayerInfo inf, GameController gc = null){
        if (gc != null)
            this.gameController = gc;
        originalInfo = inf;
        if (healthBar == null)
            healthBar = this.FindChildObject("HealthInfo").GetComponent<HealthBarSprite>();
        if (healthBar.playerName == null)
            healthBar.BaseChangeColor();
        healthBar.playerName.text = inf.nickname;
        UpdatePlayerInfo(inf);
    }

    public virtual void UpdatePlayerInfo(PlayerInfo inf){
        this.info = inf.Clone();
        this.UpdateHealthBar(this.originalInfo.team, (float)this.info.blood/this.originalInfo.blood);
    }

    public void resetBullet(){
        staticBullet.SetActive(true);
    }
}
