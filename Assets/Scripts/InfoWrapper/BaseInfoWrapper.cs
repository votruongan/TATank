using System;
using UnityEngine;
[Serializable]
public class BaseInfoWrapper {
    
	public virtual string ToString(){
        string output = JsonUtility.ToJson(this, true);
        return output;
	}

}