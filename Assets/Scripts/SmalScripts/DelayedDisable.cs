using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DelayedDisable : MonoBehaviour
{
    public bool isStarted;
	public float timer = 0f;
	public float delayedSeconds = 1.5f;
    // Start is called before the first frame update
    void OnEnable()
    {
        isStarted = true;
    }

    private void Update() {
        if (isStarted)
            timer += Time.deltaTime;
        if (timer >= delayedSeconds){
            timer = 0;
            isStarted = false;
            this.gameObject.SetActive(false);
        }
    }
}
