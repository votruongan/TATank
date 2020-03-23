using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SummaryPanelController : BaseObjectController
{
    public GameObject losePanel;
    public GameObject winPanel;

    public void DisplaySummary(MatchSummary ms){
        Debug.Log("DisplaySummary Called!");
        this.gameObject.SetActive(true);
        Debug.Log(ms.ToString());
        if (ms.isWin){
            winPanel.SetActive(true);
            losePanel.SetActive(false);
        }else{
            winPanel.SetActive(false);
            losePanel.SetActive(true);
        }
        Debug.Log("DisplaySummary Done!");
    }
}
