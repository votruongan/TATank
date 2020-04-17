using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DanderEffectScreen : DelayedDisable
{
    public RawImage fadingDisplay;
    List<PlayerInfo> infos = new List<PlayerInfo>();
    public PlayerPreviewLoader[] RedLoader; //0 is left, 1 is right
    public PlayerPreviewLoader[] BlueLoader;
    // public GameController gC;

    public void PrepareLoader(int team, PlayerInfo inf, bool exec=false){
        this.gameObject.SetActive(true);
        Debug.Log("PrepareLoaderPrepareLoaderPrepareLoaderPrepareLoaderPrepareLoader");
        // Debug.Log(inf);
        if (exec){
            if (team == 2)
                StartCoroutine(ExecPrepareBlue(inf));
            else
                StartCoroutine(ExecPrepareRed(inf));
            return;
        }
        if (team == 2){
            ResetBlue();
            // blueInfo = inf.Clone();
            infos.Add(inf.Clone());
            // gC.LoadComplete();
        }
        else{
            ResetRed();
            // redInfo = inf.Clone();
            infos.Add(inf.Clone());
            // gC.LoadComplete();
        }
    }

    void ResetRed(){
        RedLoader[0].gameObject.SetActive(false);
        RedLoader[1].gameObject.SetActive(false);
    }

    void ResetBlue(){
        BlueLoader[0].gameObject.SetActive(false);
        BlueLoader[1].gameObject.SetActive(false);
    }

    IEnumerator ExecPrepareRed(PlayerInfo inf){
        RedLoader[0].LoadFromInfo(inf);
        RedLoader[0].gameObject.SetActive(false);
        yield return null;
        RedLoader[1].LoadFromInfo(inf);
        RedLoader[1].gameObject.SetActive(false);
    }

    IEnumerator ExecPrepareBlue(PlayerInfo inf){
        BlueLoader[0].LoadFromInfo(inf);
        BlueLoader[0].gameObject.SetActive(false);
        yield return null;
        BlueLoader[1].LoadFromInfo(inf);
        BlueLoader[1].gameObject.SetActive(false);
    }

    public void CallDanderScreen(bool isHeadingRight, int team){
        Debug.Log("PLayDanderScreen = [DanderControoler] = team");
        int ind = isHeadingRight ? 1 : 0;
        ResetRed();
        this.gameObject.SetActive(true);
        switch (team)
        {
            case 1:
                RedLoader[ind].gameObject.SetActive(true);
                // RedLoader[ind].LoadFromInfo(redInfo);
                break;
            default:
                BlueLoader[ind].gameObject.SetActive(true);
                // BlueLoader[ind].LoadFromInfo(blueInfo);
                break;
        }
    }

    public void CallDanderScreen(bool isHeadingRight, PlayerInfo inf){
        Debug.Log("PLayDanderScreen = [DanderControoler] = info");
        // foreach (PlayerInfo p in cache)
        // {
            
        // }
        ResetBlue();
        int ind = isHeadingRight ? 1 : 0;
        this.gameObject.SetActive(true);
        foreach(PlayerInfo p in infos){
            if (p.id != inf.id)
                continue;
            BlueLoader[ind].gameObject.SetActive(true);
            BlueLoader[ind].LoadFromInfo(p);
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        Color fixedColor = fadingDisplay.color;
        fixedColor.a = 1;
        fadingDisplay.color = fixedColor;
        fadingDisplay.CrossFadeAlpha(0f, 0f, true);

        if (fadingDisplay == null || RedLoader == null || BlueLoader == null ){
            
        }
    }

    private void FixedUpdate() {
        if (!isStarted)
            return;
        if ((timer - 1.0f) > 0f && (timer - 1.0f) < 0.03f)
            BeforeDisable();
    }

    void BeforeDisable() {
        ToggleBlackBack(0f);
    }

    void OnEnable() {
        base.OnEnable(); 
        ToggleBlackBack(1f);
    }

    void ToggleBlackBack(float toAlpha){
        // Debug.Log("ToggleBlackBack "+ toAlpha);
        fadingDisplay.CrossFadeAlpha(toAlpha, 1.0f, false);
    }
}
