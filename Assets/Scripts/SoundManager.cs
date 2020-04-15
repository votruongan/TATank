using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using ConnectorSpace;

public class SoundManager : MonoBehaviour
{
    
    [Header("Config")]
    public float musicVolume;
    public float effectVolume;
    [Header("Audio Clips")]
    public AudioClip bgm;
    public AudioClip testClip;
    List<AudioClip> audioQueue;
    
    [Header("For Debugging")]
    public AudioSource player;
    protected static SoundManager instance;
    public static SoundManager GetInstance(){
        return instance;
    }
    // Start is called before the first frame update
    void Start()
    {
        instance = this;
        player = gameObject.GetComponent<AudioSource>();
        musicVolume = PlayerPrefs.GetFloat("musicVolume",1f);
        effectVolume = PlayerPrefs.GetFloat("effectVolume",1f);
        player.volume = musicVolume;
        // LoadAndPlay("bgm");
    }

    public void ChangeVolume(float music = -1f, float effect = -1f){
        if (music > -0.5f){
            PlayerPrefs.SetFloat("musicVolume",music);
            musicVolume = music;
            player.volume = music;
        }
        if (effect > -0.5f){
            PlayerPrefs.SetFloat("effectVolume",effect);
            effectVolume = effect;
        }
    }
    public void PlayEffectOnce(string name){
        if (GameObject.Find(name)){
            return;
        }
        PlayEffect(name);
    }
    public void StopEffect(string name){
        GameObject go = GameObject.Find(name);
        if (go != null)
            Destroy(go);
    }
    public void PlayBGM(string name){
        PlaySound(name,true);
    }
    public void PlayEffect(string name){
        if (effectVolume <0.05f){
            return;
        }
        PlaySound(name);
    }
    GameObject PlayEffect(AudioClip src,string gameObjectName){
        // Debug.Log("Sfx check0");
        if (src == null){
            return null;
        }
        // Debug.Log("Sfx check1");
        GameObject gO = new GameObject(gameObjectName);
        AudioSource sfx = gO.AddComponent(typeof(AudioSource)) as AudioSource;
        DelayedSelfDestroy dsd = gO.AddComponent(typeof(DelayedSelfDestroy)) as DelayedSelfDestroy;
        dsd.delayedSeconds = src.length;
        sfx.volume = effectVolume;
        sfx.clip = src;
        sfx.Play();
        return gO;
    }
    void PlayBGM(AudioClip src){
        if (player == null)
            player = gameObject.GetComponent<AudioSource>();
        player.Stop();
        AudioClip aud = src;
        player.clip = aud;
        player.Play();
    }
    private void PlaySound(string name, bool isBGM=false){
        // try load built-in sound
        // Debug.Log("PlaySound: " + name);
        AudioClip au = Resources.Load<AudioClip>("sound/" + name);
        if (au != null){
            BasePlaySound(name, au,isBGM);
            return;
        }
        // No built-in sound -> try to fetch the sound in cache
        byte[] rawData = null;
        string filePath = Application.persistentDataPath +"/" + name +".ogg";
        if (!System.IO.File.Exists(filePath)){
            //No sound in cache -> download the sound from server
            StartCoroutine(DownloadAndPlay(name, filePath));
            return;
        }
        // cached sound
        StartCoroutine(PlayFromCache(name, filePath,isBGM));
    }
    private void BasePlaySound(string name, AudioClip ac, bool isBGM=false){
        if (ac == null){
            Debug.Log("BasePlaySound: ac is null ");
            return;
        }
        // Debug.Log("BasePlaySound: isBGM: " + isBGM +ac);
        if (isBGM){
            PlayBGM(ac);
        }else{
            PlayEffect(ac,name);
        }
    }
    IEnumerator DownloadAndPlay(string name, string filePath, bool isBGM=false)
    {
        // Debug.Log("DownloadAndPlay: " + filePath);
        string url = ConfigMgr.ResourcesUrl +"/sounds/" + name + ".ogg";
        UnityWebRequest www = UnityWebRequest.Get(url);
        yield return www.Send();
        if (www.isNetworkError || www.isHttpError)
        {
            Debug.Log(url + "\n Get SOUND " + name  +" failed: "+www.error);
            PlaySound("1028");
        }
        else
        {
            string savePath = string.Format("{0}/{1}.ogg", Application.persistentDataPath, name);        
            System.IO.File.WriteAllBytes(savePath, www.downloadHandler.data);
            Debug.Log("DownloadAndPlay: WritetoFile: " + savePath);
            Debug.Log("DownloadAndPlay: Check filePath Existed: " + System.IO.File.Exists(filePath));
            yield return StartCoroutine( PlayFromCache(name, filePath,isBGM));
        }
    }
    IEnumerator PlayFromCache(string name, string filePath, bool isBGM=false){
        // Debug.Log("PlayFromCache: " + filePath);
        UnityWebRequest req = UnityWebRequestMultimedia.GetAudioClip(ConfigMgr.FileScheme+filePath, AudioType.OGGVORBIS);
        yield return req.Send();            
        if (req.isNetworkError)
        {
            Debug.Log("Convert SOUND " + name +" failed: "+req.error);
        }
        else
        {
            AudioClip ac = DownloadHandlerAudioClip.GetContent(req);
            BasePlaySound(name, ac,isBGM);
        }
    }
}
