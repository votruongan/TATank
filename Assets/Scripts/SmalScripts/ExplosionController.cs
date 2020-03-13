using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExplosionController : TimedController	
{
	public GameController gameController;
	void Start(){
		gameController = GameObject.Find("GameController").GetComponent<GameController>();
	}
	protected override void Execute(){
		Debug.Log("Explode at: "+target.ToString());
		Instantiate(prefab,target,Quaternion.identity);
		gameController.soundManager.PlayEffect("explosion");
	}
}