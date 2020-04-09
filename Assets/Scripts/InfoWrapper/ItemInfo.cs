using System;
using Game.Base;
using Game.Base.Packets;
using UnityEngine;
public class ItemInfo  
{
    //["", "head", "glass", "hair", "eff", "cloth", "face", "arm", "armlet", "ring", "", "", "", "suits", "necklace", "wing", "chatBall"];
    public eItemType itemType;
    public bool isNull;
    public byte BagType;
    public int UserID;
    public int ItemID;
    public int Count;
    public int Place;
    public int TemplateID;
    public int AttackCompose;
    public int DefendCompose;
    public int AgilityCompose;
    public int LuckCompose;
    public int StrengthenLevel;
    public bool IsBinds;
    public bool IsJudge;
    public DateTime BeginDate;
    public int ValidDate;
    public string Color;
    public string Skin;
    public bool IsUsed;
    public int Hole1;
    public int Hole2;
    public int Hole3;
    public int Hole4;
    public int Hole5;
    public int Hole6;
    public string Pic;
    public int RefineryLevel;

    public ItemInfo(ref GSPacketIn pkg, bool isNullable){
        if (isNullable){
            if (pkg.ReadBoolean() == false){
                isNull = true;
                return;
            }
        }else{
            BagType = pkg.ReadByte(); //bagtype
        }
        UserID = pkg.ReadInt(); // uid
        ItemID = pkg.ReadInt(); // item id
        Count = pkg.ReadInt(); // count
        Place = pkg.ReadInt(); // place
        TemplateID = pkg.ReadInt(); //TemplateID
        AttackCompose = pkg.ReadInt(); //Attack
        DefendCompose = pkg.ReadInt(); //Defence
        AgilityCompose = pkg.ReadInt(); //agi
        LuckCompose = pkg.ReadInt(); //Luck
        StrengthenLevel = pkg.ReadInt(); //StrengthenLevel
        IsBinds = pkg.ReadBoolean(); //IsBinds
        IsJudge = pkg.ReadBoolean(); // IsJudge
        BeginDate = pkg.ReadDateTime(); //BeginDate
        ValidDate = pkg.ReadInt(); //ValidDate
        Color = pkg.ReadString(); //Color
        Skin = pkg.ReadString();//Skin
        IsUsed = pkg.ReadBoolean(); //IsUsed
        Hole1 = pkg.ReadInt(); //Hole1
        Hole2 = pkg.ReadInt(); //Hole2
        Hole3 = pkg.ReadInt(); //Hole3
        Hole4 = pkg.ReadInt(); //Hole4
        Hole5 = pkg.ReadInt(); //Hole5
        Hole6 = pkg.ReadInt();   //Hole6
        Pic = pkg.ReadString(); //template.pic
        RefineryLevel = pkg.ReadInt(); //RefineryLevel
        pkg.ReadDateTime();
        if (isNullable)
            pkg.ReadInt();
        pkg.ReadByte();
        pkg.ReadInt();
        pkg.ReadByte();
        pkg.ReadInt();
    }
    
	public override string ToString(){
        string output = JsonUtility.ToJson(this, true);
        return output;
	}
}