using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CountUpController : MonoBehaviour
{
    public bool isOnCount;
    public Text displayText;
    int min, sec;
    Coroutine countRoutine;
    // Start is called before the first frame update
    void Start()
    {
        if(displayText == null)
            displayText = gameObject.GetComponent<Text>();
    }

    void OnEnable(){
        displayText.text = "";
        min = sec = 0;
        isOnCount = true;
        StartCoroutine(StartCount());
    }

    IEnumerator StartCount(){
        while (isOnCount)
        {
            yield return new WaitForSeconds(1.0f);
            displayText.text = "";
            sec += 1;
            if (sec > 59){
                min += 1;
                sec = 0;
            }
            if (min < 10)
                displayText.text = "0";
            displayText.text += min.ToString() + ":";
            if (sec < 10)
                displayText.text += "0";
            displayText.text += sec.ToString();

        }
    }

    void OnDisable(){
        isOnCount = false;
    }   

}
