using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CommonUIController : UIController
{
    [Header("SETTINGS")]
    // public GameObject yourTurnPanel;
    public GameObject loginPanel;
    public GameObject mainPanel;
    public GameObject loadingScreen;
    // public GameObject fightUI;
    public GameObject countUpUI;
    public NotiPanelController notiPanel;
    public BagAndInfoController bagAndInfoController;
    public LoginPanelController loginPanelController;
    // public InputField loginIdText;
    // public InputField loginPassText;
    // public Text loginHostText;
    public PlayerPreviewLoader mainPlayerPreview;
    // [Header("FOR DEBUG")]
    // public MainPlayerController mainPlayerController;
    // public bool isTakePower;
    // public int power;   
    // public int angle;
    // public Dictionary<string, int> propDict;
    

    public void LogOut(){
        PlayerPrefs.DeleteAll();
        GameObject.Find("SettingsPanel").SetActive(false);
        loginPanel.SetActive(true);
        bagAndInfoController.ResetDisplay();
    }

    void Start()
    {
        if(gameController == null)
            gameController = GameObject.Find("GameController").GetComponent<GameController>();
            
        if (loginPanel == null)
            loginPanel = GameObject.Find("LoginPanel");
        if (mainPanel == null)
            mainPanel = GameObject.Find("MainPanel");
        if (countUpUI == null)
            countUpUI = GameObject.Find("CountUpUI");
        if(loadingScreen == null)
            loadingScreen = GameObject.Find("LoadingScreen");
        if (mainPlayerPreview == null)
            mainPlayerPreview = GameObject.Find("MainPlayerPreview").GetComponent<PlayerPreviewLoader>();
        StartCoroutine(ShowLoginScreen());
        // if(yourTurnPanel == null)
        //     yourTurnPanel = GameObject.Find("YourTurnPanel");


        // if(healthText == null)
        //     healthText = GameObject.Find("HealthText").GetComponent<Text>();
        // if (propDict == null){
        //     propDict = new Dictionary<string, int>()
        //     {    
        //         {"X2",10001},{"X1",10002},{"S3",10003},{"P50",10004},
        //         {"P40",10005},{"P30",10006},{"P20",10007},{"P10",10008}
        //     };
        // }
    }
    IEnumerator ShowLoginScreen(){
        yield return null;
        Debug.Log("Waiting to show login screen");
        while (gameController.connector.gameController == null){

        }
        if (!(gameController.connector.isLogedIn)){
            string id = PlayerPrefs.GetString("Id","");
            string pass = PlayerPrefs.GetString("28102000","");
            string host = PlayerPrefs.GetString("Host","");
            loginPanel.SetActive(true);
            mainPanel.SetActive(false);
            countUpUI.SetActive(false);
            // loginIdText = GameObject.Find("Login_Id").GetComponent<InputField>();
            // loginPassText = GameObject.Find("Login_Password").GetComponent<InputField>();
            // loginHostText = GameObject.Find("Login_Server_Label").GetComponent<Text>();
            if (!(string.IsNullOrEmpty(id) || string.IsNullOrEmpty(pass) || string.IsNullOrEmpty(host))){
                loginPanelController.loginId.text = id;
                loginPanelController.loginPass.text = pass;
                loginPanelController.loginHost = host;
                LoginToHost();
            }  
        }
        else{
            loginPanel.SetActive(false);
            mainPanel.SetActive(true);
            countUpUI.SetActive(false);
            UpdateMainPlayerPreview(gameController.connector.localPlayerInfo);
        }
        loadingScreen.SetActive(false);
    }
    public void SetLoadingScreen(bool isActive){
        loadingScreen.SetActive(isActive);
        countUpUI.SetActive(!isActive);
    }
    public void ConnectHost(){
		gameController.ConnectHost(GameObject.Find("HsIPA_InputField_Text").GetComponent<Text>().text);
        GameObject.Find("HostIpPanel").SetActive(false);
    }
    [Header("Set Match UI")]
    public GameObject matchButton;
    public GameObject readyButton;
    public GameObject cancelButton;

    public void OpenMatch(){
        matchButton.SetActive(false);
        countUpUI.SetActive(true);
        gameController.StartMatch();
    }
    public void SoloPVPMatch(){
        countUpUI.SetActive(true);
        gameController.StartMatch();
    }
    
    public void CancelMatch(){
        matchButton.SetActive(true);
        countUpUI.SetActive(false);
        gameController.StopMatch();
    }

    public void OpenRegister(){
        gameController.connector.OpenRegister();
    }

    public void LoginToHost(){
		string id = loginPanelController.loginId.text;
        if (string.IsNullOrEmpty(id)){
            Debug.Log("No Id entered");
            return;
        }
		string pass = loginPanelController.loginPass.text;
        if (string.IsNullOrEmpty(pass)){
            Debug.Log("No pass entered");
            return;
            
        }
		string host = loginPanelController.loginHost;
        if ((string.IsNullOrEmpty(host))){
            if (host == "Test")
                host = "127.0.0.1";
            Debug.Log("No host entered");     
            return;
        }
        ExecLogin(id, pass,host);
    }

    private void ExecLogin(string id, string pass, string host){
        notiPanel.ShowText("Đang xử lý ... ");
        if (gameController == null){
            gameController = GameObject.Find("GameController").GetComponent<GameController>();
        }
        bool res = gameController.ConnectHost(id, pass, host);
        if (res){
            //login succeed
            notiPanel.ShowTextAndClose("Đăng nhập thành công");
            loginPanel.SetActive(false);
            // GameObject.Find("FogCamera").SetActive(false);
            // fightUI.SetActive(false);
            mainPanel.SetActive(true);
            return;
        }
        notiPanel.ShowTextAndClose("Đăng nhập lỗi: "+gameController.GetClientConnector().lastErr.ToString());
    }


    public override void UpdateMainPlayerPreview(PlayerInfo pInfo){
        Debug.Log("Check at UIController");
        mainPlayerPreview.GetComponent<PlayerPreviewLoader>().LoadFromInfo(pInfo);
        // loginPanel.SetActive(false);
    }
}
