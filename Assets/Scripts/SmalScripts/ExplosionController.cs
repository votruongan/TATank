using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExplosionController : TimedController	
{
	public static GameController gameController;
	public static ExplosionController instance;
	public List<Vector3> explodeQueue;
	
	void Start(){
		instance = this;
		gameController = GameObject.Find("GameController").GetComponent<GameController>();
	}
	
	public void ResetQueue(){
		explodeQueue = new List<Vector3>();
	}

	public void WillExplode(Vector3 target){
		explodeQueue.Add(target);
	}

	public void ExplodeNearest(Vector3 target){
		float min = 100f,dis;
		int pos = 0, count = explodeQueue.Count;
		if (count > 1)
			for (int i = 0; i < explodeQueue.Count; i++)
			{
				dis = Vector3.Distance(target, explodeQueue[i]);
				if (dis < min){
					min = dis; pos = i;
				}
			}
		Explode(explodeQueue[pos]);
		explodeQueue.RemoveAt(pos);
	}
	public void Explode(Vector3 targ){
		Debug.Log("Explode at: "+targ.ToString());
		Instantiate(prefab,targ,Quaternion.identity);
		gameController.soundManager.PlayEffect("explosion");
	}

	public static ExplosionController GetInstance(){
		return instance;
	}

	protected override void Execute(){
		Explode(target);
	}
}