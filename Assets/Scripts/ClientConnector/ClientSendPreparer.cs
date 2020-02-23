// using Game.Base;
// using Game.Base.Packets;
// using System;
// using System.Collections;
// using System.Collections.Generic;
// using System.IO;
// using System.Net;
// using System.Security.Cryptography;
// using System.Text;
// using System.Timers;
// using System.Web;
// //using System.Web.Security;
// using System.Xml;
// using System.Reflection;
// using UnityEngine;
// using ConnectorSpace;

// public static class ClientSendPreparer
// {


//     public void CreateRoom()
//     {
//         this.m_lastRecv = 0;
//         GSPacketIn pkg = new GSPacketIn((byte)ePackageType.GAME_ROOM_CREATE);
//         pkg.WriteByte((byte)ConfigMgr.RoomType);
//         pkg.WriteByte((byte)ConfigMgr.TimeType);
//         pkg.WriteString("Kien dep trai!");
//         //pkg.WriteString("123456");
//         this.SendTCP(pkg);
//     }

//     public override void Disconnect()
//     {
//         Console.WriteLine("hello world");
//         //base.Disconnect();
//         //this.m_state = ePlayerState.Stopped;
//         //this.LastError = "Bị kick khỏi game!";
//     }

//     public void EnterWaitingRoom()
//     {
//         this.m_state = ePlayerState.WaitingRoom;
//         GSPacketIn pkg = new GSPacketIn((byte)ePackageType.SCENE_LOGIN);
//         this.SendTCP(pkg);
//     }

//     public void ExitRoom()
//     {
//         GSPacketIn pkg = new GSPacketIn((byte)ePackageType.GAME_ROOM_KICK);
//         //pkg.WriteInt(3);
//         this.SendTCP(pkg);
//     }

//     public void FindTarget()
//     {
//         GSPacketIn pkg = new GSPacketIn((byte)GAME_CMD);
//         pkg.WriteByte((byte)eTankCmdType.BOT_COMMAND);
//         this.SendTCP(pkg);
//     }

//     protected override void OnDisconnect()
//     {
//         base.OnDisconnect();
//         if (this.m_state != ePlayerState.Stopped)
//         {
//             this.m_state = ePlayerState.Stopped;
//             this.LastError = "Bị kick khỏi game!";
//         }
//     }
//     public void SendLoadingComplete()
//     {
//         GSPacketIn pkg = new GSPacketIn(GAME_CMD);
//         pkg.WriteByte(0x10);
//         pkg.WriteInt(100);
//         this.SendTCP(pkg);
//     }
//     public void SendMessage(string message)
//     {
//         GSPacketIn pkg = new GSPacketIn(3);
//         pkg.WriteInt(1);
//         pkg.WriteString(message);
//         this.SendTCP(pkg);
//     }
//     public void SendGameCMDDirection(int dir)
//     {
//         GSPacketIn pkg = new GSPacketIn(GAME_CMD);
//         pkg.WriteByte((byte)eTankCmdType.DIRECTION);
//         pkg.WriteInt(dir);
//         this.SendTCP(pkg);
//     }

//     public void SendShootTag(bool b, int time)
//     {
//         GSPacketIn pkg = new GSPacketIn(GAME_CMD);
//         pkg.WriteByte((byte) eTankCmdType.FIRE_TAG);
//         pkg.WriteBoolean(b);
//         pkg.WriteByte((byte) time);
//         this.SendTCP(pkg);
//     }

//     public void SendItemEquip (){
//         GSPacketIn pkg = new GSPacketIn((byte)ePlayerPackageType.ITEM_EQUIP);
//         pkg.WriteInt(localPlayerInfo.id);
//         this.SendTCP(pkg);
//     }
    
// //Send Game cmd
//     public void Skip()
//     {
//         GSPacketIn pkg = new GSPacketIn(GAME_CMD);
//         pkg.Parameter1 = this.m_playerId;
//         pkg.WriteByte((byte)eTankCmdType.SKIPNEXT);
//         pkg.WriteByte((byte)eTankCmdType.SKIPNEXT);
//         this.SendTCP(pkg);
//         Debug.Log("Send SKIP successfully");
//     }

//     public void Move(int x, int y,byte dir)
//     {
//         // Debug.Log("Main Player Moving "+x+" - "+y+" d: "+dir);
//         GSPacketIn pkg = new GSPacketIn(GAME_CMD);
//         pkg.Parameter1 = this.m_playerId;
//         pkg.WriteByte((byte)eTankCmdType.MOVESTART);
//         pkg.WriteByte(0);
//         pkg.WriteInt(x);
//         pkg.WriteInt(y);
//         pkg.WriteByte(dir);
//         pkg.WriteBoolean(m_blood > 0);
//         this.SendTCP(pkg);
//         Debug.Log("Send MOVE successfully");
//     }

//     public void ShootTag(bool tag, byte time)
//     {
//         this.SendShootTag(tag, time);
//         //this.FindTarget();
//     }

//     public void Shoot(int x, int y, int force, int angle)
//     {
//         // this.SendGameCMDShoot(x, y, force, angle);
//         GSPacketIn pkg = new GSPacketIn((short)GAME_CMD);
//         pkg.Parameter1 = this.m_playerId;
//         //pkg.Parameter2 = -1;
//         Debug.Log("pkg.Parameter1: "+pkg.Parameter1.ToString() + " lifeTime: "+m_lifeTime.ToString());
//         pkg.WriteByte((byte)eTankCmdType.FIRE);
//         pkg.WriteInt(x);
//         pkg.WriteInt(y);
//         pkg.WriteInt(force);
//         pkg.WriteInt(angle);
//         this.SendTCP(pkg);
//         //this.FindTarget();
//         Debug.Log("Send SHOOT successfully");
//     }

//     public void UsingProp(byte type, int place, int templateId)
//     {
//         GSPacketIn pkg = new GSPacketIn((short)GAME_CMD);
//         pkg.Parameter1 = this.m_playerId;
//         //pkg.Parameter2 = -1;
//         Debug.Log("pkg.Parameter1: "+pkg.Parameter1.ToString() + " lifeTime: "+m_lifeTime.ToString());
//         pkg.WriteByte((byte)eTankCmdType.USING_PROP);
//         pkg.WriteByte(type);
//         pkg.WriteInt(place);
//         pkg.WriteInt(templateId);
//         this.SendTCP(pkg);
//         Debug.Log("Send USING_PROP successfully");
//     } 
//     public void StartGame()
//     {
//             GSPacketIn pkg = new GSPacketIn((byte)ePackageType.GAME_START);
//             //pkg.WriteInt(7);
//             this.SendTCP(pkg);
//     }
// }
