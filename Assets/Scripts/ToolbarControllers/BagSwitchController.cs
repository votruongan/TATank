using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BagSwitchController : MonoBehaviour
{
    public bool isOnEquip;
    public Texture2D onEquip;
    public Texture2D onItem;

    public RawImage displayImg;
    // Start is called before the first frame update
    void Start()
    {
        if (displayImg == null)
            displayImg = this.transform.GetChild(0).gameObject.GetComponent<RawImage>();
        isOnEquip = true;
        displayImg.texture = onEquip;
    }

    public void SwitchToEquip(){
        if (isOnEquip)
            return;
        isOnEquip = true;
        displayImg.texture = onEquip;
    }

    public void SwitchToItem(){
        if (!isOnEquip)
            return;
        isOnEquip = false;
        displayImg.texture = onItem;
    }
}
