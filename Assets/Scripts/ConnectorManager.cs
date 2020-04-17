using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;
using ConnectorSpace;
using Game.Logic.Phy.Actions;

public class ConnectorManager : MonoBehaviour
{
    public bool isSceneTransforming = false;
    public bool isLogedIn = false;
	public GameController gameController;
    public static List<ItemTemplateInfo> templateInfos;
    public List<PlayerInfo> playerInfos;
    public List<ItemInfo>[] localBags = new List<ItemInfo>[16];

    public PlayerInfo localPlayerInfo;
	public ClientConnector connector;
    UnityThread uThread;

    public static ItemTemplateInfo FindItemTemplateInfo(string pic, bool isMale){
        // Debug.Log("FindItemTemplateInfo: " + pic);
        int needSex = (isMale)?(1):(2);
        foreach(ItemTemplateInfo iti in templateInfos){
            if (iti.Pic == pic)
            {
            // Debug.Log(iti.Pic);
                if (iti.NeedSex == needSex || iti.NeedSex == 0){
                    return iti;
                }
                else
                {
                    continue;
                }
            }
        }
        return null;
    }   
    // use binary search to find the item having templateId
    public static ItemTemplateInfo FindItemTemplateInfo(int templateId, bool isMale){
        // Debug.Log("FindItemTemplateInfo: " + pic);
        int needSex = (isMale)?(1):(2);
        int start = 0, end = templateInfos.Count -1, index = 0;
        while (start != end){
            index = (start + end) /2;
            if (templateInfos[index].TemplateID == templateId){
                if (needSex == templateInfos[index].NeedSex || templateInfos[index].NeedSex==0)
                    return templateInfos[index];
            }
            if (templateInfos[index].TemplateID > templateId){
                if (index == end)
                    end = start;
                end = index;
            } else {
                if (index == start)
                    start = end;
                start = index;
            }
        }
        if (templateInfos[start].TemplateID == templateId 
            && needSex == templateInfos[index].NeedSex)
                    return templateInfos[index];
        return null;
    }   


#region ConnectorHandler
    //eTankCmdType.FIRE
    public void FireHandler(int pId, List<FireInfo> fireInfos){
        Debug.Log("Shoot of pId: "+pId.ToString());
        foreach (FireInfo fire in fireInfos){
            Debug.Log(fire.ToString());
            Debug.Log(fire.DetailString());
            for (int i = 0; i < fire.actionType.Count; i++){
                ActionType at = (ActionType)fire.actionType[i];
                switch (at){
                    case ActionType.PICK:
                    //ActionType.PICK, phy.Id, 0, 0, 0));
                        break;
                    case ActionType.BOMB:
                    gameController.BombExplodeAt(fire.timeInt[i], pId,fire.actionParam1[i],fire.actionParam2[i]);
                    // gameController.PlayerFire(pId,fire.timeInt[i],fire.vx,fire.vy,fire.actionParam1[i],fire.actionParam2[i]);     
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
                    // gameController.PlayerDander(pId,fire.actionParam1[i]);
                        break; 
                    case ActionType.FLY_OUT:
                    // bullet out of map
                    //ActionType.FLY_OUT, 0, 0, 0, 0));
                    // gameController.PlayerFire(pId,fire.timeInt[i],fire.vx,fire.vy,fire.actionParam1[i],fire.actionParam2[i]); 
                    break;  
                    case ActionType.START_MOVE:
                    // gameController.PlayerFire(pId,fire.timeInt[i],fire.vx,fire.vy,fire.actionParam1[i],fire.actionParam2[i]); 
                    //ActionType.START_MOVE, m_owner.Id, m_owner.X, m_owner.Y, m_owner.IsLiving ? 1 : 0));\
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
                if (at == ActionType.BOMB || at == ActionType.TRANSLATE || at == ActionType.FLY_OUT){
                    gameController.PlayerFire(pId,fire.timeInt[i],fire.vx,fire.vy,fire.actionParam1[i],fire.actionParam2[i]);  
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
    //eTankCmdType.CURRENTBALL
    public void CurrentBallHandler(int pId, bool special,int ballId){
        Debug.Log("[Connector - CURRENTBALL] pid:"+ pId + " special:"+ special + " ballId:"+ ballId);
        gameController.PlayerUpdateBall(pId, special, ballId);
    }

    //eTankCmdType.USING_PROP
    public void UsingPropHandler(int pId, byte propType,int place,int templateId){
        Debug.Log("[Connector - PROP] id: "+ pId + " type: "+ propType + " place: "+ place + " templateId: "+ templateId);
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

    //eTankCmdType.DANDER: Player dander (pow index)
    public void DanderHandler(int pId,int dander)
    {
        gameController.PlayerDander(pId,dander);
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
            // Debug.Log("backgroundSprite null");
        }
        gameController.LoadMap(mapId);
    }

    public void GridGoodsHandler(int bagType, List<ItemInfo> bag){
        if (localBags[bagType]== null || localBags[bagType].Count == 0){
            localBags[bagType] = bag;
            return;
        }
        for (int j = 0; j < bag.Count; j++){
            bool existed = false;
            for(int i = 0; i < localBags[bagType].Count; i++){
                if (bag[j].Place == localBags[bagType][i].Place){
                    localBags[bagType][i] = bag[j];
                    existed = true;
                    break;
                }
            }
            if (!existed){
                localBags[bagType].Add(bag[j]);
            }
        }
        GameObject go = GameObject.Find("BagAndInfoPanel");
        if (go != null){
            go.GetComponent<BagAndInfoController>().OnEnable();
        }
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
        // foreach(PlayerInfo pI in pList){
        //     Debug.Log(pI.ToString());
        // }
        gameController.GameCreate(pList);
    }
    public void GameOverHandler(MatchSummary ms){
        gameController.GameOver(ms);
    }
    public void ConfirmMatching(){
        gameController.ConfirmMatching();
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
    public void SendShoot(int x, int y, int force, int angle, byte shootTime){
        this.connector.ShootTag(true,shootTime);
        this.connector.Shoot(x, y, force, angle);
    }
    public void SendStunt(){
        this.connector.SendStunt();
    }
    public void SendSecondWeapon(){
        this.connector.SendSecondWeapon();
    }
    public void SendDirection(int dir){
        this.connector.SendGameCMDDirection(dir);
    }

    public void SendSkip(){
        this.connector.Skip();
    }
    public void SendUsingFly(){
        this.connector.Fly();
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
    private void OnDestroy() {
        connector.SendLogOut();
    }
    public void SendChangePlaceItem(eBagType bagType, int place, eBagType toBag, int toPlace, int count){
        connector.SendChangePlaceItem(bagType, place, toBag, toPlace, count);
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
        connector = new ClientConnector(id, password, 100, ConfigMgr.ServerIp, 9200, this);
        // GameController.LogToScreen("Connector Created");
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
        ((CommonUIController)gameController.uiController).notiPanel.ShowText("Đang xử lý ... ");
        string decStr = connector.LoadTemplate();
        XmlDocument document = new XmlDocument();
        document.LoadXml(decStr);
        XmlNodeList itemNodes = document.GetElementsByTagName("Item");
        StartCoroutine(ExecLoadTemplate(itemNodes));
        StartCoroutine(UpdateLoadTemplateProcess());
        isLogedIn = true;
        return true;
    }
    IEnumerator UpdateLoadTemplateProcess(){
        while(templateInfos.Count < 3300){
            yield return new WaitForSeconds(0.2f);
             ((CommonUIController)gameController.uiController).notiPanel.ShowText(templateInfos.Count.ToString()+"/3300");
        }
        ((CommonUIController)gameController.uiController).notiPanel.Close();
    }
    IEnumerator ExecLoadTemplate(XmlNodeList itemNodes){
        templateInfos = new List<ItemTemplateInfo>();
        string path = Application.persistentDataPath + "itemTemplate.dat";
        // File.Delete(path);
        if (System.IO.File.Exists(path)){
            using(Stream fileStream = File.OpenRead(path))
            {
                BinaryFormatter deserializer = new BinaryFormatter();
                templateInfos = (List<ItemTemplateInfo>)deserializer.Deserialize(fileStream);
            }
        }
        else{
            foreach (XmlNode childrenNode in itemNodes)
            {
                templateInfos.Add(new ItemTemplateInfo(childrenNode.Attributes));
                yield return null;
            } 
            using(Stream fileStream = File.Open(path, FileMode.Create))
            {
                BinaryFormatter serializer = new BinaryFormatter();
                serializer.Serialize(fileStream, templateInfos);
            }
        }
        Debug.Log("item template length: " + templateInfos.Count);
    }    
    public void ChangeHostIp(string txt){
        ConfigMgr.UpdateHostAddress(txt);
    }
    public void OpenRegister(){
        Application.OpenURL(ConfigMgr.RegisterUrl);
    }
    public ClientConnector GetClientConnector(){
        return connector;
    }

    IEnumerator InteruptStallConnector(){
        int count = 0;
        yield return new WaitForSeconds(0.3f);
        while (connector.lastErr == eErrorCode.LOADING)
        {
            yield return new WaitForSeconds(0.5f);
            // Debug.Log("0.5 s have passed");
            count++;
            if (connector.lastErr == eErrorCode.LOADING && count == 10){
                connector.lastErr = eErrorCode.FAILED;
            }
        }
    }
    public void UpdateLocalPlayerPreview(){
        // Debug.Log("Check at ConnectorManager");
        localPlayerInfo = connector.GetLocalPlayerInfo();
        gameController.uiController.UpdateMainPlayerPreview(localPlayerInfo);
    }

    public void Disconnected(){
        gameController.uiController.notiPanel.ShowText("Mất kết nối");
    }

    public void UpdateStatsDisplay(){
        GameObject go = GameObject.Find("BagAndInfoPanel");
        if (go != null){
            go.GetComponent<BagAndInfoController>().LoadStats();
        }
    }


#endregion
}
