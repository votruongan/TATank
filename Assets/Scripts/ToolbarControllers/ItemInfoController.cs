using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ItemInfoController : BaseObjectController
{
    public RectTransform rt;
    public Text itemName;
    public Text attackProp;
    public Text agilityProp;
    public Text defenceProp;
    public Text luckProp;
    string foreAtt; string foreAgi; string foreDef; string foreLuc;

    // Start is called before the first frame update
    void Start()
    {
        if (rt != null){
            return;
        }
        rt = this.gameObject.GetComponent<RectTransform>();
        itemName = this.FindChildObject("ItemName").GetComponent<Text>();
        attackProp = this.FindChildObject("AttackProp").GetComponent<Text>();
        agilityProp = this.FindChildObject("AgilityProp").GetComponent<Text>();
        defenceProp = this.FindChildObject("DefenceProp").GetComponent<Text>();
        luckProp = this.FindChildObject("LuckProp").GetComponent<Text>();
        foreAtt = attackProp.text;foreAgi = agilityProp.text;foreDef = defenceProp.text;foreLuc = luckProp.text;
    }

    public void Hide(){
        this.gameObject.SetActive(false);
    }
    public void ShowInfo(ItemTemplateInfo iti, ItemInfo ii, RectTransform targ){
        if (rt == null){
            Start();
        }
        rt.anchoredPosition = targ.anchoredPosition;
        itemName.text = iti.Name;
        attackProp.text = foreAtt + iti.Attack.ToString() + " (+" + ii.AttackCompose.ToString()+")";
        agilityProp.text = foreAgi + iti.Agility.ToString() + " (+" + ii.AgilityCompose.ToString()+")";
        defenceProp.text = foreDef + iti.Defence.ToString() + " (+" + ii.DefendCompose.ToString()+")";
        luckProp.text = foreLuc + iti.Luck.ToString() + " (+" + ii.LuckCompose.ToString()+")";
        this.gameObject.SetActive(true);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
