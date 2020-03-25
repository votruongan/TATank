using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimedController : MonoBehaviour	
{
	public GameObject prefab;
	protected Vector3 target;
    protected IEnumerator coroutine;
    // mili seconds
    public void DelayedExecute(int time, Vector3 target){
        // Debug.Log("DELAY Execute called");
        this.target = target;
        coroutine = WaitAndExecute((float)time/1000);
        StartCoroutine(coroutine);
    }
    public void CancelDelayedExecution(){
        Debug.Log("CancelExecution Called");
        StopCoroutine(coroutine);
    }
    protected virtual void Execute(){
        // Debug.Log("VIRTUAL Execute");

    }

    IEnumerator WaitAndExecute(float time){
        // Debug.Log("WAIT AND Execute");
        yield return new WaitForSeconds(time);
        this.Execute();
    }
}