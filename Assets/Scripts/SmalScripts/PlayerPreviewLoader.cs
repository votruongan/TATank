using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

public class PlayerPreviewLoader : BaseObjectController
{
    public RawImage cloth;
    public RawImage face;    
    public RawImage hair;
    public Text pName;
    private static bool isWeaponLoaded = false;
    private static UnityEngine.Object[] weaponPaths;
    PlayerInfo _playerInfo;

    public void LoadFromInfo(PlayerInfo pInfo){
        // Debug.Log("Processing load request ...");
        // Debug.Log(pInfo.ToString());
        if (pInfo == null){
            Debug.Log("PInfo is NULL");
            return;
        }
        _playerInfo = pInfo;
        // Debug.Log("hierarchy: " + this.gameObject.activeInHierarchy + " - self " + this.gameObject.activeSelf );
        if (!this.gameObject.activeInHierarchy){
            return;
        }
        StartCoroutine(ExecLoad(pInfo));
    }

    IEnumerator ExecLoad(PlayerInfo pInfo){
        while (hair == null || face == null || cloth == null){
            yield return null;
        }
        face.gameObject.SetActive(false);
        cloth.gameObject.SetActive(false);
        hair.gameObject.SetActive(false);
        yield return null;
        if (pName == null)
            pName = this.FindChildObject("PlayerName").GetComponent<Text>();
        if (!string.IsNullOrEmpty(pInfo.nickname))
            pName.text = pInfo.nickname;
        List<int> styleID = pInfo.GetStyleList();
        bool isMale = pInfo.sex;
        this.FindChildObject("LoadingPreview").gameObject.SetActive(false);
        hair.texture = loadImage("hair",isMale,styleID[(int)eItemType.hair]);
        hair.gameObject.SetActive(true);
        cloth.texture = loadImage("cloth",isMale,styleID[(int)eItemType.cloth]);
        cloth.gameObject.SetActive(true);
        face.texture = loadImage("face",isMale,styleID[(int)eItemType.face]);
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
            // Debug.Log(pathInResource);
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
        // Debug.Log(pathInResource);
        Texture2D res = Resources.Load<Texture2D>(pathInResource+"show");
        return res;
    }

    // Start is called before the first frame update
    void Start()
    {        
        // if (!isWeaponLoaded){
        //     isWeaponLoaded = true;
        //     weaponPaths = Resources.LoadAll("weapon");
        //     foreach(var w in weaponPaths){
        //         Debug.Log(w.name);
        //     }
        // }
        face = this.FindChildObject("Face").GetComponent<RawImage>();
        hair = this.FindChildObject("Hair").GetComponent<RawImage>();
        cloth = this.FindChildObject("Cloth").GetComponent<RawImage>();
        if (hair.texture == null){
            face.gameObject.SetActive(false);
            cloth.gameObject.SetActive(false);
            hair.gameObject.SetActive(false);
        }
        try{
            pName = this.FindChildObject("PlayerName").GetComponent<Text>();
            // pName.gameObject.SetActive(false);
        } catch(Exception e){

        }
        if (_playerInfo != null){
            StartCoroutine(ExecLoad(_playerInfo));
        }
    }

}
