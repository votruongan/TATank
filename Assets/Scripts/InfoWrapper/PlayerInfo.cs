﻿using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class PlayerInfo
{
	public bool isNoHole;
	public bool isLiving;
	public bool isMainPlayer;
	public int id;
	public string nickname;
	public bool sex; // true: male, false: female
	public int x; // pos is in pixel
	public int y; // pos is in pixel
	public int direction;
	public int blood;
    public int energy;
    public int dander;
    public int shootCount;
    public int effectCount;
	public List<int> property3;
	public List<int> property4;
	public string style; //2: hair; 4: cloth; 5: face; 6: weapon
	public string color;
	public string skin;
	public int mainWeapon;
	public int team;	// 1: blue, 2: red
	public PlayerInfo(){}

	public string ToString(){
        string output = JsonUtility.ToJson(this, true);
		string prop3 = JsonUtility.ToJson(style, true);
		string prop4 = JsonUtility.ToJson(color, true);
		output = output + "\n" + prop3 + "\n" + prop4;
        return output;
	}

    public PlayerInfo Clone()
    {
        using (var ms = new MemoryStream())
        {
            var formatter = new BinaryFormatter();
            formatter.Serialize(ms, this);
            ms.Position = 0;

            return (PlayerInfo) formatter.Deserialize(ms);
        }
    }

	
    public static int ConvertStyleId(string equipType, bool isMale, int equipId){
        // Debug.Log(equipType + " - " +equipId);
        if (equipId == 0)
            return 0;
        int offset = 0;
        if (isMale){
            if (equipType == "hair"){
                offset = 3100; //74
            }
            if (equipType == "cloth"){
                offset = 5101; //2
            }
            if (equipType == "face"){
                offset = 6101; //21
            }  
        }
        else
        {
            //1214,,3274,4204,5203,6222,70282,,,,17001,,,,,
            if (equipType == "hair"){
                offset = 3200; //74
            }
            if (equipType == "cloth"){
                offset = 5201; //2
            }
            if (equipType == "face"){
                offset = 6201; //21
            }  
        }
        return equipId - offset;
    }

	public List<int> GetStyleList(){
        string[] styles = this.style.Split(',');//2: hair; 4: cloth; 5: face; 6: weapon
        List<int> styleID = new List<int>();
        foreach(string str in styles){
            if (str == ""){
                styleID.Add(0);
                continue;
            }
            styleID.Add(Int32.Parse(str));
        }
		return styleID;
	}

}