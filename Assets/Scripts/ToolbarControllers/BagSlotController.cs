using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BagSlotController : MonoBehaviour
{
    public RawImage slotImage;
    // Start is called before the first frame update
    void Start()
    {
        slotImage = this.transform.GetChild(0).gameObject.GetComponent<RawImage>();
        UnLoadDisplay();
    }
    public void LoadDisplay(string path){
        Texture2D te = Resources.Load<Texture2D>(path);
        if (te == null){
            slotImage.color = new Color(28/255,10/255,200/255,1f);
            slotImage.texture = null;
            return;
        }
        slotImage.color = Color.white;
        slotImage.texture = te;
    }
    public void UnLoadDisplay(){
        slotImage.color = new Color(0f,0f,0f,0f);
    }
}
