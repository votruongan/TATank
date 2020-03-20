using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EquipController : MonoBehaviour
{
    public SpriteRenderer rendr;
    public string clothPath;
    
    public string equipType;
    public Sprite[] subSprite;
    public int publicEquipId = 1;
    int prevEquipId;
    // Start is called before the first frame update
    void Start()
    {
        rendr = this.gameObject.GetComponent<SpriteRenderer>();
        ChangeSpriteSheet(true,1);
    }

    public void ChangeSpriteSheet(bool isMale, int equipId){
        string pathInResource = "Equip";
        string gender = (isMale)?("/m"):("/f");
        gender = gender +"/" + equipType;
        if (equipId > 1000){
            equipId = PlayerInfo.ConvertStyleId(equipType,isMale,equipId);
        }
        if (equipId > 0){
            pathInResource = pathInResource + gender + "/" +equipType + equipId.ToString() + "/1/";            
        }
        else{
            pathInResource = pathInResource + gender + "/default/";   
        }
        if (equipType == "hair"){
            pathInResource = pathInResource + "B/";
        }
        clothPath = pathInResource + "game";
        Debug.Log("Changing " + equipType + " to " + pathInResource);
        // Debug.Log(pathInResource);
        subSprite = Resources.LoadAll<Sprite>(pathInResource+"game");
        if (subSprite == null){
            ChangeSpriteSheet(isMale,0);
        }
        if (equipType == "hair"){
            //Shift position according to hair length;
            Vector3 pos = this.transform.localPosition;
            this.transform.localPosition = new Vector3 (pos.x,(subSprite[0].rect.height<40)?(0.2f):(0.18f),0f);
        }
    }


    public void ChangeSpriteSheet(bool isMale, string equipName){
        string pathInResource = "Equip";
        string gender = (isMale)?("/m"):("/f");
        gender = gender +"/" + equipType;
        pathInResource = pathInResource + gender + "/" +equipType + "/" + equipName + "/";
        if (equipType == "hair"){
            pathInResource = pathInResource + "B/";
        }
        clothPath = pathInResource + "game";
        // Debug.Log(pathInResource);
        subSprite = Resources.LoadAll<Sprite>(pathInResource+"game");
        if (subSprite == null){
            ChangeSpriteSheet(isMale,0);
        }
    }

//For debugging: detect changes in equipid
    void Update(){
        // if (publicEquipId != prevEquipId){
        //     ChangeSpriteSheet(true,publicEquipId);  
        //     prevEquipId = publicEquipId;
        // }
    }
    
    void LateUpdate()
    {
        string spriteName = rendr.sprite.name;
        var newSprite = Array.Find(subSprite, item => item.name == spriteName);
        if (newSprite)
            rendr.sprite = newSprite;
    }

}
