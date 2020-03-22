using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SummaryPanelController : BaseObjectController
{
    protected static SummaryPanelController _instance;
    // Start is called before the first frame update
    public GameObject losePanel;
    public GameObject winPanel;

    public void DisplaySummary(MatchSummary ms){
        this.gameObject.SetActive(true);
        if (ms.isWin){
            winPanel.SetActive(true);
            losePanel.SetActive(false);
        }else{
            winPanel.SetActive(false);
            losePanel.SetActive(true);
        }
    }

    void Start()
    {
        if (_instance == null){
            _instance = this;
        } else{
            return;
        }
        this.gameObject.SetActive(false);
    }


    public static SummaryPanelController GetInstance(){
        return _instance;
    }

}
