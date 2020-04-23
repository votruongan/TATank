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
    int firedTime;
    Sprite bulletSprite;
    int bulletId;
    Sprite defaultBulletSprtite;
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
    bool isOnDander = false;
    bool needSendDanderScreen = false;

	[Header("GROUND")]
	protected FixedJoint2D bulletJoint;

    private EquipController ClothSprite;
    private EquipController FaceSprite;
    private EquipController HairSprite;
    private EquipController ArmSprite;
    
    // Start is called before the first frame update
    protected void Start()
    {
        base.Start();
        if (gameController == null)
            gameController = GameObject.Find("GameController").GetComponent<GameController>();
        defaultBulletSprtite = Resources.Load<Sprite>("bullet/hammer");
        bulletJoint = this.GetComponent<FixedJoint2D>();

        ClothSprite = this.FindChildObject("ClothSprite").GetComponent<EquipController>();
        FaceSprite = this.FindChildObject("FaceSprite").GetComponent<EquipController>();
        HairSprite = this.FindChildObject("HairSprite").GetComponent<EquipController>();
        ArmSprite = this.FindChildObject("ArmSprite").GetComponent<EquipController>();

        if (staticBullet == null)
            staticBullet = this.FindChildObject("StaticBullet");
        
        anim = transform.GetChild(1).gameObject.GetComponent<Animator>();
        if (info != null)
            if (info.direction > 180){
                this.RotateLiving();
            }
    }

    // public void ChangeAppearance(string cloth, string face, string hair){
    //     Debug.Log("Changing Appearance: "+ cloth + " - " + face + " - " + hair);
    //     if (!string.IsNullOrEmpty(cloth))
    //         StartCoroutine(ExecChangeCloth(cloth,0));
    //     if (!string.IsNullOrEmpty(face))
    //         StartCoroutine(ExecChangeFace(face,0));
    //     if (!string.IsNullOrEmpty(hair))
    //         StartCoroutine(ExecChangeHair(hair,0));
    // }
    
    public void ChangeAppearance(List<int> styleID){
        Debug.Log("Changing Appearance: "+ styleID);
        StartCoroutine(ExecChangeCloth(null,styleID[(int)eItemType.cloth]));
        StartCoroutine(ExecChangeFace(null,styleID[(int)eItemType.face]));
        StartCoroutine(ExecChangeHair(null,styleID[(int)eItemType.hair]));
        StartCoroutine(ExecChangeArm(null,styleID[(int)eItemType.arm]));
    }

    IEnumerator ExecChangeArm(string name, int equipId){
        while(ArmSprite == null){
            yield return new WaitForSeconds(0.01f);
        }
        if (string.IsNullOrEmpty(name)){
            ArmSprite.ChangeSpriteSheet(originalInfo.sex, equipId);
        }else{
            ArmSprite.ChangeSpriteSheet(originalInfo.sex, name);
        }
    }
    IEnumerator ExecChangeCloth(string name, int equipId){
        while(ClothSprite == null){
            yield return new WaitForSeconds(0.01f);
        }
        if (string.IsNullOrEmpty(name)){
            ClothSprite.ChangeSpriteSheet(originalInfo.sex, equipId);
        }else{
            ClothSprite.ChangeSpriteSheet(originalInfo.sex, name);
        }
    }
    IEnumerator ExecChangeFace(string name, int equipId){
        while(FaceSprite == null){
            yield return new WaitForSeconds(0.01f);
        }
        if (string.IsNullOrEmpty(name)){
            FaceSprite.ChangeSpriteSheet(originalInfo.sex, equipId);
        }else{
            FaceSprite.ChangeSpriteSheet(originalInfo.sex, name);
        }
    }

    IEnumerator ExecChangeHair(string name, int equipId){
        while(HairSprite == null){
            yield return new WaitForSeconds(0.01f);
        }
        if (string.IsNullOrEmpty(name)){
            HairSprite.ChangeSpriteSheet(originalInfo.sex, equipId);
        }else{
            HairSprite.ChangeSpriteSheet(originalInfo.sex, name);
        }
    }

    private GameObject danderDisplay;
    public void SetDander(int dander){
        if (danderDisplay == null)
            danderDisplay = this.FindChildObject("DanderDisplay");
        if (dander == 200){
            danderDisplay.SetActive(true);
        }
        else{
            danderDisplay.SetActive(false);
        }
    }

    public override void Teleport(float time, Vector3 pos){
        this.PlayEffect("EffectBuff");
        this.PlayAnimation("Happy");
        StartCoroutine(WaitAndSetIdle(1.5f));
        base.Teleport(time,pos);
    }

    public void UsingFightingProp(string propString){
        Debug.Log("Using propstring: " + propString);
        if ((!anim.GetBool("IsMoving")&&!anim.GetBool("IsFiring")&&!anim.GetBool("IsFired"))){
            this.PlayAnimation("Happy");
            StartCoroutine(WaitAndSetIdle(1.5f));
        }
        this.PlayEffect("EffectBuff");
        gameController.soundManager.PlayEffect("choose");
        gameController.soundManager.PlayEffect("noti");
        gameController.soundManager.PlayEffect("move");
        Texture propTexture = null;
        try
        {
            propTexture = GameObject.Find("Prop_"+propString+"_Button").transform.GetChild(0).gameObject.GetComponent<RawImage>().texture;
        }
        catch (System.Exception){  }
        ((FightUIController)gameController.uiController).AppendCurrentProp(propTexture);
        this.DisplayPropIcon(propTexture);
        if (propString == "DANDER"){
            isOnDander = true;
            needSendDanderScreen = true;
        }
    }
    
    IEnumerator WaitAndSetIdle(float time){
        yield return new WaitForSeconds(time);
        this.PlayAnimation("Idle");
    }
    public void AddFireTag(byte time){
        speedTime = time;
    }
    protected void FixedUpdate() {
        base.FixedUpdate();
    }
	public void SetBulletId(int bId){
        Debug.Log("Setting Bullet Id: " + bId);
        bulletId = bId;
	}
	public void SetBullet(string bulletName){
        Debug.Log("Loading Bullet: " + bulletName);
		Sprite tryLoad = Resources.Load<Sprite>("bullet/" + bulletName);
        if (tryLoad == null){
            return;
        }
        bulletSprite = tryLoad;
	}
    public void Fire(int time, float vx, float vy, Vector3 target){
        //System.Threading.Thread.Sleep(speedTime);
        //staticBullet.SetActive(false);
        // Fire(vx,vy, target);
        if (needSendDanderScreen){
            gameController.PlayDanderScreen(this.isHeadingRight, this.info);
            StartCoroutine(WaitAndCloseDanderFlag(2.0f));
            needSendDanderScreen = false;
        }
        StartCoroutine(ExecFire(time,vx,vy,target));
    }
    IEnumerator WaitAndCloseDanderFlag(float secs){
        yield return new WaitForSeconds(secs-0.02f);
        isOnDander  = false;
    }
    public void Fire(float vx, float vy, Vector3 target){
        //staticBullet.SetActive(false);
        // StartCoroutine(ExecFire(vx,vy,target));
    }
    IEnumerator ExecFire(int time, float vx, float vy, Vector3 target){
        if (isOnDander){
            firedTime++;
        }
        float waitSecs = (firedTime-1) * 0.5f;
        while (this.isMoving || isOnDander)
        {
            yield return new WaitForSeconds(0.02f);
        }
        yield return new WaitForSeconds(waitSecs);
        SoundManager.GetInstance().PlayEffect("startFire");
        movingBullet = Instantiate(bulletPrefab,this.transform.position + new Vector3(0f,0.2f,0f),Quaternion.identity);
        BulletController bCtrl = movingBullet.GetComponent<BulletController>();
        bCtrl.SetBall(bulletSprite);
        bCtrl.Fire(time, vx, vy);
        bCtrl.SetTarget(target);
        gameController.CameraToBullet(this);
		this.PlayAnimation("Fired");
    }


    IEnumerator WaitAndDamage(float time, int damage, bool critical, int remainingBlood){
        yield return new WaitForSeconds(time);
        this.PlayAnimation("Cry");
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
        gameController.mainCamController.Shake();
        ((FightUIController)gameController.uiController).ShowRedFrame();
        UpdateHealthBar(info.team,(float)remainingBlood/originalInfo.blood);
    }
    public virtual void Damaged(int time, int damage, bool critical, int remainingBlood){
        StartCoroutine(WaitAndDamage(time/1000, damage, critical, remainingBlood));
        Debug.Log("damaging, isCritical: "+critical+ " remaining blood: "+ remainingBlood);
        this.info.blood = remainingBlood;
    }

    public virtual void GetTurn(bool isTurn){
        ((FightUIController)gameController.uiController).ResetCurrentProp();
        firedTime = 0;
        if (defaultBulletSprtite == null)
            defaultBulletSprtite = Resources.Load<Sprite>("bullet/hammer");
        bulletSprite = defaultBulletSprtite;
        if (isTurn)
            healthBar.getTurnSprite.gameObject.SetActive(true);
        else
            healthBar.getTurnSprite.gameObject.SetActive(false);
    }

    public virtual void SetPlayerInfo(PlayerInfo inf, GameController gc = null){
        if (gc != null)
            this.gameController = gc;
        originalInfo = inf;
        List<int> styleID = originalInfo.GetStyleList();
        ChangeAppearance(styleID);
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
        SetDander(inf.dander);
    }

    public void resetBullet(){
        staticBullet.SetActive(true);
    }
}
