namespace ConnectorSpace
{
    using Game.Base;
    using Game.Base.Packets;
    using log4net;
    using System;
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
	
    internal class BotPlayer : BaseConnector
    {
        public string Account;
        public static byte GAME_CMD = 0x5b;
        public string InnerPwd;
        public string LastError;
        public string LastMsg;
        public static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
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
        public int UserId;

        public BotPlayer(string account, int interval, string ip, int port) : base(ip, port, false, new byte[0x2000], new byte[0x2000])
        {
            this.Account = account;
            this.Password = "123456";
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
            //this.m_log = new StreamWriter(System.IO.File.Create(string.Format("./logs/{0}.log", account)));
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
            UnityEngine.Debug.Log("Create Login");
            //string str = DateTime.Now.ToFileTime().ToString();
			string str = "132178654824148372";
            string requestUriString = string.Format("{0}?content={1}", ConfigMgr.CreateLoginUrl, HttpUtility.UrlEncode(this.Account + "|" + this.Password + "|" + str + "|" + Md5(this.Account + this.Password + str + ConfigMgr.Md5Key)));
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
                using (WebResponse response2 = WebRequest.Create(string.Format("{0}?username={1}", ConfigMgr.LoginSelectList, this.Account)).GetResponse())
                {
                    using (StreamReader reader2 = new StreamReader(response2.GetResponseStream()))
                    {
                        xml = reader2.ReadToEnd();
                    }
                }
				UnityEngine.Debug.Log("XXX" + xml);
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
                //this.Act(new PlayerExecutable(this.LoginWeb));
            }
            else
            {
                this.m_state = ePlayerState.Stopped;
                this.LastError = "CreateLogin Failed!";
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
            string s = this.Account + "," + this.InnerPwd;
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
        }


        public void LoginWeb()
        {
            this.InnerPwd = this.GrenateInnerPassword();
            string s = this.Account + "," + Md5(this.Password) + "," + this.InnerPwd + "," + this.NickName;
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
                Console.WriteLine(Encoding.UTF8.GetString(output.ToArray(), 7, output.ToArray().Length - 7));
                byte[] inArray = Rsa(output.ToArray());
                requestUriString = string.Format("{0}?p={1}&v=0", ConfigMgr.LoginUrl, HttpUtility.UrlEncode(Convert.ToBase64String(inArray)));
				UnityEngine.Debug.Log(requestUriString);
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
                Console.WriteLine("{0} web login success....", this.Account);
                this.m_state = ePlayerState.WebLogined;
                if (!base.Connect())
                {
                    Console.WriteLine("{0} connect to server failed...", this.Account);
                    this.m_state = ePlayerState.Stopped;
                    this.LastError = "Kh\x00f4ng thể kết nối tới server.";
                }
                else
                {
                    Console.WriteLine("{0} socket connected....", this.Account);
                    if (this.m_state == ePlayerState.WebLogined)
                    {
						this.LoginSocket();
                        //this.Act(new PlayerExecutable(this.LoginSocket));
                    }
                }
            }
            else
            {
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
                        Console.WriteLine("player[{0}] execute error:{1}", this.Account, exception.Message);
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

        public override void OnRecvPacket(GSPacketIn pkg)
        {
			UnityEngine.Debug.Log("ONRecvPacket");
            this.m_lastRecv = pkg.Code;
            this.m_recvCount++;
            Console.WriteLine("player [{0}]:{1} receive: 0x{2:X2} (ePackageType.{3})", this.Account, DateTime.Now, pkg.Code, (ePackageType)pkg.Code);
            /*if (this.m_log != null)
            {
                this.m_log.WriteLine(Marshal.ToHexDump(string.Format("player [{0}]:{1} recive:", this.Account, DateTime.Now), pkg.Buffer, 0, pkg.Length));
            }*/
            switch (pkg.Code)
            {
                case 1:
                    if (pkg.ReadByte() != 0)
                    {
                        string str = pkg.ReadString();
                        Console.WriteLine("{0} login socket failed:{1}", this.Account, str);
                        this.m_state = ePlayerState.Stopped;
                        this.LastError = "SocketError:" + str;
                        break;
                    }
                    Console.WriteLine("{0} login socket success!", this.Account);
                    this.UserId = pkg.ClientID;
                    break;

                case 2:
                    this.m_state = ePlayerState.Stopped;
                    this.LastError = "KickReason:" + pkg.ReadString();
                    break;

                case 3:
                    pkg.ReadInt();
                    this.LastMsg = pkg.ReadString();
                    break;

                case 0x5b:
                {
					UnityEngine.Debug.Log("0x5b");
                    eTankCmdType type = (eTankCmdType) pkg.ReadByte();
                    switch (type)
                    {
                        case eTankCmdType.TAKE_CARD:
                        case eTankCmdType.GAME_OVER:
                            this.Act(new PlayerExecutable(this.EnterWaitingRoom));
                            this.Act(new PlayerExecutable(this.CreateRoom));
                            return;

                        case eTankCmdType.START_GAME:
                            this.m_state = ePlayerState.StartGame;
                            return;

                        case eTankCmdType.GAME_CREATE:
                        {
							UnityEngine.Debug.Log("GAME_CREATE");
                            this.m_gameCount++;
                            pkg.ReadInt(); 
                            pkg.ReadInt();
                            pkg.ReadInt();
                            int num2 = pkg.ReadInt();
							UnityEngine.Debug.Log(num2);
                            for (int i = 0; i < num2; i++)
                            {
                                pkg.ReadInt();
                                pkg.ReadString();
                                int num4 = pkg.ReadInt();
                                string nn = pkg.ReadString();
								UnityEngine.Debug.Log(nn);
                                pkg.ReadBoolean();
                                pkg.ReadByte();
                                pkg.ReadInt();
                                pkg.ReadBoolean();
                                pkg.ReadInt();
                                pkg.ReadString();
                                pkg.ReadString();
                                pkg.ReadString();
                                pkg.ReadInt();
                                pkg.ReadInt();
                                pkg.ReadInt();
                                pkg.ReadInt();
                                pkg.ReadString();
                                pkg.ReadDateTime();
                                pkg.ReadInt();
                                pkg.ReadInt();
                                pkg.ReadInt();
                                pkg.ReadBoolean();
                                pkg.ReadInt();
                                pkg.ReadString();
                                pkg.ReadInt();
                                pkg.ReadInt();
                                pkg.ReadInt();
                                pkg.ReadInt();
                                pkg.ReadInt();
                                pkg.ReadInt();
                                pkg.ReadInt();
                                pkg.ReadInt();
                                pkg.ReadString();
                                pkg.ReadInt();
                                pkg.ReadString();
                                pkg.ReadInt();
                                pkg.ReadBoolean();
                                pkg.ReadInt();
                                if (pkg.ReadBoolean())
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
                                int num5 = pkg.ReadInt();
                                int num6 = pkg.ReadInt();
                                int num7 = pkg.ReadInt();
                                if (num4 == this.UserId)
                                {
                                    this.m_team = num5;
                                    this.m_playerId = num6;
                                    this.m_blood = num7;
                                }
                                if (pkg.ReadInt() != 0)
                                {
                                    pkg.ReadInt();
                                    pkg.ReadInt();
                                    pkg.ReadInt();
                                    pkg.ReadString();
                                    pkg.ReadInt();
                                    pkg.ReadInt();
                                    int num9 = pkg.ReadInt();
                                    for (int j = 0; j < num9; j++)
                                    {
                                        pkg.ReadInt();
                                        pkg.ReadInt();
                                    }
                                }
                            }
                            this.m_state = ePlayerState.CreateGame;
                            return;
                        }
                        case eTankCmdType.GAME_LOAD:
                            pkg.ReadInt(); //maxTime
                            int mapId = pkg.ReadInt(); //mapID
                            UnityEngine.Debug.Log("mapId: " + mapId.ToString());
                            return;
                        case eTankCmdType.LOAD:
                            this.m_state = ePlayerState.Loading;
                            this.Act(new PlayerExecutable(this.SendLoadingComplete));
                            return;

                        case eTankCmdType.TURN:
                            if (pkg.Parameter1 == this.m_playerId)
                            {
                                this.m_state = ePlayerState.Shoot;
                                this.m_shootCount++;
                                this.Act(new PlayerExecutable(this.Shoot));
                            }
                            return;
                    }
                    break;
                }
                case 0x5e:
                if (pkg.ReadInt() != 0)//if (m_isHost == false)
                {
                    this.m_state = ePlayerState.Room;
                    this.m_isHost = true;
                    this.m_roomCount++;
                    //this.Act(new PlayerExecutable(this.StartGame));
                }
                    break;
                case 5:
                    this.Act(new PlayerExecutable(this.EnterWaitingRoom));						
                    System.Threading.Thread.Sleep(3000);
					this.Act(new PlayerExecutable(this.CreateRoom));
					//Debug.Log(this.m_State == ePlayerState.Room);
					//Debug.Log(this.m_isHost);
					//Debug.Log(this.m_roomCount);
					System.Threading.Thread.Sleep(3000);
                    this.Act(new PlayerExecutable(this.StartGame));
                    break;
            }
        }
		
        public static byte[] Rsa(byte[] input)
        {
            RSACryptoServiceProvider provider = new RSACryptoServiceProvider();
            provider.FromXmlString(ConfigMgr.RSAKey);
            return provider.Encrypt(input, false);
        }

        private void SendGameCMDShoot(int x, int y, int force, int angle)
        {
            GSPacketIn pkg = new GSPacketIn(GAME_CMD);
            pkg.WriteByte(2);
            pkg.WriteInt(x);
            pkg.WriteInt(y);
            pkg.WriteInt(force);
            pkg.WriteInt(angle);
            this.SendTCP(pkg);
        }

        public void SendLoadingComplete()
        {
            GSPacketIn pkg = new GSPacketIn(GAME_CMD);
            pkg.WriteByte(0x10);
            pkg.WriteInt(100);
            this.SendTCP(pkg);
        }

        private void SendMessage(string message)
        {
            GSPacketIn pkg = new GSPacketIn(3);
            pkg.WriteInt(1);
            pkg.WriteString(message);
            this.SendTCP(pkg);
        }

        private void SendShootTag(bool b, int time)
        {
            GSPacketIn pkg = new GSPacketIn(GAME_CMD);
            pkg.WriteByte(0x60);
            pkg.WriteBoolean(b);
            pkg.WriteByte((byte) time);
            this.SendTCP(pkg);
        }

        public override void SendTCP(GSPacketIn pkg)
        {
            base.SendTCP(pkg);
            this.m_sentCount++;
            this.m_lastSent = pkg.Code;
            this.m_lastSentTime = DateTime.Now;
            this.m_actionInterval = TickHelper.GetTickCount() - this.m_lastSentTick;
            this.m_lastSentTick = TickHelper.GetTickCount();
            /*if (this.m_log != null)
            {
                this.m_log.WriteLine(Marshal.ToHexDump(string.Format("player [{0}]:{1} sent:", this.Account, DateTime.Now), pkg.Buffer, 0, pkg.Length));
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

        public void Shoot()
        {
            this.FindTarget();
        }

        public void Start()
        {
            UnityEngine.Debug.Log("{0} starting....");
            this.m_timer.Start();
            this.Act(new PlayerExecutable(this.CreateLogin));
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
}

