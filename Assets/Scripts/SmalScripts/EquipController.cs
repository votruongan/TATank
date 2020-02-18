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
        // Debug.Log(pathInResource);
        subSprite = Resources.LoadAll<Sprite>(pathInResource+"game");
    }

//For debugging: detect changes in equipid
    void Update(){
        if (publicEquipId != prevEquipId){
            ChangeSpriteSheet(true,publicEquipId);  
            prevEquipId = publicEquipId;
        }
    }
    
    void LateUpdate()
    {
        string spriteName = rendr.sprite.name;
        var newSprite = Array.Find(subSprite, item => item.name == spriteName);
        if (newSprite)
            rendr.sprite = newSprite;
    }

}
