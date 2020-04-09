using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SummaryPanelController : BaseObjectController
{
    public GameObject losePanel;
    public GameObject winPanel;

    void Start(){
    }
    IEnumerator CheckAndDisplay(MatchSummary ms){
        GameObject gO = GameObject.Find("Bullet (Clone)");
        while(gO != null){
            gO = GameObject.Find("Bullet_Hammer(Clone)");
            yield return new WaitForSeconds(0.3f);
        }
        // Debug.Log("DisplaySummary Called!");
        this.gameObject.SetActive(true);
        Debug.Log(ms.ToString());
        if (ms.isWin){
            winPanel.SetActive(true);
            losePanel.SetActive(false);
            SoundManager.GetInstance().PlayEffect("choose");
            SoundManager.GetInstance().PlayEffect("happy");
        }else{
            winPanel.SetActive(false);
            losePanel.SetActive(true);
            SoundManager.GetInstance().PlayEffect("sad");
        }
        // Debug.Log("DisplaySummary Done!");
    }

    public void DisplaySummary(MatchSummary ms){
        this.gameObject.SetActive(true);
        StartCoroutine(CheckAndDisplay(ms));
    }
}
