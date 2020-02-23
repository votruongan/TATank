using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BagAndInfoSetUp : BaseToolbarController
{
    public GameController gameController;
    public PlayerPreviewLoader mpl;
    public GridLayoutGroup bagSlots;
    public GridLayoutGroup infoSlots;
    private string sexEquipString = "Equip/";
    // Start is called before the first frame update
    protected override void Start()
    {
        gameController = GameObject.Find("GameController").GetComponent<GameController>();
        bagSlots.cellSize = new Vector2(Screen.width * 16/192,Screen.height * 16/108);
        infoSlots.cellSize = new Vector2(Screen.width * 0.0625f,Screen.height * 11/108);
        if (gameController.connector.localPlayerInfo.sex)
            sexEquipString += "m/";
        else
            sexEquipString += "f/";
        StartCoroutine(WaitAndLoad());
        base.Start();
        mpl = this.FindChildObject("MainPlayerPreview").GetComponent<PlayerPreviewLoader>();
        mpl.LoadFromInfo(gameController.GetLocalPlayerInfo());
    }

    IEnumerator WaitAndLoad(){
        List<ItemInfo> items = gameController.connector.localBags[(int)eBageType.MainBag];
        while(items == null){
            yield return new WaitForSeconds(0.2f);
        }
        StartCoroutine(LoadInfoSlot(items));
        StartCoroutine(LoadBagSlot(items));
    }

    IEnumerator LoadInfoSlot(List<ItemInfo> items){
        yield return null;
        foreach (ItemInfo item in items){
            if (item.Place > 30)
                continue;
            if (GameObject.Find("InfoSlot ("+item.Place.ToString()+")") == null){
                continue;
            }
            BagSlotController slot = GameObject.Find("InfoSlot ("+item.Place.ToString()+")").GetComponent<BagSlotController>();
            if (slot == null)
                continue;
            string equipType = getEquipType(item.Pic);
            Debug.Log(sexEquipString+equipType+"/"+item.Pic+"/icon_1");
            slot.LoadDisplay(sexEquipString+equipType+"/"+item.Pic+"/icon_1");
        }
    }
    string getEquipType(string equipName){
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
    IEnumerator LoadBagSlot(List<ItemInfo> items){
        yield return null;
        foreach (ItemInfo item in items){
            if (item.Place < 31)
                continue;
            GameObject slotObj = GameObject.Find("BagSlot ("+(item.Place-31).ToString()+")");
            if (slotObj == null){
                continue;
            }
            BagSlotController slot = slotObj.GetComponent<BagSlotController>();
            if (slot == null)
                continue;
            string equipType = getEquipType(item.Pic);
            Debug.Log(sexEquipString+equipType+"/"+item.Pic+"/icon_1");
            slot.LoadDisplay(sexEquipString+equipType+"/"+item.Pic+"/icon_1");
        }
    }
    
    
    public virtual void Close(){
        gameController.uiController.UpdateMainPlayerPreview(gameController.GetLocalPlayerInfo());
        base.Close();
    }
}
