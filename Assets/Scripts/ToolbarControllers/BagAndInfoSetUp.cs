using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BagAndInfoSetUp : BaseToolbarController
{
    public GameController gameController;
    public PlayerPreviewLoader mpl;
    // Start is called before the first frame update
    protected override void Start()
    {
        gameController = GameObject.Find("GameController").GetComponent<GameController>();
        base.Start();
        mpl = this.FindChildObject("MainPlayerPreview").GetComponent<PlayerPreviewLoader>();
        mpl.LoadFromInfo(gameController.GetLocalPlayerInfo());
    }
    public virtual void Close(){
        base.Close();
        gameController.uiController.UpdateMainPlayerPreview(gameController.GetLocalPlayerInfo());
    }
}
