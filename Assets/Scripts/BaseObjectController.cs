using UnityEngine;

public class BaseObjectController : MonoBehaviour {
    protected GameObject FindChildObject(string gOName){
        for (int i = 0; i < this.transform.childCount;i++){
            if (this.transform.GetChild(i).name == gOName){
                return this.transform.GetChild(i).gameObject;
            }
        }
        return null;
    }

}