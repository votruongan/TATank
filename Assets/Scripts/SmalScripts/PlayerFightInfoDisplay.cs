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
    public GameObject danderButton;
    
    public PlayerInfo original;
    int previousEnergy;
    int previousBlood;
    int previousDander;
    // Start is called before the first frame update
    void Start()
    {
        bloodSlider = this.FindChildObject("HealthIndicator").GetComponent<Slider>();
        danderSlider = this.FindChildObject("DanderIndicator").GetComponent<Slider>();
        energySlider = this.FindChildObject("EnergyIndicator").GetComponent<Slider>();
        danderButton = this.FindChildObject("DanderButton");
        bloodText = this.FindChildObject("HealthText").GetComponent<Text>();
    }

    // Update is called once per frame
    public void FightInfoDisplay(int energy = -1, int blood = -1, int dander = -1)
    {  
        // Debug.Log(original.ToString());
        if (energy != -1 && energy != previousEnergy){
            energySlider.value = (float) energy/original.energy;
            previousEnergy = energy;
        }
        if (blood != -1 && blood != previousBlood){
            bloodText.text = blood.ToString();
            bloodSlider.value = (float)blood / original.blood;
            previousBlood = blood;
        }
        if (dander != -1 && dander != previousDander){
            danderSlider.value = (float) dander/200 * 0.75f;
            if (dander == 200){
                danderButton.gameObject.SetActive(true);
            }
            previousDander = dander;
        }
    }
}
