using Game.Base;
using Game.Base.Packets;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Timers;
using System.Web;
//using System.Web.Security;
using System.Xml;
using System.Reflection;
using UnityEngine;
using ConnectorSpace;

public class ClientConnector : BaseConnector
{
    public string m_account;
    public string m_password;
    public static byte GAME_CMD = 0x5b;
    public string InnerPwd;
    public string LastError;
    public string LastMsg;
    private long m_actionInterval;
    private int m_blood;
    private int m_gameCount;
    private int m_grade;
    private bool m_isHost;
    private short m_lastRecv;
    private short m_lastSent;
    private long m_lastSentTick;
    private DateTime m_lastSentTime;
    //private StreamWriter m_log;
    private int m_playerId;
    private Queue<PlayerExecutable> m_queue;
    private int m_recvCount;
    private int m_roomCount;
    private int m_sentCount;
    private int m_shootCount;
    private ePlayerState m_state;
    private int m_team;
    private System.Timers.Timer m_timer;
    public string NickName;
    public string Password;
    private int UserId;
    public ConnectorManager connectorManager;
    public int m_posX;
    public int m_posY;
    public int m_lifeTime;
    public eErrorCode lastErr;
    private List<PlayerInfo> playersList;
    
    private PlayerInfo localPlayerInfo;

#region UNIMPORTANT
    public ClientConnector(string account, string password, int interval, string ip, int port, ConnectorManager cManager) :
      base(ip, port, false, new byte[0x2000], new byte[0x2000])
    {
        this.m_account = account;
        this.Password = password;
        this.m_isHost = false;
        this.m_timer = new System.Timers.Timer((double) interval);
        this.m_timer.Elapsed += new ElapsedEventHandler(this.m_timer_Elapsed);
        this.m_queue = new Queue<PlayerExecutable>();
        this.m_state = ePlayerState.Init;
        this.m_lastSentTick = TickHelper.GetTickCount();
        this.m_actionInterval = 0L;
        this.m_recvCount = 0;
        this.m_sentCount = 0;
        base.Strict = true;
        base.Encryted = true;
        this.connectorManager = cManager;
        ConfigMgr.UpdateHostAddress(ip);
        this.lastErr = eErrorCode.DONE;
        //this.m_log = new StreamWriter(System.IO.File.Create(string.Format("./logs/{0}.log", account)));
    }

    public PlayerInfo GetLocalPlayerInfo(){
        return localPlayerInfo.Clone();
    }

    public void Act(PlayerExecutable action)
    {
        lock (this.m_queue)
        {
            this.m_queue.Enqueue(action);
        }
    }

    public void CreateLogin()
    {
        this.lastErr = eErrorCode.LOADING;
        UnityEngine.Debug.Log("Create Login");
        //string str = DateTime.Now.ToFileTime().ToString();
		string str = "132178654824148372";
        string requestUriString = string.Format("{0}?content={1}", ConfigMgr.CreateLoginUrl, HttpUtility.UrlEncode(this.m_account + "|" + this.Password + "|" + str + "|" + Md5(this.m_account + this.Password + str + ConfigMgr.Md5Key)));
        string str3 = "";
        Debug.Log(requestUriString);
        try {
            // WebRequest wR = WebRequest.Create(requestUriString);
            // Debug.Log("Request Header is: "+ wR.Headers.Get("Host"));
            WebResponse response = WebRequest.Create(requestUriString).GetResponse();
            // Debug.Log("Response Header is: "+ response.Headers);
            StreamReader reader = new StreamReader(response.GetResponseStream());
            str3 = reader.ReadLine();
        }catch(System.Exception e){
            Debug.Log("WebRequest.Create fail");
        }
        
        if (str3 == "0")
        {
            this.m_state = ePlayerState.CreateLogined;
            string xml = "";
            requestUriString = string.Format("{0}?username={1}", ConfigMgr.LoginSelectList, this.m_account);
            try {
                // WebRequest wR = WebRequest.Create(string.Format("{0}?username={1}", ConfigMgr.LoginSelectList, this.m_account));
                // Debug.Log("Request Header is: "+ wR.Headers.Get("Host"));
                WebResponse response = WebRequest.Create(requestUriString).GetResponse();
                Debug.Log(requestUriString);
                // Debug.Log("Response Header is: "+ response.Headers);
                StreamReader reader = new StreamReader(response.GetResponseStream());
                xml = reader.ReadToEnd();
            }catch(System.Exception e){
                Debug.Log("CreateLogin-Username fail: " + e.Message);
            }
            // using (WebResponse response2 = WebRequest.Create().GetResponse())
            // {
            //     using (StreamReader reader2 = new StreamReader(response2.GetResponseStream()))
            //     {
            //         xml = reader2.ReadToEnd();
            //     }
            // }
			UnityEngine.Debug.Log("XXX" + xml);
            // GameController.LogToScreen("XXX" + xml);
            if (xml.IndexOf("NickName") > 0)
            {
                XmlDocument document = new XmlDocument();
                document.LoadXml(xml);
                XmlNodeList elementsByTagName = document.GetElementsByTagName("Item");
                for (int i = 0; i < elementsByTagName.Count; i++)
                {
                    this.NickName = elementsByTagName[i].Attributes["NickName"].Value;
                    this.m_grade = Int32.Parse(elementsByTagName[i].Attributes["Grade"].Value);
                }
            }
			this.LoginWeb();
            if (lastErr == eErrorCode.LOADING)
                lastErr = eErrorCode.DONE;
            
            //this.Act(new PlayerExecutable(this.LoginWeb));
        }
        else
        {
            this.m_state = ePlayerState.Stopped;
            lastErr = eErrorCode.LOGIN_FAILED;
            this.LastError = "CreateLogin Failed!";
            // GameController.LogToScreen("CreateLogin Failed");
        }
        
    }
    
    void CreateSoloRoom(){
        Debug.Log("CreateSoloRoom");
        CreateRoom((byte)ConfigMgr.RoomType, (byte)ConfigMgr.TimeType);
    }
    void CreateExploreRoom(){
        Debug.Log("CreateExploreRoom");
        CreateRoom((byte)4, (byte)2);
    }

    public void CreateRoom(byte roomType, byte timeType = 2)
    {
        this.m_lastRecv = 0;
        GSPacketIn pkg = new GSPacketIn((byte)ePackageType.GAME_ROOM_CREATE);
        pkg.WriteByte(roomType);
        pkg.WriteByte(timeType);
        pkg.WriteString("Kien dep trai!");
        pkg.WriteString("");
        this.SendTCP(pkg);
    }

    public override void Disconnect()
    {
        // Debug.Log(string.Format("ClientDisconnected"));
        connectorManager.Disconnected();
        base.Disconnect();
        //base.Disconnect();
        //this.m_state = ePlayerState.Stopped;
        //this.LastError = "Bị kick khỏi game!";
    }

    public void EnterWaitingRoom(int typeScene)
    {
        this.m_state = ePlayerState.WaitingRoom;
        GSPacketIn pkg = new GSPacketIn((byte)ePackageType.SCENE_LOGIN);
        pkg.WriteInt(typeScene);
        this.SendTCP(pkg);
    }

    public void ExitRoom()
    {
        GSPacketIn pkg = new GSPacketIn((byte)ePackageType.GAME_ROOM_KICK);
        //pkg.WriteInt(3);
        this.SendTCP(pkg);
    }

    public void FindTarget()
    {
        GSPacketIn pkg = new GSPacketIn((byte)GAME_CMD);
        pkg.WriteByte((byte)eTankCmdType.BOT_COMMAND);
        this.SendTCP(pkg);
    }

    public string GrenateInnerPassword()
    {
        char[] chArray = new char[] { 
            'a', 'b', 'c', 'd', 'e', 'f', 'g', 'h', 'i', 'j', 'k', 'l', 'm', 'n', 'o', 'p', 
            'q', 'r', 's', 't', 'u', 'v', 'w', 'x', 'y', 'z'
         };
        string str = "";
        int num = 0;
        System.Random random = new System.Random();
        while (num < 6)
        {
            str = str + chArray[random.Next(0x1a)];
            num++;
        }
        return str;
    }

    public void LoginSocket()
    {
        string s = this.m_account + "," + this.InnerPwd;
        MemoryStream output = new MemoryStream();
        byte[] data = new byte[8];
        System.Random random = new System.Random();
        using (BinaryWriter writer = new BinaryWriter(output))
        {
            writer.Write((short) DateTime.UtcNow.Year);
            writer.Write((byte) DateTime.UtcNow.Month);
            writer.Write((byte) DateTime.UtcNow.Date.Day);
            writer.Write((byte) DateTime.UtcNow.Hour);
            writer.Write((byte) DateTime.UtcNow.Minute);
            writer.Write((byte) DateTime.UtcNow.Second);
            int[] numArray = new int[] { random.Next(0xff), random.Next(0xff), random.Next(0xff), random.Next(0xff), random.Next(0xff), random.Next(0xff), random.Next(0xff), random.Next(0xff) };
            for (int i = 0; i < numArray.Length; i++)
            {
                writer.Write((byte) numArray[i]);
                data[i] = (byte) numArray[i];
            }
            writer.Write(Encoding.UTF8.GetBytes(s));
        }
        byte[] src = Rsa(output.ToArray());
        GSPacketIn pkg = new GSPacketIn(1);
        pkg.WriteInt(ConfigMgr.Version);
        pkg.WriteInt(1);
        pkg.Write(src);
        this.SendTCP(pkg);
        this.setKey(data);
        UnityEngine.Debug.Log("{0} sending login socket....");
        // this.lastErr = eErrorCode.DONE;
    }

    public string LoadTemplate(){
        string requestUriString = ConfigMgr.LoadTemplateUrl;
        string str3 = "";
        Debug.Log("LOADING TEMPLATE: " + requestUriString);

        string decStr;
        var ms = new MemoryStream();
        try {
            WebRequest wR = WebRequest.Create(requestUriString);
            Debug.Log("Request Header is: "+ wR.Headers.Get("Host"));
            WebResponse response = wR.GetResponse();
            Debug.Log("Response Header is: "+ response.Headers);
            response.GetResponseStream().CopyTo(ms);
        }catch(System.Exception e){
            Debug.Log("Cannot copy GetResponseStream to MemoryStream");
        }
        byte[] dat = Marshal.Uncompress(ms.ToArray());
        Debug.Log("Template loaded and decompressed: rawLength - compLength: " + dat.Length + " - " + ms.Length);
        decStr = Encoding.UTF8.GetString(dat);
        return decStr;
        // using (WebResponse response = WebRequest.Create(requestUriString).GetResponse())
        // {
        //     string decStr;
        //     using (var ms = new MemoryStream()){
        //         response.GetResponseStream().CopyTo(ms);
        //         byte[] dat = Marshal.Uncompress(ms.ToArray());
        //         Debug.Log("Template loaded and decompressed: rawLength - compLength: " + dat.Length + " - " + ms.Length);
        //         decStr = Encoding.UTF8.GetString(dat);
        //     }
        //     return decStr;
        //     // saveTo = tempList.ToArray();
        // }
    }

    public void LoginWeb()
    {
        this.InnerPwd = this.GrenateInnerPassword();
        string s ;
        if (string.IsNullOrEmpty(this.NickName)){
            s = this.m_account + "," + Md5(this.Password) + "," + this.InnerPwd + "," + this.m_account;
        } else{
            s = this.m_account + "," + Md5(this.Password) + "," + this.InnerPwd + "," + this.NickName;
        }
        MemoryStream output = new MemoryStream();
        string requestUriString = "";
        Debug.Log("LoginWeb");
        try{
            BinaryWriter writer = new BinaryWriter(output);
            writer.Write((short) DateTime.UtcNow.Year);
            writer.Write((byte) DateTime.UtcNow.Month);
            writer.Write((byte) DateTime.UtcNow.Date.Day);
            writer.Write((byte) DateTime.UtcNow.Hour);
            writer.Write((byte) DateTime.UtcNow.Minute);
            writer.Write((byte) DateTime.UtcNow.Second);
            writer.Write(Encoding.UTF8.GetBytes(s));
            // Debug.Log(Encoding.UTF8.GetString(output.ToArray(), 7, output.ToArray().Length - 7));
            byte[] inArray = Rsa(output.ToArray());
            requestUriString = string.Format("{0}?p={1}&v=0", ConfigMgr.LoginUrl, HttpUtility.UrlEncode(Convert.ToBase64String(inArray)));

        } catch (System.Exception e){
            Debug.Log("Cannot generate requestUriString");
        }
        // using (BinaryWriter writer = new BinaryWriter(output))
        // {
        //     writer.Write((short) DateTime.UtcNow.Year);
        //     writer.Write((byte) DateTime.UtcNow.Month);
        //     writer.Write((byte) DateTime.UtcNow.Date.Day);
        //     writer.Write((byte) DateTime.UtcNow.Hour);
        //     writer.Write((byte) DateTime.UtcNow.Minute);
        //     writer.Write((byte) DateTime.UtcNow.Second);
        //     writer.Write(Encoding.UTF8.GetBytes(s));
        //     // Debug.Log(Encoding.UTF8.GetString(output.ToArray(), 7, output.ToArray().Length - 7));
        //     byte[] inArray = Rsa(output.ToArray());
        //     requestUriString = string.Format("{0}?p={1}&v=0", ConfigMgr.LoginUrl, HttpUtility.UrlEncode(Convert.ToBase64String(inArray)));
		// 	Debug.Log(requestUriString);
        // }   
        string str3 = "";
        Debug.Log(requestUriString);
        try{
            WebResponse response = WebRequest.Create(requestUriString).GetResponse();
            StreamReader reader = new StreamReader(response.GetResponseStream());
            str3 = reader.ReadLine();
        } catch (System.Exception e){
            Debug.Log("Cannot getResponse");
        }
        // using (WebResponse response = WebRequest.Create(requestUriString).GetResponse())
        // {
        //     using (StreamReader reader = new StreamReader(response.GetResponseStream()))
        //     {
        //         str3 = reader.ReadLine();
        //     }
        // }
        Debug.Log("str3 value: " + str3);
        if (str3.IndexOf("true") > 0)
        {
            Debug.Log(string.Format("{0} web login success....", this.m_account));
            this.m_state = ePlayerState.WebLogined;
            if (!base.Connect())
            { 
                this.lastErr = eErrorCode.SERVER_FAILED;
                Debug.Log(string.Format("{0} connect to server failed...", this.m_account));
                this.m_state = ePlayerState.Stopped;
                this.LastError = "Kh\x00f4ng thể kết nối tới server.";
            }
            else
            {
                Debug.Log(string.Format("{0} socket connected....", this.m_account));
                if (this.m_state == ePlayerState.WebLogined)
                {
					// this.LoginSocket();
                    this.Act(new PlayerExecutable(this.LoginSocket));
                }
            }
        }
        else
        {
            this.lastErr = eErrorCode.USER_FAILED;
            this.m_state = ePlayerState.Stopped;
            this.LastError = "Lỗi dữ liệu User";
        }
    }

    private void m_timer_Elapsed(object sender, ElapsedEventArgs e)
    {
        if (this.m_state == ePlayerState.Stopped)
        {
            this.Stop();
            this.m_queue = new Queue<PlayerExecutable>();
        }
        else
        {
            m_lifeTime += 100;
            PlayerExecutable executable = null;
            lock (this.m_queue)
            {
                if (this.m_queue.Count > 0)
                {
                    executable = this.m_queue.Dequeue();
                }
            }
            if (executable != null)
            {
                try
                {
                    executable();
                }
                catch (Exception exception)
                {
                    this.m_state = ePlayerState.Stopped;
                    this.LastError = exception.Message;
                    Debug.Log(string.Format("player[{0}] execute error:{1}", this.m_account, exception.Message));
                    Debug.Log(string.Format(exception.StackTrace));
                }
            }
        }
    }

    public static string Md5(string input)
    {
        using (System.Security.Cryptography.MD5 md5 = System.Security.Cryptography.MD5.Create())
		{
			byte[] inputBytes = System.Text.Encoding.ASCII.GetBytes(input);
			byte[] hashBytes = md5.ComputeHash(inputBytes);

			// Convert the byte array to hexadecimal string
			StringBuilder sb = new StringBuilder();
			for (int i = 0; i < hashBytes.Length; i++)
			{
				sb.Append(hashBytes[i].ToString("X2"));
			}
			return sb.ToString().ToLower();
		}
    }

    protected override void OnDisconnect()
    {
        base.OnDisconnect();
        if (this.m_state != ePlayerState.Stopped)
        {
            this.m_state = ePlayerState.Stopped;
            this.LastError = "Bị kick khỏi game!";
        }
    }

#endregion

    public override void OnRecvPacket(GSPacketIn pkg)
    {
        this.m_lastRecv = pkg.Code;
        this.m_recvCount++;
        // Debug.Log("player ["+this.m_account+"]:"+DateTime.Now+" receive: "+pkg.Code+" (ePackageType."+(ePackageType)pkg.Code+")");
        /*if (this.m_log != null)
        {
            this.m_log.WriteLine(Marshal.ToHexDump(string.Format("player [{0}]:{1} recive:", this.m_account, DateTime.Now), pkg.Buffer, 0, pkg.Length));
        }*/
        Debug.Log("OnRecvPacket: " + (ePackageType)pkg.Code);
        switch (pkg.Code)
        {
            case(byte) ePlayerPackageType.LOGIN:{
            //LOGIN
                if (pkg.ReadByte() != 0)
                {
                    string str = pkg.ReadString();
                    Debug.Log(this.m_account+" login socket failed: " + str);
                    this.m_state = ePlayerState.Stopped;
                    this.LastError = "SocketError:" + str;
                    break;
                }
                this.UserId = pkg.ClientID;
                pkg.ReadInt(); //4
                this.m_playerId = pkg.Parameter1;
                localPlayerInfo  = new PlayerInfo();
                localPlayerInfo.id = this.m_playerId;
                localPlayerInfo.nickname = this.NickName;
                ClientRecvPreparer.PlayerInfo_LOGIN(ref localPlayerInfo, ref pkg);

                Debug.Log("login socket success! pid " +  this.m_playerId+" uid: "+this.UserId);
                // Debug.Log("login info: "+ localPlayerInfo.ToString());
                // Debug.Log("localPlayerInfo style: " + localPlayerInfo.style);
                UnityThread.executeInUpdate(() =>
                {
                    //Call UpdateLocalPlayerPreview in ConnectorManager
                    connectorManager.UpdateLocalPlayerPreview();
                });
                break;
            }
            case 2:{
            //KIT_USER
                Debug.Log("CMDType 2 KIT_USER");
                this.m_state = ePlayerState.Stopped;
                this.LastError = "KickReason:" + pkg.ReadString();
                Debug.Log(this.LastError);
                break;
            }
            case 3:{
            //SYS_MESS
                Debug.Log("CMDType 3 - SYS MESS");
                int val = pkg.ReadInt();
                Debug.Log("CMDType 3 val: " + val.ToString());
                this.LastMsg = pkg.ReadString();
                Debug.Log(this.LastMsg);
                break;
            }
            case (byte)ePlayerPackageType.GRID_GOODS:{
                // MainBag = 0, PropBag = 1, TaskBag = 2, 
                //TempBag = 4, CaddyBag = 5, Bank    = 11, Store   =12, Card=15
                int bagType = pkg.ReadInt();
                int slotLength = pkg.ReadInt();
                List<ItemInfo> bagBag = new List<ItemInfo>();
                Debug.Log("bagtype: " + bagType + " - " + (eBagType)bagType + " slotLEngth: " +slotLength);
                for (int ind = 0; ind < slotLength; ind++){
                    pkg.ReadInt();
                    bagBag.Add(new ItemInfo(ref pkg, true));
                    // Debug.Log(bagBag[ind].ToString());
                }
                UnityThread.executeInUpdate(() =>
                {
                    //Call StartGameHandler in ConnectorManager
                    connectorManager.GridGoodsHandler(bagType, bagBag);
                });
                break;
            }
            case (byte)ePackageType.GAME_ROOM_CREATE:{
                RoomInfo ri = new RoomInfo();
                ri.id = pkg.ReadInt();
                ri.roomType = (eRoomType) pkg.ReadByte();
                ri.hardLevel = (eHardLevel) pkg.ReadByte();
                ri.timeMode = pkg.ReadByte();
                ri.playerCount = pkg.ReadByte();
                ri.placescount = pkg.ReadByte();
                ri.isPasswd = pkg.ReadBoolean();
                ri.mapid = pkg.ReadInt();
                ri.isplaying = pkg.ReadBoolean();
                ri.roomname = pkg.ReadString();
                ri.gametype = (eGameType) pkg.ReadByte();
                ri.levelLimits = pkg.ReadInt();
                pkg.ReadBoolean();//pkg.WriteBoolean(false);
                Debug.Log(ri.ToString());
                this.m_state = ePlayerState.Room;
                this.m_isHost = true;
                this.m_roomCount++;
                this.Act(new PlayerExecutable(this.StartGame));
                break;
            }
            case (byte)ePlayerPackageType.ITEM_EQUIP:{
                {
                pkg.ReadInt(); //id
                pkg.ReadInt(); //agi
                pkg.ReadInt(); //Attack
                pkg.ReadString(); //Colors
                pkg.ReadString(); //Skin
                pkg.ReadInt(); //Defence
                pkg.ReadInt(); // GP
                pkg.ReadInt(); //Grade
                pkg.ReadInt(); //Luck
                pkg.ReadInt(); //Hide
                pkg.ReadInt(); //Repute
                pkg.ReadBoolean(); // Sex
                pkg.ReadString(); // Style
                pkg.ReadInt(); //Offer
                pkg.ReadString(); //NickName
                pkg.ReadBoolean();pkg.ReadInt();
                pkg.ReadInt(); //Win
                pkg.ReadInt(); //Total
                pkg.ReadInt(); //Escape
                pkg.ReadInt();pkg.ReadString();pkg.ReadInt();pkg.ReadInt();pkg.ReadBoolean();
                pkg.ReadInt();pkg.ReadString();pkg.ReadString();pkg.ReadInt();
                pkg.ReadInt(); //FightPower
                pkg.ReadInt();pkg.ReadInt();pkg.ReadString();pkg.ReadInt();pkg.ReadString();
                //AchievementPoint
                pkg.ReadInt();pkg.ReadString();pkg.ReadDateTime();
                }
                int iCount = pkg.ReadInt(); //itemCount
                List<ItemInfo> items = new List<ItemInfo>();
                for (int ind = 0; ind < iCount; ind++)
                {
                    items.Add(new ItemInfo(ref pkg,false));
                    Debug.Log(items[ind].ToString());
                }
                break;
            }
            case (byte)ePlayerPackageType.UPDATE_PRIVATE_INFO:{
                localPlayerInfo.money = pkg.ReadInt();
                localPlayerInfo.medal = pkg.ReadInt();
                localPlayerInfo.gold = pkg.ReadInt();
                localPlayerInfo.giftToken = pkg.ReadInt();
                break;
            }
            case (byte)ePlayerPackageType.UPDATE_PlAYER_INFO:{
                localPlayerInfo.nickname = this.m_account;
                localPlayerInfo.id = this.m_playerId;
                localPlayerInfo.grade = this.m_grade;
                localPlayerInfo.gp = pkg.ReadInt();//info.GP);
                pkg.ReadInt();//info.Offer);
                pkg.ReadInt();//info.RichesOffer);
                pkg.ReadInt();//info.RichesRob);
                localPlayerInfo.win = pkg.ReadInt();//info.Win);
                localPlayerInfo.totalMatch = pkg.ReadInt();//info.Total);
                pkg.ReadInt();//info.Escape);

                localPlayerInfo.attack = pkg.ReadInt();//info.Attack);
                localPlayerInfo.defence = pkg.ReadInt();//info.Defence);
                localPlayerInfo.agility =  pkg.ReadInt();//info.Agility);
                localPlayerInfo.luck = pkg.ReadInt();//info.Luck);
                pkg.ReadInt();//info.Hide);
                localPlayerInfo.style = pkg.ReadString();//info.Style);
                localPlayerInfo.color = pkg.ReadString();//info.Colors);
                localPlayerInfo.skin = pkg.ReadString();//info.Skin);

                pkg.ReadInt();//info.ConsortiaID);
                pkg.ReadString();//info.ConsortiaName);
                pkg.ReadInt();//info.ConsortiaLevel);
                pkg.ReadInt();//info.ConsortiaRepute);

                pkg.ReadInt();//info.Nimbus);
                pkg.ReadString();//info.PvePermission);
                pkg.ReadString();//"1");
                localPlayerInfo.fightPower = pkg.ReadInt();//info.FightPower);
                // pkg.ReadInt();//1);
                // pkg.ReadInt();//-1);
                // pkg.ReadString();//"ss");
                // pkg.ReadInt();//1);
                // pkg.ReadString();//"ss");
                // ////AchievementPoint
                // pkg.ReadInt();//0);
                // ////honor
                // pkg.ReadString();//"honor");
                // // //LastSpaDate
                // // if ();//info.ExpendDate != null)
                // //     pkg.ReadDateTime();//();//DateTime)info.ExpendDate);
                // // else { pkg.ReadDateTime();//DateTime.MinValue); }
                // //charmgp
                // pkg.ReadInt();//100);
                // //consortiaCharmGP
                // pkg.ReadInt();//100);

                // pkg.ReadDateTime();//DateTime.MinValue);
                // ////DeputyWeaponID
                // pkg.ReadInt();//10001);
                // pkg.ReadInt();//0);
                // // box gi ko biet
                // pkg.ReadInt();//info.AnswerSite);
                // // pkg.ReadInt();//0);
                Debug.Log(localPlayerInfo.ToString());
                UnityThread.executeInUpdate(() =>
                {
                    //Call Update mainplayer info in ConnectorManager
                    connectorManager.UpdateLocalPlayerPreview();
                    connectorManager.UpdateStatsDisplay();
                });
                break;
            }
            case (byte)ePlayerPackageType.GAME_CMD:{
            //GAME_CMD
				//Debug.Log("GAME_CMD");
                eTankCmdType type = (eTankCmdType) pkg.ReadByte();  
                int pId = pkg.Parameter1;        
                int LifeTime = pkg.Parameter2;
                Debug.Log("[CMD] " + ((eTankCmdType)type).ToString() +" - lifeTime: "+LifeTime.ToString());
                switch (type)
                {
                    case eTankCmdType.TAKE_CARD:
                        // Debug.Log("TAKE_CARD ");
                        // return;
                    case eTankCmdType.GAME_OVER:                        
                        // this.Act(new PlayerExecutable(this.EnterWaitingRoom));
                        // this.Act(new PlayerExecutable(this.CreateRoom));
                        // this.Act(new PlayerExecutable(this.StartGame));
                        MatchSummary ms = ClientRecvPreparer.GameOver_MatchSummary(ref pkg, this.m_playerId);
                        UnityThread.executeInUpdate(() =>
                        {
                            //Call StartGameHandler in ConnectorManager
                            connectorManager.GameOverHandler(ms);
                        });
                        return;
                    case eTankCmdType.GAME_TIME:
                        this.m_lifeTime = pkg.ReadInt();
                        Debug.Log("GAME_TIME sync: "+ this.m_lifeTime.ToString());
                        return;
                    case eTankCmdType.START_GAME:
                        // Game.Logic/PVPGame.cs
                        int size = pkg.ReadInt(); //number of players
                        // List<PlayerInfo> Players = new List<PlayerInfo>();
                        for (int i = 0; i < size; i++){
                            int tmpId = pkg.ReadInt();
                            for (int j = 0; j < size; j++){
                                if (tmpId != playersList[j].id){
                                    continue;
                                }
                                playersList[j].x = pkg.ReadInt();
                                playersList[j].y = pkg.ReadInt();
                                playersList[j].direction = pkg.ReadInt();
                                playersList[j].blood = pkg.ReadInt();
                                    pkg.ReadInt(); //2
                                    pkg.ReadInt(); //34
                                playersList[j].isMainPlayer = false;
                                //check if player is this user
                                //Debug.Log("thisPid: "+ this.m_playerId.ToString() + " - Pid: " + Players[i].id.ToString());
                                // if (playersList[j].id == this.m_playerId){
                                if (playersList[j].nickname == this.m_account 
                                    || playersList[j].nickname == this.NickName){
                                    playersList[j].isMainPlayer = true;
                                    this.m_playerId = playersList[j].id;
                                    this.m_posX = playersList[j].x;
                                    this.m_posY = playersList[j].y;
                                }
                                playersList[j].dander = pkg.ReadInt();
                                playersList[j].effectCount = pkg.ReadInt();
                                playersList[j].property3 = new List<int>();
                                playersList[j].property4 = new List<int>();
                                for (int k = 0; k < playersList[j].effectCount; k++)
                                {
                                    playersList[j].property3.Add(pkg.ReadInt());
                                    playersList[j].property4.Add(pkg.ReadInt());
                                }
                                break;
                            }
                        }
                        UnityThread.executeInUpdate(() =>
                        {
                            //Call StartGameHandler in ConnectorManager
                            connectorManager.StartGameHandler(playersList);
                        });
                        this.m_state = ePlayerState.StartGame;
                        return;
                    case eTankCmdType.GAME_CREATE:
						this.m_gameCount++;
                        pkg.ReadInt(); //Roomtype
                        pkg.ReadInt(); //gametype
                        pkg.ReadInt(); //Timetype
                        localPlayerInfo.nickname = this.m_account;
                        this.playersList = ClientRecvPreparer.GameCreate_PlayerList(ref localPlayerInfo, ref pkg);
                        this.m_playerId = localPlayerInfo.id;
                        this.m_state = ePlayerState.CreateGame;
                        UnityThread.executeInUpdate(() =>
                        {
                            //Call GameCreateHandler in ConnectorManager
                            connectorManager.GameCreateHandler(this.playersList);
                        });
                        return;
                    case eTankCmdType.GAME_LOAD:
                        
                        pkg.ReadInt(); //maxTime
                        int mapId = pkg.ReadInt();
                        UnityThread.executeInUpdate(() =>
                        {
                            //Call LoadMapHandler in ConnectorManager
                            connectorManager.GameLoadHandler(mapId);
                        });
                        return;
                    case eTankCmdType.LOAD:
                        
                        this.m_state = ePlayerState.Loading;
                        // this.Act(new PlayerExecutable(this.SendLoadingComplete));
                        return;
                    case eTankCmdType.GAME_MISSION_INFO:
                        MissionInfo mi = ClientRecvPreparer.GetPVEMissionInfo(ref pkg);
                        UnityThread.executeInUpdate(() =>
                        {
                            connectorManager.PVEMissionPrepare(mi);
                        });
                        return;
                    case eTankCmdType.GAME_UI_DATA:
                        // update turn index and kill count;
                        int TurnIndex = pkg.ReadInt();
                        int count = pkg.ReadInt();
                        int param3 = pkg.ReadInt(), param4 = pkg.ReadInt();
                        UnityThread.executeInUpdate(() =>
                        {
                            Debug.Log(string.Format("GAME_UI_DATA : TurnIndex:{0} - count:{1} - param3,4:{2},{3}",
                                                                    TurnIndex,count,param3,param4));
                        });
                        return;
                    case eTankCmdType.ADD_LIVING:
                        // update turn index and kill count;
                        LivingInfo li = ClientRecvPreparer.AddLivingInfo(ref pkg);
                        UnityThread.executeInUpdate(() =>
                        {
                            Debug.Log("ADD_LIVING : " + li.ToString());
                        });
                        return;
                    case eTankCmdType.TURN:
                        Debug.Log("pId turn: "+ pkg.Parameter1.ToString());
                        int wind = pkg.ReadInt();
                        pkg.ReadBoolean();
                        pkg.ReadByte();
                        pkg.ReadByte();
                        pkg.ReadByte();
                        bool isHiding = pkg.ReadBoolean();

                        //turnTime
                        // TimeType
                        int timeType = pkg.ReadInt();
                        List<BoxInfo> newBoxesList = new List<BoxInfo>();
                        int newBoxesCount = pkg.ReadInt();
                        for(int j = 0; j < newBoxesCount; j++)
                        {
                            newBoxesList.Add(new BoxInfo());
                            newBoxesList[j].id = pkg.ReadInt();
                            newBoxesList[j].x = pkg.ReadInt();
                            newBoxesList[j].y = pkg.ReadInt();
                            pkg.ReadInt();
                            // pkg.ReadBoolean();
                        }
                        List<PlayerInfo> updatedPlayerList = new List<PlayerInfo>();
                        int listSize = pkg.ReadInt();
                        for (int j = 0; j < listSize; j++)
                        {
                            updatedPlayerList.Add(new PlayerInfo());
                            updatedPlayerList[j].id = pkg.ReadInt();
                            updatedPlayerList[j].isLiving = pkg.ReadBoolean();
                            updatedPlayerList[j].x = pkg.ReadInt();
                            updatedPlayerList[j].y = pkg.ReadInt();
                            updatedPlayerList[j].blood = pkg.ReadInt();
                            updatedPlayerList[j].isNoHole = pkg.ReadBoolean();
                            updatedPlayerList[j].energy = pkg.ReadInt();
                            updatedPlayerList[j].dander = pkg.ReadInt();
                            updatedPlayerList[j].shootCount = pkg.ReadInt();
                        }
                        int turnIndex = pkg.ReadInt();

                        UnityThread.executeInUpdate(() =>
                        {
                            //Call TurnHandler in ConnectorManager
                            connectorManager.TurnHandler(pkg.Parameter1, newBoxesList, updatedPlayerList);
                        });
                        // this.m_state = ePlayerState.Shoot;
                        // this.m_shootCount++;
                        //this.Act(new PlayerExecutable(this.Shoot));
                        return;
                    case eTankCmdType.CURRENTBALL:  
                        bool special = pkg.ReadBoolean();
                        int currentBallId = pkg.ReadInt();  
                        UnityThread.executeInUpdate(() =>
                        {
                            //Call CurrentBallHandler in ConnectorManager
                            connectorManager.CurrentBallHandler(pId,special,currentBallId);
                        });
                        //pkg.WriteByte((byte)player.BallCount);
                        return;
                    case eTankCmdType.FIRE:
                        List<FireInfo> fireInfos = ClientRecvPreparer.MakeFireInfo(ref pkg);
                        UnityThread.executeInUpdate(() =>
                        {
                            //Call FireHandler in ConnectorManager
                            connectorManager.FireHandler(pId,fireInfos);    
                        });        
                        return;
                    case eTankCmdType.FIRE_TAG:
                        bool tag = pkg.ReadBoolean();
                        byte speedTime = pkg.ReadByte();
                        UnityThread.executeInUpdate(() =>
                        {
                            //Call FireTagHandler in ConnectorManager
                        connectorManager.FireTagHandler(pId,tag,speedTime);  
                        });                
                        return;
                    case eTankCmdType.USING_PROP:    
                        byte propType = pkg.ReadByte();
                        int place = pkg.ReadInt();
                        int templateId = pkg.ReadInt();      
                        UnityThread.executeInUpdate(() =>
                        {
                            //Call UsingPropHandler in ConnectorManager
                        connectorManager.UsingPropHandler(pId,propType,place,templateId);  
                        });                
                        return;
                    case eTankCmdType.MOVESTART:
                        byte moveType = pkg.ReadByte();
                        int tx = pkg.ReadInt();
                        int ty = pkg.ReadInt();
                        byte dir = pkg.ReadByte();
                        bool isLiving = pkg.ReadBoolean();
                        string action = pkg.ReadString(); // is null or empty or "move"
                        // Debug.Log("MOVE type: "+moveType.ToString()+" tX: " + tx.ToString()+ 
                        //         " tY:"+ ty.ToString()+ " dir: "+dir.ToString());        
                        UnityThread.executeInUpdate(() =>
                        {
                            //Call MoveStartHandler in ConnectorManager
                            connectorManager.MoveStartHandler(pId,moveType,tx,ty,dir,isLiving); 
                        });
                        
                        return;
                    case eTankCmdType.DIRECTION:
                        int direction = pkg.ReadInt();
                        UnityThread.executeInUpdate(() =>
                        {
                            //Call CurrentBallHandler in ConnectorManager
                            connectorManager.DirectionHandler(pId,direction);
                        });
                        return;
                    case eTankCmdType.DANDER:
                        int playerId = pkg.Parameter1;
                        int dander = pkg.ReadInt();
                        localPlayerInfo.dander = dander;        
                        UnityThread.executeInUpdate(() =>
                        {
                            //Call CurrentBallHandler in ConnectorManager
                            connectorManager.DanderHandler(pId,dander);
                        });
                        return;
                        
                    default:
                        Debug.Log("Unhandled GAME_CMD type: " + type.ToString());
                        return;
                }
                break;  
            }
            // case 0x5e:{
            // //GAME_CREATE_ROOM
            //     Debug.Log("GET GAME_CREATE_ROOM PACKAGE");
            //     if (pkg.ReadInt() != 0)//if (m_isHost == false)
            //     {
            //         this.m_state = ePlayerState.Room;
            //         this.m_isHost = true;
            //         this.m_roomCount++;
            //         this.Act(new PlayerExecutable(this.StartGame));
            //     }
            //         break;
            // }
            case 5:
            {
            //SYS_DATE
                Debug.Log("GET SYS_DATE PACKAGE");
                // this.Act(new PlayerExecutable(this.EnterWaitingRoom));						
                // System.Threading.Thread.Sleep(100);
				// this.Act(new PlayerExecutable(this.CreateRoom));
				// //Debug.Log(this.m_State == ePlayerState.Room);
				// //Debug.Log(this.m_isHost);
				// //Debug.Log(this.m_roomCount);
				// System.Threading.Thread.Sleep(100);
                // this.Act(new PlayerExecutable(this.StartGame));
                break;
            }
            case (int)ePackageType.GAME_PAIRUP_START:      
                UnityThread.executeInUpdate(() =>
                {
                    connectorManager.ConfirmMatching();
                });
                break;
            default:
                Debug.Log("Unhandled Package type: " + ((ePackageType)pkg.Code).ToString());
                break;
        }
    }
	
    public static byte[] Rsa(byte[] input)
    {
        RSACryptoServiceProvider provider = new RSACryptoServiceProvider();
        provider.FromXmlString(ConfigMgr.RSAKey);
        return provider.Encrypt(input, false);
    }



    public void SendLogOut()
    {
        GSPacketIn pkg = new GSPacketIn((byte)ePackageType.GAME_PLAYER_EXIT);
        //pkg.WriteInt(7);
        this.SendTCP(pkg);
			
    }
    public void SendLoadingComplete()
    {
        GSPacketIn pkg = new GSPacketIn(GAME_CMD);
        pkg.WriteByte(0x10);
        pkg.WriteInt(100);
        this.SendTCP(pkg);
    }
    void ExecStartMatch(){
        this.Act(new PlayerExecutable(this.StartGame));
    }
    public void StartMatch(eRoomType gameType, int timeType){
        // this.Act(new PlayerExecutable(this.EnterWaitingRoom));
        System.Threading.Thread.Sleep(100);
        switch (gameType){
            case eRoomType.Match:
                EnterWaitingRoom(1);
                this.Act(new PlayerExecutable(this.CreateSoloRoom));
                break;
            default: 				
                Debug.Log("Starting exploration match");						
                EnterWaitingRoom(2);
                this.Act(new PlayerExecutable(this.CreateExploreRoom));		
                break;
        }
    }

    private void SendMessage(string message)
    {
        GSPacketIn pkg = new GSPacketIn(3);
        pkg.WriteInt(1);
        pkg.WriteString(message);
        this.SendTCP(pkg);
    }
    public void SendGameCMDDirection(int dir)
    {
        GSPacketIn pkg = new GSPacketIn(GAME_CMD);
        pkg.WriteByte((byte)eTankCmdType.DIRECTION);
        pkg.WriteInt(dir);
        this.SendTCP(pkg);
    }
    public void SendChangePlaceItem(eBagType bagType, int place, eBagType toBag, int toPlace, int count)
    {
        GSPacketIn pkg = new GSPacketIn((byte)ePackageType.CHANGE_PLACE_ITEM);
        pkg.WriteByte((byte)bagType);
        pkg.WriteInt(place);
        pkg.WriteByte((byte)toBag);
        pkg.WriteInt(toPlace);
        pkg.WriteInt(count);
        this.SendTCP(pkg);
    }

    private void SendShootTag(bool b, int time)
    {
        GSPacketIn pkg = new GSPacketIn(GAME_CMD);
        pkg.WriteByte((byte) eTankCmdType.FIRE_TAG);
        pkg.WriteBoolean(b);
        pkg.WriteByte((byte) time);
        this.SendTCP(pkg);
    }

    public void SendKillSelf()
    {
        GSPacketIn pkg = new GSPacketIn(GAME_CMD);
        pkg.WriteByte((byte) eTankCmdType.KILLSELF);
        this.SendTCP(pkg);
    }

    public void SendItemEquip (){
        GSPacketIn pkg = new GSPacketIn((byte)ePackageType.ITEM_EQUIP);
        pkg.WriteInt(localPlayerInfo.id);
        this.SendTCP(pkg);
    }

    public override void SendTCP(GSPacketIn pkg)
    {
        Debug.Log("Sending pkg code " + ((ePackageType)pkg.Code).ToString() +" para1: "+pkg.Parameter1.ToString() 
                        +"para2: " + pkg.Parameter2.ToString());
                        
        GameController.LogToScreen("Sending pkg code " + ((ePackageType)pkg.Code).ToString() +" para1: "+pkg.Parameter1.ToString() 
                        +"para2: " + pkg.Parameter2.ToString());
        base.SendTCP(pkg);
        Debug.Log("SEND "+ ((ePackageType)pkg.Code).ToString() + " SUCCEED");
        this.m_sentCount++;
        this.m_lastSent = pkg.Code;
        this.m_lastSentTime = DateTime.Now;
        this.m_actionInterval = TickHelper.GetTickCount() - this.m_lastSentTick;
        this.m_lastSentTick = TickHelper.GetTickCount();
        /*if (this.m_log != null)
        {
            this.m_log.WriteLine(Marshal.ToHexDump(string.Format("player [{0}]:{1} sent:", this.m_account, DateTime.Now), pkg.Buffer, 0, pkg.Length));
        }*/
    }

    public virtual void setKey(byte[] data)
    {
        for (int i = 0; i < 8; i++)
        {
            base.RECEIVE_KEY[i] = data[i];
            base.SEND_KEY[i] = data[i];
        }
    }

//Common command

//Send Game cmd

    // private void SendGameCMDShoot(int x, int y, int force, int angle)
    // {
    //     Debug.Log("shoot : x "+x+" y: "+y +" force: "+force+" angle: "+angle);
    //     Debug.Log("Send SHOOT successfully");
    // }

    // public void SendGameCMDMove(int x, int y, byte dir){
    // }


    // public void SendGameCMDSkipNext(){
    // }

    public void Skip()
    {
        GSPacketIn pkg = new GSPacketIn(GAME_CMD);
        pkg.Parameter1 = this.m_playerId;
        pkg.WriteByte((byte)eTankCmdType.SKIPNEXT);
        pkg.WriteByte((byte)eTankCmdType.SKIPNEXT);
        this.SendTCP(pkg);
        Debug.Log("Send SKIP successfully");
    }

    public void Fly()
    {
        GSPacketIn pkg = new GSPacketIn(GAME_CMD);
        pkg.Parameter1 = this.m_playerId;
        pkg.WriteByte((byte)eTankCmdType.FLY);
        this.SendTCP(pkg);
        Debug.Log("Send FLY successfully");
    }

    public void Move(int x, int y,byte dir)
    {
        // Debug.Log("Main Player Moving "+x+" - "+y+" d: "+dir);
        GSPacketIn pkg = new GSPacketIn(GAME_CMD);
        pkg.Parameter1 = this.m_playerId;
        pkg.WriteByte((byte)eTankCmdType.MOVESTART);
        pkg.WriteByte(0);
        pkg.WriteInt(x);
        pkg.WriteInt(y);
        pkg.WriteByte(dir);
        pkg.WriteBoolean(localPlayerInfo.blood > 0);
        this.SendTCP(pkg);
        Debug.Log("Send MOVE successfully");
    }

    public void ShootTag(bool tag, byte time)
    {
        this.SendShootTag(tag, time);
        //this.FindTarget();
    }

    public void Shoot(int x, int y, int force, int angle)
    {
        // this.SendGameCMDShoot(x, y, force, angle);
        GSPacketIn pkg = new GSPacketIn((short)GAME_CMD);
        pkg.Parameter1 = this.m_playerId;
        //pkg.Parameter2 = -1;
        Debug.Log("pkg.Parameter1: "+pkg.Parameter1.ToString() + " lifeTime: "+m_lifeTime.ToString());
        pkg.WriteByte((byte)eTankCmdType.FIRE);
        pkg.WriteInt(x);
        pkg.WriteInt(y);
        pkg.WriteInt(force);
        pkg.WriteInt(angle);
        Debug.LogFormat("Send SHOOT successfully - force:{0} - angle:{1} - x:{2} - y:{3}",force,angle,x,y);
        this.SendTCP(pkg);
        //this.FindTarget();
    }
    public void SendStunt()
    {
        // this.SendGameCMDShoot(x, y, force, angle);
        GSPacketIn pkg = new GSPacketIn((short)GAME_CMD);
        pkg.Parameter1 = this.m_playerId;
        //pkg.Parameter2 = -1;
        // Debug.Log("pkg.Parameter1: "+pkg.Parameter1.ToString() + " lifeTime: "+m_lifeTime.ToString());
        pkg.WriteByte((byte)eTankCmdType.STUNT);
        Debug.LogFormat("Send Stunt");
        this.SendTCP(pkg);
        //this.FindTarget();
    }

    public void SendSecondWeapon()
    {
        // this.SendGameCMDShoot(x, y, force, angle);
        GSPacketIn pkg = new GSPacketIn((short)GAME_CMD);
        pkg.Parameter1 = this.m_playerId;
        //pkg.Parameter2 = -1;
        // Debug.Log("pkg.Parameter1: "+pkg.Parameter1.ToString() + " lifeTime: "+m_lifeTime.ToString());
        pkg.WriteByte((byte)eTankCmdType.SECONDWEAPON);
        Debug.LogFormat("Send SecondWeapon");
        this.SendTCP(pkg);
        //this.FindTarget();
    }


    public void UsingProp(byte type, int place, int templateId)
    {
        GSPacketIn pkg = new GSPacketIn((short)GAME_CMD);
        pkg.Parameter1 = this.m_playerId;
        //pkg.Parameter2 = -1;
        Debug.Log("pkg.Parameter1: "+pkg.Parameter1.ToString() + " lifeTime: "+m_lifeTime.ToString());
        pkg.WriteByte((byte)eTankCmdType.USING_PROP);
        pkg.WriteByte(type);
        pkg.WriteInt(place);
        pkg.WriteInt(templateId);
        this.SendTCP(pkg);
        Debug.Log("Send USING_PROP successfully");
    }
    public void Start()
    {
        this.lastErr = eErrorCode.LOADING;
        UnityEngine.Debug.Log("starting....");
        this.m_timer.Start();
        this.Act(new PlayerExecutable(this.CreateLogin));
        GameController.LogToScreen("Client create login");
    }

    public void StartGame()
    {
        GSPacketIn pkg = new GSPacketIn((byte)ePackageType.GAME_START);
        //pkg.WriteInt(7);
        this.SendTCP(pkg);
			
    }

    public void Stop()
    {
        this.m_state = ePlayerState.Stopped;
        this.Disconnect();
        this.m_timer.Stop();
        /*if (this.m_log != null)
        {
            this.m_log.Dispose();
        }*/
    }

    public long ActionInterval
    {
        get
        {
            return this.m_actionInterval;
        }
    }

    public int GameCount
    {
        get
        {
            return this.m_gameCount;
        }
    }

    public short LastRecv
    {
        get
        {
            return this.m_lastRecv;
        }
    }

    public short LastSent
    {
        get
        {
            return this.m_lastSent;
        }
    }

    public DateTime LastSentTime
    {
        get
        {
            return this.m_lastSentTime;
        }
    }

    public int RecvCount
    {
        get
        {
            return this.m_recvCount;
        }
    }

    public int RoomCount
    {
        get
        {
            return this.m_roomCount;
        }
    }

    public int SentCount
    {
        get
        {
            return this.m_sentCount;
        }
    }

    public ePlayerState State
    {
        get
        {
            return this.m_state;
        }
    }
}
