using System;
// using UnityEngine;
using System.Xml;
using System.Collections;
[Serializable]
public class ItemTemplateInfo : BaseInfoWrapper
{
    public int TemplateID;

    public string Name;

    public int CategoryID;

    public string Description;

    public int Attack;

    public int Defence;

    public int Luck;

    public int Agility;

    public int Level;

    public string Pic;

    public string AddTime;

    public int Quality;

    public int MaxCount;

    public string Data;

    public int Property1;

    public int Property2;

    public int Property3;

    public int Property4;

    public int Property5;

    public int Property6;

    public int Property7;

    public int Property8;

    public int NeedSex;

    public int NeedLevel;

    public bool CanDrop;

    public bool CanDelete;

    public bool CanEquip;

    public bool CanUse;

    public string Script;

    public string Colors;

    public bool CanStrengthen;

    public bool CanCompose;

    public int BindType;

    public int FusionType;

    public int FusionRate;

    public int FusionNeedRate;

    public int RefineryType;

    public string Hole;

    public int RefineryLevel;

    public eBagType BagType
    {
        get
        {
            switch (CategoryID)
            {
                case 10:
                case 11:
                case 12:
                    return eBagType.PropBag;
                default:
                    return eBagType.MainBag;
            }
        }
    }

    void SetInfo(XmlAttributeCollection attColl){
        foreach(XmlAttribute att in attColl){
            var field = this.GetType().GetField(att.Name);
            if (field.FieldType == typeof(int)){
                field.SetValue(this,Int32.Parse(att.Value));
            } else
            {
                if (field.FieldType == typeof(bool)){
                    field.SetValue(this,(att.Value == "true")?(true):(false));
                } else
                {
                    field.SetValue(this,att.Value);                    
                }
            }
            // yield return null;
        }    
    }
    public ItemTemplateInfo(XmlAttributeCollection attColl){
        SetInfo(attColl);
    }
}