using System;
using UnityEngine;
[Serializable]
public class BaseInfoWrapper {
    
	public override string ToString(){
                string output = JsonUtility.ToJson(this, true);
                return output;
	}

}