using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class CurrentPropDisplayer : MonoBehaviour
{
    // Start is called before the first frame update

    public void Append(Texture texture){
        GameObject go = new GameObject("CurProp");
        go.transform.parent = this.transform;
        int siz = Screen.height/10;
        RawImage ri = go.AddComponent<RawImage>();
        ri.texture = texture;
        ((RectTransform)go.transform).sizeDelta = new Vector2(siz,siz);
    }
    public void Reset(){
        foreach(Transform t in this.transform){
            Destroy(t.gameObject);
        }
    }
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
