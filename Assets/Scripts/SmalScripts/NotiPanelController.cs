using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class NotiPanelController : MonoBehaviour
{

    public Text notiText;

    public void ShowText(string txt){
        notiText.text = txt;
        this.gameObject.SetActive(true);
        Close(2000);
    }

    public void ShowTextAndClose(string txt, int ms = 1000){
        notiText.text = txt;
        this.gameObject.SetActive(true);
        Close(ms);
    }

    public void Close(int ms = 0){
        if (ms == 0){
            this.gameObject.SetActive(false);
        }
        else
        {
            StartCoroutine(WaitAndClose(ms/1000));
        }
    }

    IEnumerator WaitAndClose(float secs){
        yield return new WaitForSeconds(secs);
        Close(0);
    }
}
