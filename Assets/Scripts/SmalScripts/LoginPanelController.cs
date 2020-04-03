using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.IO;
using System.Net;
using UnityEngine;
using UnityEngine.UI;
using ConnectorSpace;

public class LoginPanelController : BaseObjectController
{
    public InputField loginId;
    public InputField loginPass;
    public Dropdown serverDropdown;
    public string loginHost;
    public GameObject  changeHostUrlPanel;
    // Start is called before the first frame update
    List<string> hostIP;
    List<string> hostName;
    public void SelectServerFromDropdown(Dropdown change){
        loginHost = hostIP[change.value];
    }

    void Start()
    {
        if (serverDropdown == null){
            loginId = this.FindChildObject("Login_Id").GetComponent<InputField>();
            loginPass = this.FindChildObject("Login_Password").GetComponent<InputField>();
            serverDropdown = this.FindChildObject("ServerDropdown").GetComponent<Dropdown>();
        }
        ConfigMgr.UpdateHostAddress("127.0.0.1");
        UpdateServerList();
    }

    public void UpdateServerList(){
        StartCoroutine(ExecUpdateList());
    }

    public Text attemptDisplay;
    int attemptCount;
    IEnumerator ExecUpdateList(){
        string requestUriString = ConfigMgr.ServerListUrl;
        string rawText = "";
        try
        {
            WebResponse response = WebRequest.Create(requestUriString).GetResponse();
            StreamReader reader = new StreamReader(response.GetResponseStream());
            rawText = reader.ReadToEnd();
        }
        catch (System.Exception)
        {
        }
        attemptCount++;
        attemptDisplay.text = attemptCount.ToString();
        if (string.IsNullOrEmpty(rawText)){
            PlayerPrefs.DeleteAll();
            changeHostUrlPanel.SetActive(true);
        }
        Debug.Log(rawText);
        XmlDocument document = new XmlDocument();
        document.LoadXml(rawText);
        XmlNodeList ele = document.GetElementsByTagName("Item");
        hostIP = new List<string>();
        hostName = new List<string>();
        if (ele.Count < 1){
            changeHostUrlPanel.SetActive(true);
        }else{
            for (int i = 0; i < ele.Count; i++)
            {
                if (ele[i].Attributes["IP"].Value != "127.0.0.1"){
                    hostIP.Add(ele[i].Attributes["IP"].Value);
                }else{
                    hostIP.Add(ConfigMgr.ServerIp);
                }
                hostName.Add(ele[i].Attributes["Name"].Value);
                yield return null;
            }
            serverDropdown.ClearOptions();
            serverDropdown.AddOptions(hostName);
            if (hostIP.Count > 0)
                loginHost = hostIP[0];
        }
    }


    // private float totalTime;
    // private void FixedUpdate() {
    //     if (changeHostUrlPanel.activeInHierarchy)
    //         return;
    //     totalTime += Time.fixedDeltaTime;
    //     if (totalTime > 15f){
    //         UpdateServerList();
    //         totalTime = 0;
    //     }
    // }
}
