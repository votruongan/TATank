using UnityEngine;
[System.Serializable]
public class BaseInfoWrapper {
    
	public virtual string ToString(){
        string output = JsonUtility.ToJson(this, true);
        return output;
	}

}