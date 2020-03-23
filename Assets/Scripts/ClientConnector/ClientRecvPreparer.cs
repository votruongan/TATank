using Game.Base;
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

public static class ClientRecvPreparer
{
    public static List<FireInfo> MakeFireInfo(ref GSPacketIn pkg){
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
            //pkg.ReadInt();
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
        return fireInfos;
    }


    public static MatchSummary  GameOver_MatchSummary(ref GSPacketIn pkg, int mainPID){
        MatchSummary ms = new MatchSummary();
        int pCount = pkg.ReadInt();
        int id;
        for (int i = 0; i < pCount; i++)
        {    
            id = pkg.ReadInt();
            if (id != mainPID){
                pkg.ReadBoolean();
                for (int j = 0; j < 23; j ++){
                    pkg.ReadInt();
                }
                continue;
            }
            ms.isWin = pkg.ReadBoolean();
            ms.Grade = pkg.ReadInt();
            ms.GP = pkg.ReadInt();
            ms.TotalKill = pkg.ReadInt();
            ms.TotalHurt = pkg.ReadInt();
            ms.TotalShootCount = pkg.ReadInt();
            ms.TotalCure = pkg.ReadInt();
            pkg.ReadInt();
            pkg.ReadInt();
            pkg.ReadInt();
            pkg.ReadInt();
            pkg.ReadInt();
            pkg.ReadInt();
            pkg.ReadInt();
            pkg.ReadInt();
            ms.GainGP = pkg.ReadInt();
            pkg.ReadInt();
            pkg.ReadInt();
            pkg.ReadInt();
            pkg.ReadInt();
            pkg.ReadInt();
            pkg.ReadInt();
            ms.GainOffer = pkg.ReadInt();
            ms.CanTakeOut = pkg.ReadInt();
        }
        ms.riches = pkg.ReadInt();
        return ms;
    }

    public static List<PlayerInfo> GameCreate_PlayerList(ref GSPacketIn pkg){
        int num2 = pkg.ReadInt(); // number of players
        List<PlayerInfo> playersList = new List<PlayerInfo>();
        for (int i = 0; i < num2; i++)
        {
            playersList.Add(new PlayerInfo());
            pkg.ReadInt();
            pkg.ReadString();
            playersList[i].id = pkg.ReadInt();
            playersList[i].nickname = pkg.ReadString();
            pkg.ReadBoolean(); // is vip
            pkg.ReadInt();  // vip level
            playersList[i].sex = pkg.ReadBoolean(); // sex
            pkg.ReadInt();
            playersList[i].style = pkg.ReadString();
            playersList[i].color = pkg.ReadString();
            playersList[i].skin = pkg.ReadString();
            pkg.ReadInt(); // grade
            pkg.ReadInt(); // repute
            playersList[i].mainWeapon = pkg.ReadInt(); 
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
            
            playersList[i].team = pkg.ReadInt();
            playersList[i].id = pkg.ReadInt();
            playersList[i].blood = pkg.ReadInt();
        }
        return playersList;
    }

}
