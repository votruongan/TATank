using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using System.Linq;
using ConnectorSpace;

public class PlayerPreviewLoader : BaseObjectController
{
    public RawImage cloth;
    public RawImage face;    
    public RawImage hair;
    public RawImage arm;
    public Text pName;
    private static bool isWeaponLoaded = false;
    private static UnityEngine.Object[] weaponPaths;
    PlayerInfo _playerInfo;

    public void LoadFromInfo(PlayerInfo pInfo){
        Debug.Log(this.gameObject.name + " processing load request ...");
        this.gameObject.SetActive(true);
        if (pInfo == null){
            Debug.Log("PInfo is NULL");
            return;
        }
        _playerInfo = pInfo;
        // Debug.Log("hierarchy: " + this.gameObject.activeInHierarchy + " - self " + this.gameObject.activeSelf );
        if (this.gameObject.activeSelf)
            StartCoroutine(ExecLoad(pInfo));
    }

    void DisableDisplay(){
        face.gameObject.SetActive(false);
        cloth.gameObject.SetActive(false);
        hair.gameObject.SetActive(false);
        arm.gameObject.SetActive(false);
    }

    IEnumerator ExecLoad(PlayerInfo pInfo){
        Debug.Log( this.gameObject.name + " : " + pInfo.ToString());
        while (hair == null || face == null || cloth == null){
            this.gameObject.SetActive(false);
            yield return new WaitForSeconds(0.02f);
        }
        // DisableDisplay();
        yield return null;
        if (pName == null)
            pName = this.FindChildObject("PlayerName").GetComponent<Text>();
        if (!string.IsNullOrEmpty(pInfo.nickname))
            pName.text = pInfo.nickname;
        List<int> styleID = pInfo.GetStyleList();
        bool isMale = pInfo.sex;
        SetDisplayerTexture(hair, "hair",isMale,styleID[(int)eItemType.hair]);
        SetDisplayerTexture(cloth, "cloth",isMale,styleID[(int)eItemType.cloth]);
        SetDisplayerTexture(face, "face",isMale,styleID[(int)eItemType.face]);
        SetDisplayerTexture(arm, "arm",isMale,styleID[(int)eItemType.arm]);
        this.FindChildObject("LoadingPreview").gameObject.SetActive(false);
    }
    private void SetDisplayerTexture(RawImage target, string type, bool isMale, int id){
        target.gameObject.SetActive(false);
        StartCoroutine(LoadImageOnline(target, type, isMale,id));
    }

    IEnumerator LoadImageOnline(RawImage target, string equipType, bool isMale, int equipId){
        Texture2D downloaded = null;
        bool isDone = false;
        string pathInResource = "/image";
        string pic = ConnectorManager.FindItemTemplateInfo(equipId,isMale).Pic;
        if (equipType == "arm"){
            pathInResource = pathInResource + "/arm/" +pic+ "/1/0/show";
        }
        else{
            pathInResource = pathInResource + "/equip";
            string gender = (isMale)?("/m/"):("/f/");
            // gender = gender  + equipType;
            pathInResource = pathInResource + gender + equipType + "/" +pic;
            // equipId = PlayerInfo.ConvertStyleId(equipType,isMale,equipId);
            if (equipType == "face"){
                pathInResource = pathInResource +  "/icon_1";
            } else {
                pathInResource = pathInResource + "/1";
                if (equipType == "hair"){
                    pathInResource = pathInResource + "/B";
                }
                pathInResource = pathInResource + "/show"; 
            }
        }
        // yield return StartCoroutine(GetImage(pathInResource, downloaded, isDone));
        yield return StartCoroutine(GetImage(pathInResource, (result) => {
            Debug.Log("Setting " + pic);
            target.texture = result;
            if (result == null){
                target.gameObject.SetActive(false);
            }else{
                target.gameObject.SetActive(true);
            }
        }));
        // while(!isDone){
        //     yield return new WaitForSeconds(0.04f);
        // }
        // Debug.Log("Setting " + pic);
        // target.texture = downloaded;
        // if (downloaded == null){
        //     target.gameObject.SetActive(false);
        // }
    }

    IEnumerator GetImage(string baseUrl,System.Action<Texture2D> callback){//, Texture2D target, bool isDone){
        // try load from cache
        Texture2D tex = null;
        string cacheDir = Application.temporaryCachePath + baseUrl + ".png";
        if (System.IO.File.Exists(cacheDir)){
            byte[] ba;
            ba = System.IO.File.ReadAllBytes(cacheDir);
            tex = new Texture2D(1,1);
            tex.LoadImage(ba);
        } else {
        //download and create new image
            // Debug.Log(baseUrl);
            string showDir = ConfigMgr.ResourcesUrl + baseUrl + ".png?lv=14&";
            // Debug.Log(showDir);
            UnityWebRequest www = UnityWebRequestTexture.GetTexture(showDir);
            yield return www.SendWebRequest();
            if(www.isNetworkError || www.isHttpError) {
                Debug.Log("get EQUIP "+name+" error: " + www.error);
            }
            else {
                tex = ((DownloadHandlerTexture)www.downloadHandler).texture as Texture2D;
                // target = tex as Texture2D;
            }
            //Write file to cache
            new System.IO.FileInfo(cacheDir).Directory.Create();
            System.IO.File.WriteAllBytes(cacheDir, tex.EncodeToPNG());
            // Debug.Log("Done " + baseUrl);
        }
        if (callback != null) callback(tex);
    }

    // Load from Built-in version
    // private void SetDisplayerTexture(RawImage target, string type, bool isMale, int id){
    //     Texture2D t = loadImage(type,isMale,id);
    //     if (t == null){
    //         target.gameObject.SetActive(false);
    //         return;
    //     }
    //     target.texture = t;
    //     target.gameObject.SetActive(true);
    // }

    // Load from built-in
    // private Texture2D loadImage(string equipType, bool isMale, int equipId){
    //     string pathInResource = "Equip";
    //     string gender = (isMale)?("/m"):("/f");
    //     gender = gender +"/" + equipType;
    //     gender = (equipType=="arm")?("/notype"):(gender);
    //     // equipId = PlayerInfo.ConvertStyleId(equipType,isMale,equipId);
    //     string pic = ConnectorManager.FindItemTemplateInfo(equipId,isMale).Pic;
    //     if (equipType == "face"){
    //         // if (equipId > 0){
    //         //     pathInResource = pathInResource + gender + "/" +equipType + equipId.ToString() + "/icon_1";            
    //         // }
    //         // else{
    //         //     pathInResource = pathInResource + gender + "/default/icon_1";   
    //         // }
    //         // Debug.Log(pathInResource);
    //         pathInResource = pathInResource + gender + "/" +pic+ "/icon_1"; 
    //         return Resources.Load<Texture2D>(pathInResource);
    //     }
    //     if (equipType == "arm"){
    //         pathInResource = pathInResource + "/notype/" +pic+ "/1/0/show"; 
    //         return Resources.Load<Texture2D>(pathInResource);
    //     }
    //     // if (equipId > 0){
    //     //     pathInResource = pathInResource + gender + "/" +equipType + equipId.ToString() + "/1/";                        
    //     // }
    //     // else{
    //     //     pathInResource = pathInResource + gender + "/default/1/";   
    //     // }
    //     pathInResource = pathInResource + gender + "/" +pic+ "/1"; 

    //     if (equipType == "hair"){
    //         pathInResource = pathInResource + "/B";
    //     }
    //     // Debug.Log(pathInResource);
    //     Texture2D res = Resources.Load<Texture2D>(pathInResource+"/show");
    //     return res;
    // }

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
        if (face == null){
            face = this.FindChildObject("Face").GetComponent<RawImage>();
            hair = this.FindChildObject("Hair").GetComponent<RawImage>();
            cloth = this.FindChildObject("Cloth").GetComponent<RawImage>();
            arm = this.FindChildObject("Arm").GetComponent<RawImage>();
        }
        if (face.texture == null){
            DisableDisplay();
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
