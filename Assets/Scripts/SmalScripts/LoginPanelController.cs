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
        UpdateServerList();
    }

    private void UpdateServerList(){
        string requestUriString = ConfigMgr.ServerListUrl;
        string rawText = "";        
        using (WebResponse response = WebRequest.Create(requestUriString).GetResponse())
        {
            using (StreamReader reader = new StreamReader(response.GetResponseStream()))
            {
                rawText = reader.ReadToEnd();
            }
        }
		
        XmlDocument document = new XmlDocument();
        document.LoadXml(rawText);
        XmlNodeList ele = document.GetElementsByTagName("Item");
        hostIP = new List<string>();
        hostName = new List<string>();
        for (int i = 0; i < ele.Count; i++)
        {
            hostIP.Add(ele[i].Attributes["IP"].Value);
            hostName.Add(ele[i].Attributes["Name"].Value);
        }
        serverDropdown.ClearOptions();
        serverDropdown.AddOptions(hostName);
        loginHost = hostIP[0];
    }
    private float totalTime;
    private void FixedUpdate() {
        totalTime += Time.fixedDeltaTime;
        if (totalTime > 0.5f){
            UpdateServerList();
            totalTime = 0;
        }
    }
}
