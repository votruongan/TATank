using UnityEngine;
public class BaseInfoWrapper {
    
	public virtual string ToString(){
        string output = JsonUtility.ToJson(this, true);
        return output;
	}

}