using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using ConnectorSpace;

public class GameController : MonoBehaviour
{
	public float G = 9.8f;
	[Header("For References")]
	public bool isStarted = false;
    [Header("FOR DEBUGGING")]
    public bool isOnLocalTest = false;
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
	private int[] playerIdCache;
    [Header("DEBUG")]
	bool isOnDander;
	public static GameController instance;

	public static GameController GetInstance(){
		return instance;
	}

    public static void LogToScreen(string log){
        debugString += "\n" + log;
    }

	void OnGUI(){
		// GUI.TextArea (new Rect (10, 10, Screen.width/3, Screen.height/3), debugString);
	}

#region CONNECTOR_HANDLER
	public GameObject BoxSprite;
    public void PlayerTurn(int pId, List<BoxInfo> newBoxes, List<PlayerInfo> playerList){
		PlayerController pCtrl = null;
		isOnDander = false;
		ExplosionController.GetInstance().ResetQueue();
    	if (pId == mainPlayerController.info.id){
			uiController.DisplayYourTurn();
	    	mainPlayerController.GetTurn(true);
        	countDownController.ToggleObject(true);
			pCtrl = mainPlayerController;
    	}
		else{
			mainPlayerController.GetTurn(false);
        	countDownController.ToggleObject(false);
    	}
		foreach(PlayerInfo p in playerList){
			PlayerController setPCtrl = FindPlayerById(p.id);
			if (setPCtrl == null)
				continue;
			setPCtrl.UpdatePlayerInfo(p);
			if (p.id != pId){
				setPCtrl.GetTurn(false);
			}else{
				pCtrl = setPCtrl;
				setPCtrl.GetTurn(true);
			}
		}
		foreach(BoxInfo box in newBoxes){

		}
    	mainCamController.LockTo(pCtrl.transform);
    }

	public void BombExplodeAt(int time, int pId,int ePPosX,int ePPosY){
		Vector3 explodePos = foreController.PixelToWorldPosition(ePPosX,ePPosY,true);
		int offset = (isOnDander)?(2200):(0);
		// explosionController.DelayedExecute(time+offset,explodePos);
		ExplosionController.GetInstance().WillExplode(explodePos);
		digController.DelayedExecute(time+offset,explodePos);
	}

	public void BombExplodeAt(Vector3 pos){
		ExplosionController.GetInstance().ExplodeNearest(pos);
	}

	PlayerController FindPlayerById(int pId){
		PlayerController pCtrl = null;
		if (pId == mainPlayerController.info.id){
		 	pCtrl = mainPlayerController;
		}else{
			if (playerIdCache == null){
				playerIdCache = new int[]{-1,-1,-1,-1,-1,-1,-1,-1};
				for(int i = 0; i < playerControllers.Count; i++){
					playerIdCache[i] = playerControllers[i].info.id;
				}
			}
			for(int i = 0; i < playerControllers.Count; i++){
				if (playerIdCache[i] == pId){
					pCtrl = playerControllers[i];
				}
			}

		}
		return pCtrl;
	}
	public void PlayerDander(int pId, int dander){
		PlayerController pCtrl = FindPlayerById(pId);
		if (pCtrl == null)
			return;		
		if (pId == mainPlayerController.info.id){
			mainPlayerController.SetDander(dander);
			return;
		}
		pCtrl.SetDander(dander);
	}

	public void PlayerUpdateBall(int pId, bool special, int ballId){
		Debug.Log("[GameController] CURRENTBALL - special:" + special);
		PlayerController pCtrl = FindPlayerById(pId);
		if (special){
			//perform player using dander
			isOnDander = true;
			pCtrl.SetBullet("special");
			pCtrl.UsingFightingProp("DANDER");
			return;
		}
	}

	public void PlayDanderScreen(bool headingRight, int team){
		((FightUIController)uiController).PlayDanderScreen(headingRight,team);
	}

	private int delayCameraCount = 0;

	public void CameraToBullet(PlayerController pCtrl){
		if (delayCameraCount == 0){
    		mainCamController.LockTo(pCtrl.movingBullet.transform);
		} else {
			delayCameraCount --;
		}
	}

	public void PlayerDamage( int delayedTime, int pId, int damage,bool critical, int remainingBlood){
		PlayerController pCtrl = FindPlayerById(pId);
		if (pCtrl == null)
			return;
		int offset = (isOnDander)?(2400):(400);
		pCtrl.Damaged(delayedTime + offset,damage,critical,remainingBlood);
	}
    // add the destination to explode
    public void PlayerFire(int pId, int time, int pVx, int pVy, int targX, int targY){
		PlayerController pCtrl = FindPlayerById(pId);
		Vector3 targPos = foreController.PixelToWorldPosition(targX,targY,true);
		if (pCtrl == null)
			return;
		float vx = (float)pVx / 100;
		float vy = (float)-pVy / 100;
		pCtrl.PlayAnimation("PlayerFiring");
		pCtrl.Fire(time,vx,vy,targPos);
    }
    public void PlayerDirection(int pId, int dir){
		PlayerController pCtrl = FindPlayerById(pId);
		if (pCtrl == null)
			return;
		if ((pCtrl.isHeadingRight && dir == 255)
			|| (!pCtrl.isHeadingRight && dir == 1)){
			pCtrl.RotateLiving();
		}
    }
	 public void PlayerMove(int pId, int x, int y, byte dir){
		if (pId == mainPlayerController.info.id){
			return;
		}
		PlayerController pC = FindPlayerById(pId);
		if (pC == null)
			return;
		Vector2 pPos = new Vector2(x,y);
		Vector3 pos = foreController.PixelToWorldPosition(x,y,true);
    	pC.MoveTo(pos,dir);
    }
	
    public void PlayerUsingProp(int pId, byte propType,int place,int templateId){
		PlayerController pC = FindPlayerById(pId);
		if (pC == null)
			return;
		string propName = uiController.FightingPropIdToName(templateId);
		if (propName == "X2"){
			delayCameraCount = 2;
		} if (propName == "X1"){
			delayCameraCount = 1;			
		}
		Debug.Log("Go Fighting Prop: name:"+ propName +" type:" + propType + " place:"+ place);
		//254, -2, propId
		if (propType == 254 && place == -2){
    		pC.UsingFightingProp(propName);
		}

    }
    public void PlayerFly(int time, int pId, int m_x,int m_y){
		PlayerController pC = FindPlayerById(pId);
		pC.SetBullet("fly");
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
	int loadCount = 0;
	public void LoadComplete(){
		loadCount++;
		Debug.Log("-- -- -- LoadComplete: " + loadCount);
		if (loadCount > 0){
			connector.SendLoadComplete();
			// Debug.Log(" fore:" + foreController + " mainCam:"+ mainCamController);
			// Debug.Log(" botl:" + foreController.bottomLeftPosition + " topr:"+ foreController.topRightPosition);
			StartCoroutine(WaitAndUpdateCamera(foreController.bottomLeftPosition,foreController.topRightPosition));
			// minimapCamController.TranslateTo(foreController.transform.position);
		}
	}

	IEnumerator WaitAndUpdateCamera(Vector3 clampMin, Vector3 clampMax){
		yield return new WaitForSeconds(0.2f);
		if (mainCamController == null)
			mainCamController = GameObject.Find("MainCamera").GetComponent<CameraController>();
		if (foreController == null)
			foreController = GameObject.Find("Foreground").GetComponent<ForegroundController>();
		mainCamController.UpdateClamp(foreController.bottomLeftPosition,foreController.topRightPosition);
	}

	public void LoadMap(int mapId)
	{
		uiController.SetLoadingScreen(true);
		string path = "map/"+mapId.ToString()+"/";
		//Change background according to mapid
		backgroundSprite.sprite = Resources.Load<Sprite>(path+"back");
		//Change foreground and dead sprite, and update their collider
		foreController.LoadMapOnline(mapId);
		// foreController.LoadMap(path);
		soundManager.PlayBGM(mapId.ToString());
		backgroundSprite.transform.position = foreController.transform.position;
		digController.CheckIfDiggable();
		// digController.isDiggable  = false;
	}
	public void GameOver(MatchSummary ms)
	{
		Debug.Log("[GAMECONTROLLER] GAME OVER");
		// foreach(PlayerController p in playerControllers){
		// 	Destroy(p.gameObject);
		// }
		uiController.SetFightingUI(false);
		((FightUIController)uiController).summaryPanelController.DisplaySummary(ms);
		// SwitchScene("Scene_Game","Scene_Front");
		StartCoroutine(TimedEndGame(17f));
	}

	public void ConfirmMatching()
	{
		((CommonUIController)uiController).ConfirmOpenMatch();
	}	

	IEnumerator TimedEndGame(float secs){
		yield return new WaitForSeconds(secs);
		connector.connector.SendLogOut();
		SwitchScene("Scene_Game","Scene_Front");
	}


	public void GameCreate(List<PlayerInfo> Players)
	{
		Debug.Log("[GAMECONTROLLER] GAME CREATE");
		// foreach(PlayerInfo p in Players){
		// 	Debug.Log(p.ToString());
		// }
		uiController.SetLoadingScreen(true);
		connector.playerInfos = Players;
		isStarted = false;
		if (isOnLocalTest)
			return;
		SwitchScene("Scene_Front","Scene_Game");
	}	

	
    void SwitchScene(string from, string to){
        Scene temp = SceneManager.CreateScene("temp");
		connector.isSceneTransforming = true;
        StartCoroutine(ExecSwitchScene(from,to));
    }
    IEnumerator ExecSwitchScene(string from, string to){
        AsyncOperation lao = SceneManager.LoadSceneAsync(to,LoadSceneMode.Additive);
        lao.allowSceneActivation = false;
        while (!lao.isDone){
            if (lao.progress >= 0.9f)
            {
                Debug.Log("load scene done");
                break;
            }
        	yield return null;
        }
        AsyncOperation uao = SceneManager.UnloadSceneAsync(from);
        lao.allowSceneActivation = true;
        SceneManager.UnloadSceneAsync("temp");
    }

	public void StartGame(List<PlayerInfo> Players)
	{
		// foreach(PlayerInfo p in Players){
		// 	Debug.Log(p.ToString());
		// }
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
			if (uiController == null){
				uiController = GameObject.Find("UIController").GetComponent<FightUIController>();
			}
			if (p.team == 2){
				((FightUIController)uiController).LoadBluePlayerPreview(p);
			} else{
				((FightUIController)uiController).LoadRedPlayerPreview(p);
			}
			//Set player info and GameController	
			if (p.isMainPlayer){
				pOb = Instantiate(mainPlayerPrefab, pos, Quaternion.identity);
				mainPlayerController = pOb.GetComponent<MainPlayerController>();
				// Debug.Log(mainPlayerController);
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
		// Debug.Log("UpdateMainPlayerController called");
		uiController.SetLoadingScreen(false);
	}	
#endregion



#region SIGNAL_FROM_MAIN_PLAYER 
	public void KillSelf(){
		connector.connector.SendLogOut();
		if (this == null)
			return;
		StartCoroutine(TimedEndGame(2f));
	}
	public void StartMatch(){
		connector.StartMatch();
	}
	public void StopMatch(){
		connector.StopMatch();
	}	

	public void SendUsingFly(){
		connector.SendUsingFly();
	}

	public void SendPlayerDander(){
		// connector.SendSecondWeapon();
		if (mainPlayerController.isOnTurn)
			connector.SendStunt();
	}

    public void SendPlayerFire(int force, int angle){
		// PlayerController enemyControl = mainPlayerController;
		// foreach(PlayerController pc in playerControllers){
		// 	if (pc.info.id != mainPlayerController.info.id){
		// 		enemyControl = pc;
		// 		break;
		// 	}
		// } 
    	Vector2 pPos = foreController.WorldPositionToPixel(mainPlayerController.transform.position, true);
		// if (isOnLocalTest){
		// 	Debug.Log("Main Player Fired: " + ((int)(mainPlayerController.isHeadingRight?(byte)1:(byte)255)).ToString());
		// 	Debug.Log("Main Player Fired: " + ((int)pPos.x).ToString() + ", "+
		// 				 ((int)pPos.y).ToString() + ", " + force.ToString()+ ", " + angle.ToString());
		// 	return;
		// }
		connector.SendDirection(mainPlayerController.isHeadingRight?(byte)1:(byte)255);
		//Send ShootTag and Shoot packet
		// byte tim = ((byte)(countDownController.initSeconds - ((int)countDownController.timer)));
		byte tim = ((byte)(int)countDownController.timer);
    	connector.SendShoot((int)pPos.x,(int)pPos.y-30,force,angle,(byte)tim);
    }

    public void MainPlayerMove(float tx, float ty, bool isHeadingRight){
    	//get player pixel position on map
    	Vector2 pPos = foreController.WorldPositionToPixel(tx,ty,true);
    	//send position to connector
		// if (isOnLocalTest){
		// 	Debug.Log("Main Player Moved: " + ((int)pPos.x).ToString() + ", "+
		// 				 ((int)pPos.y).ToString() + ", " + ((int)(isHeadingRight?(byte)1:(byte)255)).ToString());
		// 	return;
		// }
    	connector.SendMove((int)pPos.x,(int)pPos.y,isHeadingRight?(byte)1:(byte)255);
    }
    public void MainPlayerSkip(){
    	connector.SendSkip();
		mainPlayerController.GetTurn(false);
    }

	public void SendUsingFightingProp(int propId){
		// Debug.Log("SendUsingFightingProp: " + propId.ToString());
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
		// if (connector == null){
		// 	connector = GameObject.Find("ConnectorManager").GetComponent<ConnectorManager>();
		// }
		instance = this;
		GameObject g = GameObject.Find("ConnectorManager");
		if (g == null){
			connector.gameObject.SetActive(true);
			soundManager.gameObject.SetActive(true);
			GameObject.DontDestroyOnLoad(connector.gameObject);
			GameObject.DontDestroyOnLoad(soundManager.gameObject);
			isOnLocalTest =	true;
		} else {
			connector = g.GetComponent<ConnectorManager>();
			soundManager = GameObject.Find("SoundManager").GetComponent<SoundManager>();
		}
		connector.gameController = this;
		soundManager.PlayBGM("062");
		if (uiController == null){
			uiController = GameObject.Find("UIController").GetComponent<UIController>();
			uiController.SetLoadingScreen(true);
		}
		try {
			playerIdCache = null;
			if (foreController == null){
				Debug.Log("Finding bg: " + GameObject.Find("Foreground"));
				foreController = GameObject.Find("Foreground").GetComponent<ForegroundController>();	
			}
			if (backgroundSprite == null){
				Debug.Log("Finding bg: " + GameObject.Find("Background"));
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
				mainCamController.UpdateClamp(foreController.bottomLeftPosition,foreController.topRightPosition);
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
			foreach(PlayerInfo inf in connector.playerInfos){
				// Debug.Log(inf);
				if (inf.team == 2){
					((FightUIController)uiController).LoadBluePlayerPreview(inf);
				} else{
					((FightUIController)uiController).LoadRedPlayerPreview(inf);
				}
			}
			// if (isOnLocalTest){
			// 	bool res = ConnectHost("127.0.0.1");
			// 	if (res){
			// 		StartMatch();
			// 		// isOnLocalTest = false;
			// 	}else{
			// 		Debug.Log(" Cannot simulate login ");
			// 	}
			// }
			Debug.Log("Fight Scene ");
		} catch(Exception e){
			isOnLocalTest = false;
			Debug.Log("Not Fight Scene : " + e.ToString());
		}
		isStarted = true;
		connector.isSceneTransforming = false;
		Scene tmpScene = SceneManager.GetSceneByName("temp");
		if (tmpScene.IsValid()){
			SceneManager.UnloadSceneAsync("temp");
		}
		Debug.Log("[GameController] START DONE");
	}
#endregion

    public void ChangeHostIp(Text txt){
        connector.ChangeHostIp(txt.text);
		GameObject.Find("ChangeHostPanel").SetActive(false);
		GameObject.Find("MainLoginPanel").SendMessage("UpdateServerList");
    }
	public ClientConnector GetClientConnector(){
		return connector.GetClientConnector();
	}
	public PlayerInfo GetLocalPlayerInfo(){
		return GetClientConnector().GetLocalPlayerInfo();
	}
}
