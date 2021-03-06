﻿using Game.Base;
using Game.Base.Packets;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
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
        this.SetHostIp(ip);
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
        using (WebResponse response = WebRequest.Create(requestUriString).GetResponse())
        {
            using (StreamReader reader = new StreamReader(response.GetResponseStream()))
            {
                str3 = reader.ReadLine();
            }
        }
		
        if (str3 == "0")
        {
            this.m_state = ePlayerState.CreateLogined;
            string xml = "";
            using (WebResponse response2 = WebRequest.Create(string.Format("{0}?username={1}", ConfigMgr.LoginSelectList, this.m_account)).GetResponse())
            {
                using (StreamReader reader2 = new StreamReader(response2.GetResponseStream()))
                {
                    xml = reader2.ReadToEnd();
                }
            }
			UnityEngine.Debug.Log("XXX" + xml);
            GameController.LogToScreen("XXX" + xml);
            if (xml.IndexOf("NickName") > 0)
            {
                XmlDocument document = new XmlDocument();
                document.LoadXml(xml);
                XmlNodeList elementsByTagName = document.GetElementsByTagName("Item");
                for (int i = 0; i < elementsByTagName.Count; i++)
                {
                    this.NickName = elementsByTagName[i].Attributes["NickName"].Value;
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
            GameController.LogToScreen("CreateLogin Failed");
        }
        
    }
    


    public void CreateRoom()
    {
        this.m_lastRecv = 0;
        GSPacketIn pkg = new GSPacketIn((byte)ePackageType.GAME_ROOM_CREATE);
        pkg.WriteByte((byte)ConfigMgr.RoomType);
        pkg.WriteByte((byte)ConfigMgr.TimeType);
        pkg.WriteString("Kien dep trai!");
        //pkg.WriteString("123456");
        this.SendTCP(pkg);
    }

    public override void Disconnect()
    {
        Console.WriteLine("hello world");
        //base.Disconnect();
        //this.m_state = ePlayerState.Stopped;
        //this.LastError = "Bị kick khỏi game!";
    }

    public void EnterWaitingRoom()
    {
        this.m_state = ePlayerState.WaitingRoom;
        GSPacketIn pkg = new GSPacketIn((byte)ePackageType.SCENE_LOGIN);
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


    public void LoginWeb()
    {
        this.InnerPwd = this.GrenateInnerPassword();
        string s = this.m_account + "," + Md5(this.Password) + "," + this.InnerPwd + "," + this.NickName;
        MemoryStream output = new MemoryStream();
        string requestUriString = "";
        using (BinaryWriter writer = new BinaryWriter(output))
        {
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
			Debug.Log(requestUriString);
        }   
        string str3 = "";
        using (WebResponse response = WebRequest.Create(requestUriString).GetResponse())
        {
            using (StreamReader reader = new StreamReader(response.GetResponseStream()))
            {
                str3 = reader.ReadLine();
            }
        }
        if (str3.IndexOf("true") > 0)
        {
            Console.WriteLine("{0} web login success....", this.m_account);
            this.m_state = ePlayerState.WebLogined;
            if (!base.Connect())
            { 
                this.lastErr = eErrorCode.SERVER_FAILED;
                Console.WriteLine("{0} connect to server failed...", this.m_account);
                this.m_state = ePlayerState.Stopped;
                this.LastError = "Kh\x00f4ng thể kết nối tới server.";
            }
            else
            {
                Console.WriteLine("{0} socket connected....", this.m_account);
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
                    Console.WriteLine("player[{0}] execute error:{1}", this.m_account, exception.Message);
                    Console.WriteLine(exception.StackTrace);
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
        switch (pkg.Code)
        {
            case 1:
            {
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
                pkg.ReadInt(); //attack
                pkg.ReadInt();//def
                pkg.ReadInt();//agil
                pkg.ReadInt();//luck
                pkg.ReadInt();//gp
                pkg.ReadInt();//repute
                pkg.ReadInt();//gold
                pkg.ReadInt();//money
                pkg.ReadInt();//PropBag.GetItemCount
                pkg.ReadInt();//PlayerCharacter.Hide
                pkg.ReadInt();//PlayerCharacter.FightPower
                pkg.ReadInt();
                pkg.ReadInt();
                pkg.ReadString(); //Master
                pkg.ReadInt();
                pkg.ReadString(); //HoNorMaster
                pkg.ReadDateTime();
                pkg.ReadBoolean(); //true
                pkg.ReadInt();
                pkg.ReadInt();
                pkg.ReadDateTime();

                pkg.ReadDateTime();
                pkg.ReadInt();
                pkg.ReadDateTime();
                pkg.ReadBoolean(); // false
                pkg.ReadInt(); //1599
                pkg.ReadInt(); //1599
                pkg.ReadString(); //honor

                pkg.ReadInt(); // 0
                localPlayerInfo.sex = pkg.ReadBoolean(); //sex
                string tmpStr = pkg.ReadString(); //style & color
                localPlayerInfo.style = tmpStr.Split('&')[0];
                pkg.ReadString(); //skin

                Debug.Log("login socket success! pid " +  this.m_playerId+" uid: "+this.UserId);
                Debug.Log("localPlayerInfo style: " + localPlayerInfo.style);
                UnityThread.executeInUpdate(() =>
                {
                    //Call StartGameHandler in ConnectorManager
                    connectorManager.UpdateLocalPlayerPreview();
                });
                break;
            }
            case 2:
            {
            //KIT_USER
                Debug.Log("CMDType 2 KIT_USER");
                this.m_state = ePlayerState.Stopped;
                this.LastError = "KickReason:" + pkg.ReadString();
                Debug.Log(this.LastError);
                break;
            }
            case 3:
            {
            //SYS_MESS
                Debug.Log("CMDType 3 - SYS MESS");
                int val = pkg.ReadInt();
                Debug.Log("CMDType 3 val: " + val.ToString());
                this.LastMsg = pkg.ReadString();
                Debug.Log(this.LastMsg);
                break;
            }
            case 0x5b:
            {
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
                        UnityThread.executeInUpdate(() =>
                        {
                            //Call StartGameHandler in ConnectorManager
                            connectorManager.GameOverHandler();
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
                            // Players.Add(new PlayerInfo());
                            int tmpId = pkg.ReadInt();
                            // Players[i].id = pkg.ReadInt();
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
                                if (playersList[j].nickname == this.m_account){
                                    playersList[j].isMainPlayer = true; 
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
                        int num2 = pkg.ReadInt(); // number of players
                        this.playersList = new List<PlayerInfo>();
                        for (int i = 0; i < num2; i++)
                        {
                            this.playersList.Add(new PlayerInfo());
                            pkg.ReadInt();
                            pkg.ReadString();
                            this.playersList[i].id = pkg.ReadInt();
                            this.playersList[i].nickname = pkg.ReadString();
                            pkg.ReadBoolean(); // is vip
                            pkg.ReadInt();  // vip level
                            this.playersList[i].sex = pkg.ReadBoolean(); // sex
                            pkg.ReadInt();
                            this.playersList[i].style = pkg.ReadString();
                            this.playersList[i].color = pkg.ReadString();
                            this.playersList[i].skin = pkg.ReadString();
                            pkg.ReadInt(); // grade
                            pkg.ReadInt(); // repute
                            this.playersList[i].mainWeapon = pkg.ReadInt(); 
                            pkg.ReadInt();
                            pkg.ReadString();
                            pkg.ReadDateTime();
                            pkg.ReadInt();
                            pkg.ReadInt(); // Nimbus
                            pkg.ReadInt(); // Corsotia Id
                            pkg.ReadString(); //Corsotia name
                            pkg.ReadInt(); // Corsotia level
                            pkg.ReadInt(); // Corsotia repute
                            pkg.ReadInt(); // win
                            pkg.ReadInt(); // total
                            pkg.ReadInt(); // fightPower
                            pkg.ReadInt();
                            pkg.ReadInt();
                            pkg.ReadString();
                            pkg.ReadInt();
                            pkg.ReadString();
                            bool isMarried = pkg.ReadBoolean();
                            if (isMarried)
                            {
                                pkg.ReadInt();
                                pkg.ReadString();
                            }
                            pkg.ReadInt();
                            pkg.ReadInt();
                            pkg.ReadInt();
                            pkg.ReadInt();
                            pkg.ReadInt();
                            pkg.ReadInt();
                            
                            this.playersList[i].team = pkg.ReadInt();
                            this.playersList[i].id = pkg.ReadInt();
                            this.playersList[i].blood = pkg.ReadInt();
                            if (this.playersList[i].id == this.m_playerId)
                            {
                                this.m_team = this.playersList[i].team;
                                this.m_blood = this.playersList[i].blood;
                            }
                        }
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
                        int bombCount = pkg.ReadInt();
                        float lifeTime = 0;
                        List<FireInfo> fireInfos = new List<FireInfo>();
                        for (int i = 0; i < bombCount; i++)
                        {
                            fireInfos.Add(new FireInfo());
                            // int vx = (int)(force * reforce * Math.Cos((double)(angle + reangle) / 180 * Math.PI));
                            // int vy = (int)(force * reforce * Math.Sin((double)(angle + reangle) / 180 * Math.PI));
                            //Console.ReadLine(string.Format("<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<< vx:{0}   vy:{1}", vx, vy));
                            // m_map.AddPhysical(bomb);
                            // bomb.StartMoving();
                            pkg.ReadInt();
                            pkg.ReadInt();
                            fireInfos[i].isDigMap = pkg.ReadBoolean();
                            fireInfos[i].bomId = pkg.ReadInt();
                            fireInfos[i].x = pkg.ReadInt();
                            fireInfos[i].y = pkg.ReadInt();
                            fireInfos[i].vx = pkg.ReadInt();
                            fireInfos[i].vy = pkg.ReadInt();
                            fireInfos[i].bomInfoId = pkg.ReadInt();
                            //FlyingPartical
                            //pkg.ReadString(bomb.BallInfo.FlyingPartical);
                            pkg.ReadString();
                            pkg.ReadInt();
                            pkg.ReadInt();
                            //pkg.ReadInt(0);
                            fireInfos[i].bomActionCount = pkg.ReadInt();
                            //Debug.Log("Bom Action count: "+ fireInfos[i].bomActionCount.ToString());
                            for (int j = 0; j < fireInfos[i].bomActionCount; j++)
                            {
                                fireInfos[i].timeInt.Add(pkg.ReadInt()); //0 ->null
                                fireInfos[i].actionType.Add(pkg.ReadInt()); //0 ->null
                                fireInfos[i].actionParam1.Add(pkg.ReadInt()); //0 ->null
                                fireInfos[i].actionParam2.Add(pkg.ReadInt()); //0 ->null
                                fireInfos[i].actionParam3.Add(pkg.ReadInt()); //0 ->null
                                fireInfos[i].actionParam4.Add(pkg.ReadInt()); //0 ->null
                            }
                            //Debug.Log(fireInfos[i].ToString());
                        }         
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
                        Debug.Log("MOVE type: "+moveType.ToString()+" tX: " + tx.ToString()+ 
                                " tY:"+ ty.ToString()+ " dir: "+dir.ToString());        
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
                        UnityThread.executeInUpdate(() =>
                        {
                            //Call CurrentBallHandler in ConnectorManager
                            connectorManager.DanderHandler(pId,dander);
                        });
                        return;
                    default:
                        Debug.Log("Unhandled type: " + type.ToString());
                        return;
                }
                break;  
            }
            case 0x5e:
            {
            //GAME_CREATE_ROOM
                Debug.Log("GET GAME_CREATE_ROOM PACKAGE");
                if (pkg.ReadInt() != 0)//if (m_isHost == false)
                {
                    this.m_state = ePlayerState.Room;
                    this.m_isHost = true;
                    this.m_roomCount++;
                    //this.Act(new PlayerExecutable(this.StartGame));
                }
                    break;
            }
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
        }
    }
	
    public static byte[] Rsa(byte[] input)
    {
        RSACryptoServiceProvider provider = new RSACryptoServiceProvider();
        provider.FromXmlString(ConfigMgr.RSAKey);
        return provider.Encrypt(input, false);
    }


    public void SendLoadingComplete()
    {
        GSPacketIn pkg = new GSPacketIn(GAME_CMD);
        pkg.WriteByte(0x10);
        pkg.WriteInt(100);
        this.SendTCP(pkg);
    }

    public void StartMatch(){
        this.Act(new PlayerExecutable(this.EnterWaitingRoom));						
        System.Threading.Thread.Sleep(100);
        this.Act(new PlayerExecutable(this.CreateRoom));
        System.Threading.Thread.Sleep(100);
        this.Act(new PlayerExecutable(this.StartGame));
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

    private void SendShootTag(bool b, int time)
    {
        GSPacketIn pkg = new GSPacketIn(GAME_CMD);
        pkg.WriteByte((byte) eTankCmdType.FIRE_TAG);
        pkg.WriteBoolean(b);
        pkg.WriteByte((byte) time);
        this.SendTCP(pkg);
    }

    public void SendItemEquip (){
        GSPacketIn pkg = new GSPacketIn((byte)ePlayerPackageType.ITEM_EQUIP);
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


    private void SendGameCMDShoot(int x, int y, int force, int angle)
    {
        Debug.Log("shoot : x "+x+" y: "+y +" force: "+force+" angle: "+angle);
        Debug.Log("Send SHOOT successfully");
    }

    public void SendGameCMDMove(int x, int y, byte dir){
    }

    public void SendGameCMDSkipNext(){
    }

    public void Skip()
    {
        GSPacketIn pkg = new GSPacketIn(GAME_CMD);
        pkg.Parameter1 = this.m_playerId;
        pkg.WriteByte((byte)eTankCmdType.SKIPNEXT);
        pkg.WriteByte((byte)eTankCmdType.SKIPNEXT);
        this.SendTCP(pkg);
        Debug.Log("Send SKIP successfully");
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
        pkg.WriteBoolean(m_blood > 0);
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
        this.SendGameCMDShoot(x, y, force, angle);
        GSPacketIn pkg = new GSPacketIn((short)GAME_CMD);
        pkg.Parameter1 = this.m_playerId;
        //pkg.Parameter2 = -1;
        Debug.Log("pkg.Parameter1: "+pkg.Parameter1.ToString() + " lifeTime: "+m_lifeTime.ToString());
        pkg.WriteByte((byte)eTankCmdType.FIRE);
        pkg.WriteInt(x);
        pkg.WriteInt(y);
        pkg.WriteInt(force);
        pkg.WriteInt(angle);
        this.SendTCP(pkg);
        //this.FindTarget();
        Debug.Log("Send SHOOT successfully");
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
    public void SetHostIp(string hostIp){
        ConfigMgr.UpdateHostAddress(hostIp);
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
