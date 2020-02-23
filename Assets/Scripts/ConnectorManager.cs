using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ConnectorSpace;
using Game.Logic.Phy.Actions;

public class ConnectorManager : MonoBehaviour
{
    public bool isSceneTransforming = false;
    public bool isLogedIn = false;
	public GameController gameController;
    public List<PlayerInfo> playerInfos;
    public List<ItemInfo>[] localBags = new List<ItemInfo>[16];

    public PlayerInfo localPlayerInfo;
	ClientConnector connector;
    UnityThread uThread;

#region ConnectorHandler
    //eTankCmdType.FIRE
    public void FireHandler(int pId, List<FireInfo> fireInfos){
        Debug.Log("Shoot of pId: "+pId.ToString());
        foreach (FireInfo fire in fireInfos){
            Debug.Log(fire.ToString());
            Debug.Log(fire.DetailString());
            for (int i = 0; i < fire.actionType.Count; i++){
                gameController.PlayerFire(pId,fire.timeInt[i],fire.vx,fire.vy); 
                switch ((ActionType)fire.actionType[i]){
                    case ActionType.PICK:
                    //ActionType.PICK, phy.Id, 0, 0, 0));
                        break;
                    case ActionType.BOMB:
                    gameController.BombExplodeAt(fire.timeInt[i], pId,fire.actionParam1[i],fire.actionParam2[i]);
                    //ActionType.BOMB, m_x, m_y, digMap ? 1 : 0, 0));
                        break;
                    case ActionType.KILL_PLAYER:
                    //cause damage to enemy
                    gameController.PlayerDamage(fire.timeInt[i],fire.actionParam1[i], fire.actionParam2[i],
                                                    fire.actionParam3[i]==1?false:true, fire.actionParam4[i]);
                    //ActionType.KILL_PLAYER, p.Id, damage + critical, critical != 0 ? 2 : 1, p.Blood));
                        break;
                    case ActionType.DANDER:
                    //ActionType.DANDER, p.Id, ((Player)p).Dander, 0, 0));
                        break; 
                    case ActionType.FLY_OUT:
                    // bullet out of map -> treat as startmove
                    //ActionType.FLY_OUT, 0, 0, 0, 0));
                    case ActionType.START_MOVE:
                    //gameController.PlayerFire(pId,fire.timeInt[i],fire.vx,fire.vy); 
                    //ActionType.START_MOVE, m_owner.Id, m_owner.X, m_owner.Y, m_owner.IsLiving ? 1 : 0));
                        break;
                    case ActionType.TRANSLATE:
                    gameController.PlayerFly(fire.timeInt[i],pId,fire.actionParam1[i],fire.actionParam2[i]); 
                    //ActionType.TRANSLATE, m_x, m_y, 0, 0));
                        break;
                    case ActionType.CURE:
                    //ActionType.CURE, p.Id, p.Blood, blood, 0));
                        break;
                    case ActionType.FORZEN:
                    //ActionType.FORZEN, p.Id, 0, 0, 0));
                    //pId = -1 -> UNANGLE
                        break;
                    case ActionType.UNANGLE:
                    //ActionType.UNANGLE, p.Id, 0, 0, 0));
                        break;
                }
            }
        }
    }
    //eTankCmdType.FIRE_TAG
    public void FireTagHandler(int pId, bool tag, byte spdTime){
        Debug.Log("tag: "+tag.ToString() + " spdTime: "+spdTime.ToString());  
        if (tag){
            gameController.AddFireTag(pId,spdTime);
        } 
    }
    //eTankCmdType.USING_PROP
    public void UsingPropHandler(int pId, byte propType,int place,int templateId){
        Debug.Log("[PROP] id: "+ pId + " type: "+ propType + " place: "+ place + " templateId: "+ templateId);
        gameController.PlayerUsingProp(pId, propType, place, templateId);
    }

    //eTankCmdType.MOVESTART: Player transform
    public void MoveStartHandler(int pId, byte mvType,int x,int y,byte dir,bool isLiving){
        if (!isLiving){
            return;
        }
        switch(mvType){
            case 2:
            // ghost move
                break;
            case 0:
            case 3:
            // living move
            Debug.Log("living move, pId: "+pId.ToString());
            gameController.PlayerMove(pId,x,y,dir);
                break;
        }
    }

    //eTankCmdType.CURRENTBALL
    public void CurrentBallHandler(int pId, bool special, int curBallId){

    }
    //eTankCmdType.DANDER: Player dander (pow index)
    public void DanderHandler(int pId,int dander)
    {
        
    }

    //eTankCmdType.DIRECTION
    public void DirectionHandler(int pId, int direction)
    {
        gameController.PlayerDirection(pId,direction);
    }
    //eTankCmdType.TURN: This player turn
    public void TurnHandler(int pId,List<BoxInfo> newBoxes, List<PlayerInfo> players){
        gameController.PlayerTurn(pId,newBoxes,players);
    }
    //eTankCmdType.GAME_LOAD: load map
    IEnumerator ExecGameLoadHandler(int mapId){
        Debug.Log("LoadMapHandler : " + mapId.ToString());
        while(gameController.backgroundSprite == null){
            yield return null;
            Debug.Log("backgroundSprite null");
        }
        gameController.LoadMap(mapId);
    }

    public void GirdGoodsHandler(int bagType, List<ItemInfo> bag){
        localBags[bagType] = bag;
    }
    public void GameLoadHandler(int mapId){
        // Debug.Log("LoadMapHandler : " + mapId.ToString());
        // gameController.LoadMap(mapId);
        StartCoroutine(ExecGameLoadHandler(mapId));
    }
    //eTankCmdType.START_GAME: load players
    public void StartGameHandler(List<PlayerInfo> Players){
        gameController.StartGame(Players);
    }
    public void GameCreateHandler(List<PlayerInfo> pList){
        gameController.GameCreate(pList);
    }
    public void GameOverHandler(){
        gameController.GameOver();
    }
#endregion

#region SendCommandToConnector

    public void SendLoadComplete(){
        this.connector.SendLoadingComplete();
    }
    
    public void StartMatch(){
        this.connector.StartMatch();
    }
    public void StopMatch(){
        this.connector.ExitRoom();
    }
    public void SendItemEquip (){
        this.connector.SendItemEquip();
    }
    public void SendShoot(int x, int y, int force, int angle){
        this.connector.ShootTag(true,0);
        this.connector.Shoot(x, y, force, angle);
    }

    public void SendDirection(int dir){
        this.connector.SendGameCMDDirection(dir);
    }


    public void SendSkip(){
        this.connector.Skip();
    }

    public void SendMove(int x, int y,byte dir){
        this.connector.Move(x,y,dir);
    }
    public void SendUsingProp(byte type, int place,int templateId){
        this.connector.UsingProp(type,place,templateId);
    }

#endregion
    
#region SetUpConnector
    //init communication with other thread
    void Awake()
    {
        UnityThread.initUnityThread();
    }
    // Start is called before the first frame update
    void Start()
    {
        // no GameController assigned, try to find it
        if (gameController == null){
            gameController = GameObject.Find("GameController").GetComponent<GameController>();
        }
        
        if (gameController == null)
            return;
    }
    public bool ConnectToHost(string HostIp){
        connector = new ClientConnector("bot0", "123456", 100, HostIp, 9200, this);
        GameController.LogToScreen("Connector Created");
        Debug.Log("Connector Created");
        connector.Start();
        GameController.LogToScreen("Connector Started");
        Debug.Log("Connector Started");
        return true;
    }

    public bool ConnectToHost(string id, string password, string HostIp){
        connector = new ClientConnector(id, password, 100, HostIp, 9200, this);
        GameController.LogToScreen("Connector Created");
        Debug.Log("Connector Created");
        connector.Start();
        StartCoroutine(InteruptStallConnector());
        while (connector.lastErr == eErrorCode.LOADING)
        {
            
        }
        if (connector.lastErr != eErrorCode.DONE){
            GameController.LogToScreen("LoginFailed ");
            Debug.Log("LoginFailed " + connector.lastErr.ToString());
            return false;
        }
        PlayerPrefs.SetString("Id",id);
        PlayerPrefs.SetString("Pass",password+"123");
        PlayerPrefs.SetString("Host",HostIp);
        PlayerPrefs.SetString("28102000",password);
        GameController.LogToScreen("Connector Started");
        Debug.Log("Login Done - Connector Started");
        isLogedIn = true;
        return true;
    }

    public void OpenRegister(){
        Application.OpenURL(ConfigMgr.RegisterUrl);
    }
    public ClientConnector GetClientConnector(){
        return connector;
    }

    IEnumerator InteruptStallConnector(){
        int times = 0;
        while (connector.lastErr == eErrorCode.LOADING)
        {
            yield return new WaitForSeconds(0.5f);
            Debug.Log("0.5 s have passed");
            times++;
            if (connector.lastErr == eErrorCode.LOADING && times == 10){
                connector.lastErr = eErrorCode.FAILED;
            }
        }
    }
    public void UpdateLocalPlayerPreview(){
        // Debug.Log("Check at ConnectorManager");
        localPlayerInfo = connector.GetLocalPlayerInfo();
        gameController.uiController.UpdateMainPlayerPreview(localPlayerInfo);
    }


#endregion
}
