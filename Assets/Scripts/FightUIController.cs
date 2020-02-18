﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FightUIController : UIController
{
    [Header("SETTINGS")]
    public GameController gameController;
    public GameObject yourTurnPanel;
    public GameObject loadingScreen;
    public GameObject fightUI;
    public NotiPanelController notiPanel;
    [Header("FOR DEBUG")]
    public MainPlayerController mainPlayerController;
    public bool isTakePower;
    public int power;
    public int angle;
    public Dictionary<string, int> propDict;
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
        fightUI.SetActive(isActive);
        if (isActive){
            gameController.countDownController = GameObject.Find("CountDownController").GetComponent<CountDownController>();
        }
    }
    public override void SetLoadingScreen(bool isActive){
        loadingScreen.SetActive(isActive);
        // countUpUI.SetActive(!isActive);
    }

	public override void FireClicked()
	{
        if (mainPlayerController == null)
            return;
        mainPlayerController.BeginTakePower();
        isTakePower = true;
	}
	public override void ReleasePower()
	{
        if (mainPlayerController == null)
            return;
        isTakePower = false;
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

    public override void SelectFightingProp(string propString){
        int propId = propDict[propString];
        Debug.Log("propID: "+ propId + " name: " + FightingPropIdToName(propId));
        mainPlayerController.UsingFightingProp(propString,propId);
    }

    public override void UpdateMainPlayerPreview(PlayerInfo pInfo){
        GameObject.Find("MainPlayerPreview").GetComponent<PlayerPreviewLoader>().LoadFromInfo(pInfo);
    }

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