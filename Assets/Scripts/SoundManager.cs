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
    // Start is called before the first frame update
    void Start()
    {
        musicVolume = PlayerPrefs.GetFloat("musicVolume",1f);
        effectVolume = PlayerPrefs.GetFloat("effectVolume",1f);
        player = gameObject.GetComponent<AudioSource>();
        player.volume = musicVolume;
        LoadAndPlay("bgm");
    }

    public void ChangeVolume(float music = -1f, float effect = -1f){
        if (music != -1f){
            PlayerPrefs.SetFloat("musicVolume",music);
            musicVolume = music;
        }
        if (effect != -1f){
            PlayerPrefs.SetFloat("effectVolume",effect);
            effectVolume = effect;
        }
        player.volume = music;
    }

    public void LoadAudio(string name){
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
