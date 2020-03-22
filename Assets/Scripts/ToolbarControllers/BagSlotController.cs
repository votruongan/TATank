using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BagSlotController : MonoBehaviour
{   
    public bool isEmpty = true;
    public string bagPrefix = "BagSlot";
    public int typeOffset = 0;
    public bool isSelected = false;
    public bool isHightlighted = false;
    public int slotNo;
    public Texture2D shineBackground;
    public Texture2D normalBackground;
    public Texture2D selectedBackground;
    public BagAndInfoController bagAndInfoController;
    public RawImage slotImage;
    public RawImage slotBackground;

    // Start is called before the first frame update
    void Start()
    {   
        int limInd = gameObject.name.IndexOf("(");
        string numb = gameObject.name.Remove(0,limInd+1);
        numb = numb.Remove(numb.Length-1,1);
        slotNo = Int32.Parse(numb);
        if (slotBackground == null)
            slotBackground = this.gameObject.GetComponent<RawImage>();
        if (slotImage == null)
            slotImage = this.transform.GetChild(0).gameObject.GetComponent<RawImage>();
        if (bagAndInfoController == null)
            bagAndInfoController = GameObject.Find("BagAndInfoPanel").GetComponent<BagAndInfoController>();
        UnloadDisplay();
    }

    public void Shine(){
        isHightlighted = true;
        slotBackground.texture = shineBackground;
        slotBackground.color = Color.white;
    }
    public void NormalbackGround(){
        isHightlighted = false;
        slotBackground.texture = normalBackground;
        if (normalBackground == null)
            slotBackground.color = new Color(0f,0f,0f,0f);
    }

    public void Select(){
        if (slotImage.texture == null){
            bagAndInfoController.BagSlotDeSelected();
            return;
        }
        isSelected = !isSelected;
        if (isSelected){
            bagAndInfoController.BagSlotSelected(slotNo, bagPrefix,typeOffset);
            slotBackground.texture = selectedBackground;
        } else {
            slotBackground.texture = normalBackground;
            bagAndInfoController.BagSlotDeSelected();
        }
    }
    public void LoadDisplay(string path){
        Texture2D te = Resources.Load<Texture2D>(path);
        LoadDisplay(te);
    }

    public void LoadDisplay(Texture2D txtr){
        StartCoroutine(LoadToDisplay(txtr));
    }
    IEnumerator LoadToDisplay(Texture2D txtr){
        while(slotImage == null){
            yield return new WaitForSeconds(0.2f);
        }
        isEmpty = false;
        if (txtr == null){
            slotImage.color = new Color(28/255,10/255,200/255,1f);
            slotImage.texture = null;
        }else{
            slotImage.color = Color.white;
            slotImage.texture = txtr;
        }
    }

    public void UnloadDisplay(){
        // Debug.Log("Unloading: " + this.gameObject.name);
        isEmpty = true;
        slotImage.texture = null;
        slotImage.color = new Color(0f,0f,0f,0f);
    }
}
