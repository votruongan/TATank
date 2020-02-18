﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIController : MonoBehaviour
{       
    public virtual void SetUpMainPlayerController(){}
    public virtual void UpdatePlayerInfo(){}
    public virtual void SetArrowButtonDown(string buttonName){}
    public virtual void SetArrowButtonUp(string buttonName){}
    public virtual void SetFightingUI(bool isActive){}
    public virtual void SetLoadingScreen(bool isActive){}

	public virtual void FireClicked(){}
	public virtual void ReleasePower(){}
	public virtual void DisplayYourTurn(){}
    public virtual string FightingPropIdToName(int propId){ return "";}

    public virtual void SelectFightingProp(string propString){}
    public virtual void ConnectHost(){}

    public virtual void SoloPVPMatch(){}
    
    public virtual void CancelMatch(){}
    public virtual void LoginToHost(){}
    protected virtual void ExecLogin(string id, string pass, string host){}

    public virtual void UpdateMainPlayerPreview(PlayerInfo pInfo){}

    protected virtual void UpdateAngle(){}
}