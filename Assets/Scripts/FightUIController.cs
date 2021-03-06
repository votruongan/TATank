﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FightUIController : UIController
{
    [Header("SETTINGS")]
    public GameObject yourTurnPanel;
    public GameObject loadingScreen;
    public GameObject fightUI;
    public CurrentPropDisplayer currentPropDisplayer;
    public GameObject redFrameObject;
    public DanderEffectScreen danderScreen;
    [Header("FOR DEBUG")]
    public MainPlayerController mainPlayerController;
    public bool isTakePower;
    public int power;
    public int angle;
    public Dictionary<string, int> propDict  = new Dictionary<string, int>()
            {    
                {"X2",10001},{"X1",10002},{"S3",10003},{"P50",10004},
                {"P40",10005},{"P30",10006},{"P20",10007},{"P10",10008}
            };
    public PlayerPreviewLoader RedPlayerPreview;
    public PlayerPreviewLoader BluePlayerPreview;
    public SummaryPanelController summaryPanelController;
    public Button[] InteractableButtons;

    public void LoadRedPlayerPreview(PlayerInfo inf){
        if (inf == null)
            return;
        RedPlayerPreview.gameObject.SetActive(true);    
        RedPlayerPreview.LoadFromInfo(inf);
        danderScreen.PrepareLoader(1,inf);
		// Debug.Log("-- -- -- Load Red: " + inf);
    }
    
    public void LoadBluePlayerPreview(PlayerInfo inf){
        if (inf == null)
            return;
        BluePlayerPreview.gameObject.SetActive(true);
        BluePlayerPreview.LoadFromInfo(inf);
        danderScreen.PrepareLoader(2,inf);
		// Debug.Log("-- -- -- LoadBlue: " + inf);
    }
    public void PlayDanderScreen(bool isHeadingRight, PlayerInfo inf){
        // string t = (team == 1)?("RED"):("BLUE");
        danderScreen.CallDanderScreen(isHeadingRight,inf);
    }

    // Start is called before the first frame update
    void Start()
    {
        if(gameController == null)
            gameController = GameObject.Find("GameController").GetComponent<GameController>();
        if(yourTurnPanel == null)
            yourTurnPanel = GameObject.Find("YourTurnPanel");
        if(loadingScreen == null)
            loadingScreen = GameObject.Find("LoadingScreen");
        if (propDict == null){
            propDict = new Dictionary<string, int>()
            {    
                {"X2",10001},{"X1",10002},{"S3",10003},{"P50",10004},
                {"P40",10005},{"P30",10006},{"P20",10007},{"P10",10008}
            };
        }
    }

    // public void SetArrowButtonState(string buttonName,bool state){
    //     switch(buttonName){
    //         case "up":
    //             gameController.mainPlayerController.isUpArrowDown = state;
    //         break;
    //         case "down":
    //             gameController.mainPlayerController.isDownArrowDown = state;
    //         break;
    //         case "left":
    //             gameController.mainPlayerController.isLeftKeyDown = state;
    //         break;
    //         case "right":
    //             gameController.mainPlayerController.isRightKeyDown = state;
    //         break;
    //     }
    // }
    public override void SetUpMainPlayerController(){
        mainPlayerController = gameController.mainPlayerController;
        mainPlayerController.powerIndicator = GameObject.Find("PowerIndicator").GetComponent<Slider>();
        mainPlayerController.prevPowerIndicator = GameObject.Find("PreviousPowerIndicator").GetComponent<Slider>();
        mainPlayerController.handRotator = GameObject.Find("AngleHand").GetComponent<HandRotator>();
    }

    public override void SetArrowButtonDown(string buttonName){
        if (mainPlayerController == null)
            return;
        switch(buttonName){
            case "up":
                mainPlayerController.isUpArrowDown = true;
            break;
            case "down":
                mainPlayerController.isDownArrowDown = true;
            break;
            case "left":
                mainPlayerController.isLeftArrowDown = true;
                mainPlayerController.BeginMove();
            break;
            case "right":
                mainPlayerController.isRightArrowDown = true;
                mainPlayerController.BeginMove();
            break;
        }
    }
    public override void SetArrowButtonUp(string buttonName){
        if (mainPlayerController == null)
            return;
        switch(buttonName){
            case "up":
                mainPlayerController.isUpArrowDown = false;
            break;
            case "down":
                mainPlayerController.isDownArrowDown = false;
            break;
            case "left":
                mainPlayerController.isLeftArrowDown = false;
                mainPlayerController.FinishMove();
            break;
            case "right":
                mainPlayerController.isRightArrowDown = false;
                mainPlayerController.FinishMove();
            break;
        }
    }
    public override void SetFightingUI(bool isActive){
		this.SetLoadingScreen(false);
        // mainPanel.SetActive(!isActive);
        if (fightUI != null)
            fightUI.SetActive(isActive);
        if (isActive){
            // gameController.countDownController = GameObject.Find("CountDownController").GetComponent<CountDownController>();
        }
    }
    public override void SetLoadingScreen(bool isActive){
        if (loadingScreen != null)
        loadingScreen.SetActive(isActive);
        // countUpUI.SetActive(!isActive);
    }

	public override void FireClicked()
	{
        if (mainPlayerController == null)
            return;
        // gameController.soundManager.PlayEffect("releasePower");
        // powerAudio = gameController.soundManager.PlayEffect("takePower");
        mainPlayerController.BeginTakePower();
        isTakePower = true;
	}
	public override void ReleasePower()
	{
        if (mainPlayerController == null)
            return;
        isTakePower = false;
        // Destroy(powerAudio);
        // gameController.soundManager.PlayEffect("startFire");
        mainPlayerController.ReleasePower();
	}

	public override void DisplayYourTurn()
	{
	    yourTurnPanel.SetActive(true);
	}

    public override string FightingPropIdToName(int propId){
        foreach(KeyValuePair<string,int> kvp in propDict){
            // Debug.Log("kvp: "+ kvp);
            if (kvp.Value == propId){
                return kvp.Key;
            }
        }
        return "";
    }

    int times = 0;
    Dictionary<string,bool> buttonsInteractable;

    public void ResetPropInteractable(){
        foreach(Button b in InteractableButtons){
            b.interactable = true;
        }
    }
    public void UpdatePropInteractable(int energyLeft){
        foreach(Button b in InteractableButtons){
            string prpName = b.name.Split('_')[1];
            if (energyLeft < GetPropEnergyUsage(prpName)){
                b.interactable = false;
            }
        }
    }

    public void SetButtonInteractable(string bName, bool state){
        if (buttonsInteractable == null){
            buttonsInteractable = new Dictionary<string,bool>();
            foreach(Button b in InteractableButtons){
                buttonsInteractable[b.name] = b.gameObject.activeInHierarchy;
            }
        }
        buttonsInteractable[bName] = state;
        times++;
        if (times <= 1)
        StartCoroutine(ExecSetButtonInteractable());
    }

    IEnumerator ExecSetButtonInteractable(){  
        while (times > 0)
        {
            yield return new WaitForSeconds(0.2f);
            foreach(Button b in InteractableButtons){
                b.interactable = buttonsInteractable[b.name];
                yield return null;
            }            
            times--;
        }      
    }

    public void SelectQuitGame(){
        gameController.KillSelf();
    }

    public static int GetPropEnergyUsage(string propString){
        switch(propString){
            case "P50":
                return 85; break;
            case "P40":
                return 80; break;
            case "P30":
                return 70; break;
            case "P20":
                return 55; break;
            case "P10":
                return 50; break;
            default:
                return 110; break;
        }
    }

    public void ShowRedFrame(){
        redFrameObject.SetActive(true);
    }
    public void AppendCurrentProp(Texture textur){
        currentPropDisplayer.Append(textur);
    }
    public void ResetCurrentProp(){
        currentPropDisplayer.Reset();
    }
    public override void SelectFightingProp(string propString){
        int ene = GetPropEnergyUsage(propString);
        if (!mainPlayerController.UseEnergy(ene)){
            return;
        }
        int propId = 0;
        try{
            propId = propDict[propString];
        } catch (KeyNotFoundException e){}
        Debug.Log("propID: "+ propId + " name: " + FightingPropIdToName(propId));
        if (propString == "FLY"){
            SetButtonInteractable("Prop_FLY_Button",false);
            gameController.connector.SendUsingFly();
            return;
        }
        mainPlayerController.UsingFightingProp(propString,propId);
    }

    // public override void UpdateMainPlayerPreview(PlayerInfo pInfo){
    //     GameObject.Find("MainPlayerPreview").GetComponent<PlayerPreviewLoader>().LoadFromInfo(pInfo);
    // }

    void FixedUpdate(){
        if (isTakePower){
            mainPlayerController.TakePower();
        }
    }

    protected override void UpdateAngle(){
        if (mainPlayerController != null){
            mainPlayerController.TakePower();
        }
    }
}
