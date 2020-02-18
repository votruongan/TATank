using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DamagePanelController : DelayedSelfDestroy {
    [Header("FOR DEBUGGING")]
    public int damage;
    [Header("SETTINGS")]
    public TextMesh textDamage;
    public Vector3 offsetFromOrigin;
    public Sprite[] numberSprites;
    public float numberSeperation = 0.16f;
    public List<GameObject> toDestroy;

    void Start(){
        // if (textDamage == null)
            // textDamage = this.transform.GetChild(transform.childCount-1).gameObject.GetComponent<TextMesh>();
        // textDamage.text = damage.ToString();
        offsetFromOrigin = new Vector3 (0f,0.5f,0f);
    }

    public void SetDamage(int dam){
        damage = dam;
        // textDamage.text = damage.ToString();
        string tmp = dam.ToString();
        float spawnPos_X = - (tmp.Length-1) * numberSeperation / 2;
        GameObject gObj = null;
        for (int i = 0; i < tmp.Length; i++)
        {
            // gObj = Instantiate(numberSprites[(int)Char.GetNumericValue(tmp[i])],new Vector3(spawnPos_X,0.0f, 0.0f), 
            // Quaternion.identity);
            gObj = new GameObject("DamChar");
            gObj.transform.localScale = new Vector3(0.3f,0.3f,0.3f);
            gObj.transform.position = new Vector3(transform.position.x + spawnPos_X, transform.position.y, 0f);
            toDestroy.Add(gObj);
            SpriteRenderer sr = gObj.AddComponent<SpriteRenderer>() as SpriteRenderer;
            sr.sprite = numberSprites[(int)Char.GetNumericValue(tmp[i])];
            spawnPos_X += numberSeperation;
        }
    }

    void FixedUpdate(){
        this.transform.Translate(0f,0.01f,0f);
        foreach(GameObject gObj in toDestroy){
            gObj.transform.Translate(0f,0.01f,0f);
        }
    }

    private void OnDestroy() {
        foreach (GameObject gObj in toDestroy)
        {
            Destroy(gObj);
        }
    }
}