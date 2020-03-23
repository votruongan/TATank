using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BagAndInfoController : BaseToolbarController
{
    public GameController gameController;
    public ItemInfoController itemInfoController;
    public PlayerPreviewLoader mpl;
    public GridLayoutGroup bagGrid;
    public GridLayoutGroup[] infoGrid;
    public int infoSelectingPlace = -1;
    public int bagSelectingPlace = -1;
    public int infoHightligting = -1;
    List<BagSlotController> bagSlots;
    List<BagSlotController> infoSlots;
    // Start is called before the first frame update
    List<ItemInfo> localBag;

    public bool ShowBag(eBagType bagType, int itemOffset = 0){
        foreach(BagSlotController bsc in bagSlots){
            bsc.UnloadDisplay();
        }
        List<ItemInfo> items = gameController.connector.localBags[(int)bagType];
        if (items == null)
            return false;
        Debug.Log("SHOWBAG " + (eBagType) bagType);
        localBag = items;
        StartCoroutine(LoadBagSlot(items,gameController.connector.localPlayerInfo.sex,"BagSlot",itemOffset));
        return true;
    }

    public void BagSlotDeSelected(){
        if (infoHightligting > -1){
            infoSlots[infoHightligting].NormalbackGround();
            infoSlots[infoHightligting+1].NormalbackGround();
            infoHightligting = -1;
        }
        infoSelectingPlace = -1;
        bagSelectingPlace = -1;
        itemInfoController.Hide();
        // Debug.Log("Deselected");
    }
    public void BagSlotSelected(int bagPlace, string bagPrefix, int offset = 0){
        gameController.soundManager.PlayEffect("choose");
        foreach(ItemInfo item in localBag){
            if (item.Place != bagPlace + offset)
                continue;
            ItemTemplateInfo iti = gameController.connector.FindItemTemplateInfo(item.Pic,gameController.connector.localPlayerInfo.sex);
            // Debug.Log(item.ToString());
            // Debug.Log(iti);
            RectTransform slotTransform = null;
            //Touch a slot in bag
            if (bagPrefix[0] == 'B'){
                Debug.Log("BagSelected");
                DeselectSlotInList(ref bagSlots, ref bagSelectingPlace);
                slotTransform = (RectTransform) bagSlots[bagPlace].transform;
                bagSelectingPlace = bagPlace;
                if (infoSelectingPlace > -1){
                    Debug.Log("Swap to bagg : " + infoSelectingPlace);
                    gameController.connector.SendChangePlaceItem(eBagType.MainBag,infoSelectingPlace,eBagType.MainBag,bagSelectingPlace+31,0);
                }
                infoSelectingPlace = -1;
                int p = (int)getEquipType(gameController.connector.localPlayerInfo.sex,item);
                if (p > -1){
                    infoSlots[p].GetComponent<BagSlotController>().Shine();
                    if (item.itemType == eItemType.ring || item.itemType == eItemType.armlet){
                        infoSlots[p+1].GetComponent<BagSlotController>().Shine();
                    }
                }
                infoHightligting = p;
            } 
            //Touch a slot in info
            else {
                slotTransform = (RectTransform) infoSlots[bagPlace].transform;
                if (infoSelectingPlace < 0){
                    //check if want equip item
                    if (item.Place == infoHightligting || 
                        matchPlaceAndType(item,eItemType.ring,9,10) ||
                        matchPlaceAndType(item,eItemType.armlet,7,8)){
                        //PERFORM CHANGE PLACE
                        gameController.connector.SendChangePlaceItem(eBagType.MainBag,bagSelectingPlace+31,eBagType.MainBag,bagPlace,0);
                        bagSlots[bagSelectingPlace].Select();
                        infoSlots[bagPlace].NormalbackGround();
                        infoSlots[bagPlace+1].NormalbackGround();
                        infoSlots[bagPlace-1].NormalbackGround();
                        // infoSelectingPlace = -1;
                        // bagSelectingPlace = -1;
                        // infoHightligting = -1;
                        // itemInfoController.Hide();
                        return;
                    }
                } else {
                    bagSelectingPlace = -1;
                    DeselectSlotInList(ref infoSlots, ref infoSelectingPlace);
                }
                infoSelectingPlace = bagPlace;
            }
            itemInfoController.ShowInfo(iti,item,slotTransform);
            // Debug.Log("BAG SLOT SELECTED " + bagPrefix + bagPlace + item.itemType);
            return;
        }
        // check if want remove item
        BagSlotDeSelected();
    }
    void DeselectSlotInList(ref List<BagSlotController> bscl, ref int selecting){
        if (selecting > -1){
            BagSlotController bsc = bscl[selecting];
            if (bsc.isSelected)
                bsc.Select();
            selecting = -1;
        }
    }
    bool matchPlaceAndType(ItemInfo item, eItemType typ, int place0, int place1){
        if (item.itemType == typ && item.Place == place0 && item.Place == place1){
            return true;
        }
        return false;
    }
    public void ResetDisplay(){
        if (bagSlots == null){
            return;
        }
        for (int i = 0; i < 30; i++){
            if (i < bagSlots.Count){
                bagSlots[i].UnloadDisplay();
            }
            if (i < infoSlots.Count){
                infoSlots[i].UnloadDisplay();
            }
        }
    }
    protected override void Start(){
        base.Start();
        GameObject tbsc= null;
        bagSlots = new List<BagSlotController>();
        infoSlots = new List<BagSlotController>();
        for (int i = 0; i < 30; i++){
            tbsc = GameObject.Find("BagSlot (" + i.ToString() + ")");
            if (tbsc != null){
                bagSlots.Add(tbsc.GetComponent<BagSlotController>());
            }
            tbsc = GameObject.Find("InfoSlot (" + i.ToString() + ")");
            if (tbsc != null){
                infoSlots.Add(tbsc.GetComponent<BagSlotController>());
                bagSlots[i].UnloadDisplay();
            }
        }
        OnEnable();
    }
    public void OnEnable()
    {
        if (gameController == null){
            gameController = GameObject.Find("GameController").GetComponent<GameController>();
            bagGrid.cellSize = new Vector2(Screen.width * 16/192,Screen.height * 16/108);
            foreach (GridLayoutGroup infs in infoGrid){
                infs.cellSize = new Vector2(Screen.width * 0.0625f,Screen.height * 11/108);
            }
        }
        LoadStats();
        StartCoroutine(WaitAndLoad());
        if (mpl == null)
            mpl = this.FindChildObject("MainPlayerPreview").GetComponent<PlayerPreviewLoader>();
        mpl.LoadFromInfo(gameController.GetLocalPlayerInfo());
    }
    public void LoadStats(){
        StartCoroutine(ExecLoadStat());

    }

    [Header("StatDisplay")]
    public Text AttackStat;public Text AgilityStat;public Text DefenceStat;public Text LuckStat;public Text GradeStat;
    public Text PowerStat;public Text DamageStat;public Text ArmorStat;public Text BloodStat;public Text EnergyStat;
    public Text MoneyStat;public Text GoldStat;public Text GiftTokenStat;public Text MedalStat;
    IEnumerator ExecLoadStat(){
        PlayerInfo pi = gameController.connector.localPlayerInfo;
        while (pi == null){
            yield return null;
        }
        AttackStat.text = pi.attack.ToString();
        AgilityStat.text = pi.agility.ToString();
        DefenceStat.text = pi.defence.ToString();
        LuckStat.text = pi.luck.ToString();
        PowerStat.text = pi.fightPower.ToString();
        GradeStat.text = pi.grade.ToString();
        MoneyStat.text = pi.money.ToString();
        GoldStat.text = pi.gold.ToString();
        GiftTokenStat.text = pi.giftToken.ToString();
        MedalStat.text = pi.medal.ToString();
    }   
    
    IEnumerator WaitAndLoad(){
        List<ItemInfo> items = gameController.connector.localBags[(int)eBagType.MainBag];
        localBag = items;
        while(items == null){
            yield return new WaitForSeconds(0.02f);
            items = gameController.connector.localBags[(int)eBagType.MainBag];
        }
        StartCoroutine(LoadBagSlot(items,gameController.connector.localPlayerInfo.sex,"BagSlot",31));
        StartCoroutine(LoadBagSlot(items,gameController.connector.localPlayerInfo.sex,"InfoSlot",0,30));
    }   
    
    IEnumerator LoadBagSlot(List<ItemInfo> items, bool isMale, string bagPrefix, int slotMin = 0, int slotMax = 2810){
        yield return new WaitForSeconds(0.2f);
        foreach (ItemInfo item in items){
            if (item.Place < slotMin || item.Place > slotMax)
                continue;
            loadToSlot(isMale, bagPrefix,item.Place - slotMin,item);
            yield return null;
        }
    }
    
     void loadToSlot( bool isMale, string slotPrefix, int place, ItemInfo item){
        // Debug.Log(slotPrefix +" ("+place.ToString()+")");
        GameObject gO = GameObject.Find(slotPrefix +" ("+place.ToString()+")");
        if (gO== null){
            return;
        }            
        BagSlotController slot = gO.GetComponent<BagSlotController>();
        if (slot == null)
            return;
        Texture2D te = tryLoadEquipDisplay(isMale, item);
        // Debug.Log("Loading " + slotPrefix + " " + place + "-" + item.Pic +" : " +(te != null));
        slot.LoadDisplay(te);
    }

    int placeOfEquipType(string equipType){
        int res = -1;
        switch(equipType){
            case "head": res = 0; break;
            case "hair": res = 2; break;
            case "face": res = 3; break;
            case "cloth": res = 4; break;
            case "weapon": res = 6; break;
        }
        return res;
    }

    eItemType getEquipType(bool isMale, ItemInfo item){
        if (item.itemType != eItemType.noType){
            return item.itemType;
        }
        tryLoadEquipDisplay(isMale, item);
        return item.itemType;
    }
    string spitEquipType(string equipName){
        string equipType = equipName;
        for (int i = equipType.Length - 1; i >= 0; i--){
            if (equipType[i] - 48 >-1 && equipType[i] - 48 < 10){
                continue;
            }
            equipType = equipType.Remove(i + 1,equipType.Length-i -1);
            break;
        }
        return equipType;
    }
    //Try load equip
    Texture2D tryLoadEquipDisplay(bool isMale, ItemInfo item){
        string sexEquipString = "Equip/";
        string equipName = item.Pic;
        if (gameController.connector.localPlayerInfo.sex)
            sexEquipString += "m/";
        else
            sexEquipString += "f/";
        //strip equip type - eliminate all number
        string equipType = spitEquipType(equipName);
        //try load in gender-related equips
        // Debug.Log("try: " + sexEquipString+equipType+"/"+equipName+"/icon_1");
        Texture2D te = Resources.Load<Texture2D>(sexEquipString+equipType+"/"+equipName+"/icon_1");
        Enum.TryParse(equipType,true,out item.itemType);
        if (te == null){
            //try load in not gender-related equips
            // Debug.Log("try: " + "Equip/common/"+equipType+"/"+equipName+"/icon");
            te = Resources.Load<Texture2D>("Equip/common/"+equipType+"/"+equipName+"/icon");
            //try load weapon
            if (te == null){
                item.itemType = eItemType.arm;
                // Debug.Log("try: " + "Equip/notype/"+equipName+"/00");
                te = Resources.Load<Texture2D>("Equip/notype/"+equipName+"/00");
                //try load notype
                if (te == null){
                    item.itemType = eItemType.noType;
                    // Debug.Log("try: " + "Equip/notype/"+equipName+"/icon");
                    te = Resources.Load<Texture2D>("Equip/notype/"+equipName+"/icon");
                    item.itemType = eItemType.ring;
                }
            }            
        }
        return te;
    }

    
    public virtual void Close(){
        gameController.uiController.UpdateMainPlayerPreview(gameController.GetLocalPlayerInfo());
        base.Close();
    }
}
