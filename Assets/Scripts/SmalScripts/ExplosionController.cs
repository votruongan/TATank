using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExplosionController : TimedController	
{
	public GameController gameController;
	public static ExplosionController instance;
	void Start(){
		instance = this;
		gameController = GameObject.Find("GameController").GetComponent<GameController>();

	}
	
	public static ExplosionController GetInstance(){
		return instance;
	}

	protected override void Execute(){
		Debug.Log("Explode at: "+target.ToString());
		Instantiate(prefab,target,Quaternion.identity);
		gameController.soundManager.PlayEffect("explosion");
	}
}