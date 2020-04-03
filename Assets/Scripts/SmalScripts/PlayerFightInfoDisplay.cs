using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerFightInfoDisplay: BaseObjectController
{
    public Slider bloodSlider;
    public Slider danderSlider;
    public Slider energySlider;
    public Text bloodText;
    public Text energyText;
    public GameObject danderButton;
    // public FightUIController fightUI;    
    public PlayerInfo original;
    int maxEnergy;
    int maxBlood;
    int previousDander;
    // Start is called before the first frame update
    void Start()
    {
        bloodSlider = this.FindChildObject("HealthIndicator").GetComponent<Slider>();
        danderSlider = this.FindChildObject("DanderIndicator").GetComponent<Slider>();
        energySlider = this.FindChildObject("EnergyIndicator").GetComponent<Slider>();
        danderButton = this.FindChildObject("DanderButton");
        bloodText = this.FindChildObject("HealthText").GetComponent<Text>();
        energyText = this.FindChildObject("EnergyText").GetComponent<Text>();
    }
    
    public void FightInfoDisplay(int energy = -1, int blood = -1, int dander = -1)
    {  
        // Debug.Log(original.ToString());
        if (energy != -1){
            if (energy > maxEnergy)
                maxEnergy = energy;
            energyText.text = energy.ToString();
            energySlider.value = (float) energy/(float)maxEnergy;
        }
        if (blood != -1){
            if (blood > maxBlood)
                maxBlood = blood;
            bloodText.text = blood.ToString();
            bloodSlider.value = (float)blood /(float)maxBlood;
        }
        if (dander != -1 && dander != previousDander){
            danderSlider.value = (float) dander/200 * 0.75f;
            if (dander >= 200){
                danderButton.gameObject.SetActive(true);
            }
            previousDander = dander;
        }
    }
}
