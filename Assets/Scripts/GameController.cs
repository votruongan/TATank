﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using ConnectorSpace;

public class GameController : MonoBehaviour
{
    [Header("FOR DEBUGGING")]
    public bool isOnLocalTest;
	public bool isFrontScene;
    public ConnectorManager connector;
	public ExplosionController explosionController;
	public DigController digController;
	public MainPlayerController mainPlayerController;
	public List<PlayerController> playerControllers; // players other than main player
	public CameraController minimapCamController;
	public CameraController mainCamController;
    public ForegroundController foreController;
    public CountDownController countDownController;
    public UIController uiController;
    public SpriteRenderer backgroundSprite;
	public static string debugString;
	public SoundManager soundManager;
    [Header("PREFABS")]
    public GameObject playerPrefab;
    public GameObject mainPlayerPrefab;

    public static void LogToScreen(string log){
        debugString += "\n" + log;
    }

	void OnGUI(){
		// GUI.TextArea (new Rect (10, 10, Screen.width/3, Screen.height/3), debugString);
	}

#region CONNECTOR_HANDLER
	public GameObject BoxSprite;
    public void PlayerTurn(int pId, List<BoxInfo> newBoxes, List<PlayerInfo> playerList){
    	PlayerController pCtrl = SearchPlayerId(pId);
    	if (pCtrl == null){
    		return;
    	}
    	if (pId == mainPlayerController.info.id){
			uiController.DisplayYourTurn();
	    	mainPlayerController.GetTurn(true);
        	countDownController.ToggleObject(true);
    	}
		else{
			mainPlayerController.GetTurn(false);
        	countDownController.ToggleObject(false);
    	}
		foreach(PlayerInfo p in playerList){
			PlayerController setPCtrl = SearchPlayerId(p.id);
			if (setPCtrl == null)
				continue;
			setPCtrl.UpdatePlayerInfo(p);
		}
		foreach(BoxInfo box in newBoxes){

		}
    	mainCamController.TranslateTo(pCtrl.transform.position);
    }

	public void BombExplodeAt(int time, int pId,int ePPosX,int ePPosY){
		Vector3 explodePos = foreController.PixelToWorldPosition(ePPosX,ePPosY,true);
		explosionController.DelayedExecute(time,explodePos);
		digController.DelayedExecute(time,explodePos);
	}


	PlayerController SearchPlayerId(int pId){
		PlayerController pCtrl = null;
		if (pId == mainPlayerController.info.id){
		 	pCtrl = mainPlayerController;
		}else{
			foreach(PlayerController pC in playerControllers){
				if (pC.info.id == pId){
					pCtrl = pC;
					break;
				}
			}
		}
		return pCtrl;
	}
	public void PlayerDamage( int delayedTime, int pId, int damage,bool critical, int remainingBlood){
		PlayerController pCtrl = SearchPlayerId(pId);
		if (pCtrl == null)
			return;
		pCtrl.Damaged(delayedTime,damage,critical,remainingBlood);
	}

    // add the destination to explode
    public void PlayerFire(int pId, int time, int pVx, int pVy){
		PlayerController pCtrl = SearchPlayerId(pId);
		if (pCtrl == null)
			return;
		float vx = pVx / 100;
		float vy = -pVy / 100;
		pCtrl.PlayAnimation("PlayerFired");
		pCtrl.Fire(time,vx,vy);
    	mainCamController.LockTo(pCtrl.movingBullet.transform);
    }


    public void PlayerDirection(int pId, int dir){
		PlayerController pCtrl = SearchPlayerId(pId);
		if (pCtrl == null)
			return;
		if ((pCtrl.isHeadingRight && dir == 255)
			|| (!pCtrl.isHeadingRight && dir == 1)){
			pCtrl.RotateLiving();
		}
    }


    public void PlayerMove(int pId, int x, int y, byte dir){

		// if (pId == mainPlayerController.info.id){
		// 	return;
		// }

		PlayerController pC = SearchPlayerId(pId);
		if (pC == null)
			return;
		Vector2 pPos = new Vector2(x,y);
		Vector3 pos = foreController.PixelToWorldPosition(x,y,true);
    	pC.MoveTo(pos,dir);
    }
    public void PlayerUsingProp(int pId, byte propType,int place,int templateId){
		PlayerController pC = SearchPlayerId(pId);
		if (pC == null)
			return;
			
		//254, -2, propId
		if (propType == 254 && place == -2){
			Debug.Log("Go Fighting Prop " + uiController.FightingPropIdToName(templateId));
    		pC.UsingFightingProp(uiController.FightingPropIdToName(templateId));
		}

    }
    public void PlayerFly(int time, int pId, int m_x,int m_y){
		PlayerController pC = SearchPlayerId(pId);
		if (pC == null)
			return;
		explosionController.CancelDelayedExecution();
		digController.CancelDelayedExecution();
		Vector3 toPos = foreController.PixelToWorldPosition(m_x,m_y,true);
		pC.Teleport(time/1000,toPos);		
    }

    public void AddFireTag(int pId, byte speedTime){
    	foreach(PlayerController pC in playerControllers){
    		if (pC.info.id == pId){
    			pC.AddFireTag(speedTime);
    			break;
    		}
    	}
    }


	public void LoadMap(int mapId)
	{
		uiController.SetLoadingScreen(true);
		string path = "map/"+mapId.ToString()+"/";
		//Change background according to mapid
		backgroundSprite.sprite = Resources.Load<Sprite>(path+"back");
		//Change foreground and dead sprite, and update their collider
		foreController.LoadMap(path);
		soundManager.LoadAndPlay(mapId.ToString());
		backgroundSprite.transform.position = foreController.transform.position;
		digController.CheckIfDiggable();
		digController.isDiggable  = false;
		mainCamController.UpdateClamp(foreController.transform.position,foreController.bottomLeftPosition);
		minimapCamController.TranslateTo(foreController.transform.position);
	}
	public void GameOver()
	{
		Debug.Log("[GAMECONTROLLER] GAME OVER");
		// foreach(PlayerController p in playerControllers){
		// 	Destroy(p.gameObject);
		// }
		uiController.SetFightingUI(false);
		SwitchScene("Scene_Game","Scene_Front");
	}	


	public void GameCreate(List<PlayerInfo> Players)
	{
		Debug.Log("[GAMECONTROLLER] GAME CREATE");
		// foreach(PlayerInfo p in Players){
		// 	Debug.Log(p.ToString());
		// }
		uiController.SetLoadingScreen(true);
		SwitchScene("Scene_Front","Scene_Game");
	}	

	
    void SwitchScene(string from, string to){
        Scene temp = SceneManager.CreateScene("temp");
        StartCoroutine(ExecSwitchScene(from,to));
    }
    IEnumerator ExecSwitchScene(string from, string to){
        yield return null;
        AsyncOperation lao = SceneManager.LoadSceneAsync(to,LoadSceneMode.Additive);
        lao.allowSceneActivation = false;
        while (!lao.isDone){
            if (lao.progress >= 0.9f)
            {
                Debug.Log("load done");
                break;
            }
        }
        AsyncOperation uao = SceneManager.UnloadSceneAsync(from);
        lao.allowSceneActivation = true;
        SceneManager.UnloadSceneAsync("temp");
    }

	public void StartGame(List<PlayerInfo> Players)
	{
		foreach(PlayerInfo p in Players){
			Debug.Log(p.ToString());
		}
		//Remove all Spawned players on scene
		foreach (GameObject pOb in GameObject.FindGameObjectsWithTag("Player")){
			Destroy(pOb);
		}

		uiController.SetFightingUI(true);
		playerControllers = new List<PlayerController>();
		foreach (PlayerInfo p in Players){
			Debug.Log(p.ToString());
			//pos is in pixel
			Vector2 pPos = new Vector2(p.x, p.y);
			//Get Unity unit of a pixel at position pos on Map
			Vector3 pos = foreController.PixelToWorldPosition(pPos,true);
			pos += new Vector3(0f,0.2f,0f);
			GameObject pOb;
			PlayerController pCtrl = null;
			//Set player info and GameController	
			if (p.isMainPlayer){
				pOb = Instantiate(mainPlayerPrefab, pos, Quaternion.identity);
				mainPlayerController = pOb.GetComponent<MainPlayerController>();
				Debug.Log(mainPlayerController);
				pCtrl = mainPlayerController;
			}else{ 
				pOb = Instantiate(playerPrefab, pos, Quaternion.identity);
				pCtrl = pOb.GetComponent<PlayerController>();
				playerControllers.Add(pOb.GetComponent<PlayerController>());
			}

			if (pCtrl != null){
				pCtrl.SetPlayerInfo(p,this);
			}
		}
		mainCamController.TranslateTo(mainPlayerController.transform.position);
		uiController.SetUpMainPlayerController();
		Debug.Log("UpdateMainPlayerController called");
		uiController.SetLoadingScreen(false);
	}	
#endregion



#region SIGNAL_FROM_MAIN_PLAYER 
	public void StartMatch(){
		connector.StartMatch();
	}
	public void StopMatch(){
		connector.StopMatch();
	}	

    public void MainPlayerFire(int force, int angle){
		PlayerController enemyControl = mainPlayerController;
		foreach(PlayerController pc in playerControllers){
			if (pc.info.id != mainPlayerController.info.id){
				enemyControl = pc;
				break;
			}
		}
    	Vector2 pPos = foreController.WorldPositionToPixel(mainPlayerController.transform.position, true);
		if (isOnLocalTest){
			Debug.Log("Main Player Fired: " + ((int)(mainPlayerController.isHeadingRight?(byte)1:(byte)255)).ToString());
			Debug.Log("Main Player Fired: " + ((int)pPos.x).ToString() + ", "+
						 ((int)pPos.y).ToString() + ", " + force.ToString()+ ", " + angle.ToString());
			return;
		}
		connector.SendDirection(mainPlayerController.isHeadingRight?(byte)1:(byte)255);
		//Send ShootTag and Shoot packet
    	connector.SendShoot((int)pPos.x,(int)pPos.y,force,angle);
    }

    public void MainPlayerMove(float tx, float ty, bool isHeadingRight){
    	//get player pixel position on map
    	Vector2 pPos = foreController.WorldPositionToPixel(tx,ty,true);
    	//send position to connector
		if (isOnLocalTest){
			Debug.Log("Main Player Moved: " + ((int)pPos.x).ToString() + ", "+
						 ((int)pPos.y).ToString() + ", " + ((int)(isHeadingRight?(byte)1:(byte)255)).ToString());
			return;
		}
    	connector.SendMove((int)pPos.x,(int)pPos.y,isHeadingRight?(byte)1:(byte)255);
    }
    public void MainPlayerSkip(){
    	connector.SendSkip();
		mainPlayerController.GetTurn(false);
    }

	public void SendUsingFightingProp(int propId){
		if (mainPlayerController.isOnTurn)
		{
			connector.SendUsingProp(254,-2,propId);
			return;
		}
		//Handle when using prop not on turn
	}
#endregion

	public bool ConnectHost(string hostIp){
		return ConnectHost("bot0","123456",hostIp);
	}

	public bool ConnectHost(string id, string pass, string hostIp){
		if (connector == null){
			connector = GameObject.Find("ConnectorManager").GetComponent<ConnectorManager>();	
		}
		bool res = connector.ConnectToHost(id, pass, hostIp);
		return res;
	}

	public void SendMainPlayerPos(){
		float tx = mainPlayerController.transform.position.x;
		float ty = mainPlayerController.transform.position.y;
		Vector2 pPos = foreController.WorldPositionToPixel(tx,ty,true);
		Debug.Log("Main player pos:" + pPos.ToString());
    	connector.SendMove((int)pPos.x,(int)pPos.y,mainPlayerController.isHeadingRight?(byte)1:(byte)255);
	}

#region MONOBEH_IMPLEMENTS
	void Start(){
		Scene tmpScene = SceneManager.GetSceneByName("temp");
		if (tmpScene.IsValid())		
			SceneManager.UnloadSceneAsync("temp");
		// if (connector == null){
		// 	connector = GameObject.Find("ConnectorManager").GetComponent<ConnectorManager>();
		// }
		GameObject g = GameObject.Find("ConnectorManager");
		if (g == null){
			connector.gameObject.SetActive(true);
			soundManager.gameObject.SetActive(true);
			GameObject.DontDestroyOnLoad(connector.gameObject);
			GameObject.DontDestroyOnLoad(soundManager.gameObject);
		} else {
			connector = g.GetComponent<ConnectorManager>();
			soundManager = GameObject.Find("SoundManager").GetComponent<SoundManager>();
		}
		connector.gameController = this;
		soundManager.LoadAndPlay("bgm");
		if (uiController == null){
			uiController = GameObject.Find("UIController").GetComponent<UIController>();
			uiController.SetLoadingScreen(true);
		}
		try {
			if (foreController == null){
				foreController = GameObject.Find("Foreground").GetComponent<ForegroundController>();	
			}
			if (backgroundSprite == null){
				backgroundSprite = GameObject.Find("Background").GetComponent<SpriteRenderer>();	
			}
			// no ExplosionController assigned
			if (explosionController == null){
				try{
					explosionController = GameObject.Find("ExplosionController").GetComponent<ExplosionController>();
				}
				catch(Exception e){
					Debug.Log("No explosionController : " + e.ToString());
				}
			}
			if (mainCamController == null){
				mainCamController = GameObject.Find("MainCamera").GetComponent<CameraController>();	
			}
			if (minimapCamController == null){
				minimapCamController = GameObject.Find("MinimapCamera").GetComponent<CameraController>();	
			}
			if (countDownController == null){
				countDownController = GameObject.Find("CountDownController").GetComponent<CountDownController>();
			}
			countDownController.ToggleObject(false);
			// no DigController assigned	
			if (digController == null){
				try{
					digController = GameObject.Find("DigController").GetComponent<DigController>();
				}
				catch(Exception e){
					Debug.Log("No digController : " + e.ToString());
				}
			}
			connector.SendLoadComplete();
		} catch(Exception e){
			Debug.Log("Not Fight Scene : " + e.ToString());
		}
		//string path = "map/1028/";
		//Debug.Log(Resources.Load<Sprite>(path+"back"));
	}
#endregion
	public ClientConnector GetClientConnector(){
		return connector.GetClientConnector();
	}
	public PlayerInfo GetLocalPlayerInfo(){
		return GetClientConnector().GetLocalPlayerInfo();
	}
}
