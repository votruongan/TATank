using System.Collections;
using System.Collections.Generic;
using Game.Logic.Phy.Actions;
using Game.Logic.Phy.Object;

// This class is used to wrap data of Bom to send from Connector to Handler
public class FireInfo
{
	public bool isDigMap;
	public int bomId;
	public int x;
	public int y;
	public int vx;
	public int vy;
	public int bomInfoId;
	public int bomActionCount;
    public List<int> timeInt;
    public List<int> actionType;
    public List<int> actionParam1;
    public List<int> actionParam2;
    public List<int> actionParam3;
    public List<int> actionParam4;

	public FireInfo(){
	     timeInt = new List<int>();
	     actionType = new List<int>();
	     actionParam1 = new List<int>();
	     actionParam2 = new List<int>();
	     actionParam3 = new List<int>();
	     actionParam4 = new List<int>();

	}

	public string ToString(){
		string str = "list";
		str += "/ id: " + bomId.ToString() + " x: " + x.ToString()+ 
				" y: "+y.ToString()+ " vx: "+vx.ToString()+
				 " vy: "+vy.ToString()+ " infoId: "+bomInfoId.ToString()+
				 " bomActionCount: "+ bomActionCount.ToString();
		return str;
	}
	public string DetailString(){
		string str = "bomActions:\n";
		for(int i  = 0; i < bomActionCount; i++){
			str+=i.ToString()+": time: "+timeInt[i].ToString()+
				" actType: "+((ActionType)actionType[i]).ToString()+
				" param1: "+ actionParam1[i].ToString()+
				" param2: "+ actionParam2[i].ToString()+
				" param3: "+ actionParam3[i].ToString()+
				" param4: "+ actionParam4[i].ToString()+"\n";
		}
		return str;
	}

}
