using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SettingsPanelController : BaseToolbarController
{
    public Slider musicSlider;
    public Slider effectSlider;
    public SoundManager soundManager;
    private void OnEnable() {
        if (soundManager == null){
            soundManager = GameObject.Find("SoundManager").GetComponent<SoundManager>();
        }
        musicSlider.value = soundManager.musicVolume;
        effectSlider.value = soundManager.effectVolume;
    }
}
