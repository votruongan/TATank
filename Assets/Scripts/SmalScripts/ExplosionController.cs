using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExplosionController : TimedController	
{
	protected override void Execute(){
		Debug.Log("Explode at: "+target.ToString());
		Instantiate(prefab,target,Quaternion.identity);
	}
}