using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundManager : MonoBehaviour
{
    
    [Header("Config")]
    public float musicVolume;
    public float effectVolume;
    [Header("Audio Clips")]
    public AudioClip bgm;
    public AudioClip testClip;
    
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
        LoadAndPlay("bgm");
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
    public GameObject PlayEffect(string name){
        if (effectVolume <0.05f){
            return null;
        }
        AudioClip src = Resources.Load<AudioClip>("sound/"+name);
        return PlayEffect(src,name);
    }
    public GameObject PlayEffect(AudioClip src,string name){
        // Debug.Log("Sfx check0");
        if (src == null){
            return null;
        }
        // Debug.Log("Sfx check1");
        GameObject gO = new GameObject(name);
        AudioSource sfx = gO.AddComponent(typeof(AudioSource)) as AudioSource;
        DelayedSelfDestroy dsd = gO.AddComponent(typeof(DelayedSelfDestroy)) as DelayedSelfDestroy;
        dsd.delayedSeconds = src.length;
        sfx.volume = effectVolume;
        sfx.clip = src;
        sfx.Play();
        return gO;
    }

    public void LoadAudio(string name){
        if (player == null)
            player = gameObject.GetComponent<AudioSource>();
        player.Stop();
        AudioClip aud = Resources.Load<AudioClip>("sound/"+name);
        if (aud == null)
            aud = Resources.Load<AudioClip>("sound/1008");
        player.clip = aud;
    }
    public void LoadAndPlay(string name){
        LoadAudio(name);
        player.Play();
    }
}
