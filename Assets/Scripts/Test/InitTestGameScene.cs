using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class InitTestGameScene : MonoBehaviour
{
    public GameController gC;
    void Start(){
        gC = GameObject.Find("GameController").GetComponent<GameController>();
        // SwitchScene();
        StartCoroutine(WaitLogin());
        StartCoroutine(WaitStartMatch());
    }
    void SwitchScene(string from, string to){
        Scene temp = SceneManager.CreateScene("temp");
        StartCoroutine(ExecSwitchScene(from,to));
    }
    IEnumerator ExecSwitchScene(string from, string to){
        yield return null;
        AsyncOperation lao = SceneManager.LoadSceneAsync(to,LoadSceneMode.Additive);
        lao.allowSceneActivation = false;
        while (!lao.isDone){
            if (lao.progress >= 0.9f)
            {
                Debug.Log("load done");
                break;
            }
        }
        AsyncOperation uao = SceneManager.UnloadSceneAsync(from);
        lao.allowSceneActivation = true;
        SceneManager.UnloadSceneAsync("temp");
    }
    
    IEnumerator WaitLogin(){
        yield return new WaitForSeconds(0.2f);
        gC.uiController.SetLoadingScreen(false);
        if (gC.connector != null){
            // gC.ConnectHost("127.0.0.1");
        }
    }
    
    IEnumerator WaitStartMatch(){
        // gC.StartMatch();
        GameObject gO = null;
        while(gO == null){
            gO = GameObject.Find("MainPlayer");
            yield return new WaitForSeconds(0.04f);   
        }
        gC.mainPlayerController = gO.GetComponent<MainPlayerController>();
        gC.mainCamController.LockTo(gC.mainPlayerController.gameObject.transform);
        gC.mainPlayerController.isOnTurn = true;
        PlayerInfo pInf = new PlayerInfo();
        pInf.blood = 12345;
        gC.mainPlayerController.SetPlayerInfo(pInf);
		// gC.explosionController.DelayedExecute(2000,gC.mainPlayerController.transform.position);
        // gC.mainPlayerController.Damaged(2000,1647,false,12345-1647);
        // gC.PlayerFire(0,100,100,100);
    }
}
