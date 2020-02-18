using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerPreviewLoader : MonoBehaviour
{
    public RawImage cloth;
    public RawImage face;    
    public RawImage hair;
    
    public void LoadFromInfo(PlayerInfo pInfo){
        Debug.Log("Processing load request ...");
        Debug.Log(pInfo.ToString());
        if (pInfo == null){
            Debug.Log("PInfo is NULL");
            return;
        }
        List<int> styleID = pInfo.GetStyleList();
        bool isMale = pInfo.sex;
        this.transform.GetChild(3).gameObject.SetActive(false);
        hair.texture = loadImage("hair",isMale,styleID[2]);
        hair.gameObject.SetActive(true);
        cloth.texture = loadImage("cloth",isMale,styleID[4]);
        cloth.gameObject.SetActive(true);
        face.texture = loadImage("face",isMale,styleID[5]);
        face.gameObject.SetActive(true);
    }
    
    private Texture2D loadImage(string equipType, bool isMale, int equipId){
        string pathInResource = "Equip";
        string gender = (isMale)?("/m"):("/f");
        gender = gender +"/" + equipType;
        equipId = PlayerInfo.ConvertStyleId(equipType,isMale,equipId);
        if (equipType == "face"){
            if (equipId > 0){
                pathInResource = pathInResource + gender + "/" +equipType + equipId.ToString() + "/icon_1";            
            }
            else{
                pathInResource = pathInResource + gender + "/default/icon_1";   
            }
            Debug.Log(pathInResource);
            return Resources.Load<Texture2D>(pathInResource);
        }
        if (equipId > 0){
            pathInResource = pathInResource + gender + "/" +equipType + equipId.ToString() + "/1/";            
        }
        else{
            pathInResource = pathInResource + gender + "/default/1/";   
        }
        if (equipType == "hair"){
            pathInResource = pathInResource + "B/";
        }
        Debug.Log(pathInResource);
        Texture2D res = Resources.Load<Texture2D>(pathInResource+"show");
        return res;
    }

    // Start is called before the first frame update
    void Start()
    {        
        face = this.transform.GetChild(0).gameObject.GetComponent<RawImage>();
        hair = this.transform.GetChild(1).gameObject.GetComponent<RawImage>();
        cloth = this.transform.GetChild(2).gameObject.GetComponent<RawImage>();
        face.gameObject.SetActive(false);
        cloth.gameObject.SetActive(false);
        hair.gameObject.SetActive(false);
    }

}
