using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DelayedSelfDestroy : MonoBehaviour
{
    public bool isStarted;
	public float timer = 0f;
	public float delayedSeconds = 1.5f;
    // Start is called before the first frame update
    protected void OnEnable()
    {
        isStarted = true;
    }

    public void SetStarted(bool value){
        isStarted = value;
    }

    private void Update() {
        if (isStarted)
            timer += Time.deltaTime;
        if (timer >= delayedSeconds){
            Destroy(this.gameObject);
        }
    }
}
