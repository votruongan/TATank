using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DigController : TimedController
{
	[Header("FOR DEBUG")]
	public bool isDiggable;
	public ForegroundController foreController;
	public List<Vector2> digVertices;
	public float dividedAngle;
	[Header("SETTINGS")]
	public int verticesDivision = 3;

	public void CheckIfDiggable(){
		isDiggable = foreController.IsForegroundLoaded();
	}

	protected override void Execute(){
		// place hole in front of foreground Sprite
		if (!(isDiggable)){
			return;
		}
		Instantiate(prefab,target,Quaternion.identity);
		// reCalculate foreground Collider
		Vector2 center = new Vector2(target.x, target.y);
		PrepareDigVertices(center);
		for (int i = 0; i < foreController.foreCollider.pathCount; i++){
			Vector2[] path = foreController.foreCollider.GetPath(i);
			// see if prepared dig vertices collide with path
			// add intersect vertices to path
		}
		//foreController.RefreshCollider(foreRefresher.gameObject);
	}

	// check if a point is in a shape
	bool isInShape(Vector2 point,Vector2[] shape){
		bool upY = false, downY = false,
			 leftX = false, rightX = false;
		return (upY && downY && leftX && rightX);
	}
	//Subtract the shape by the current digVertices
	Vector2[] SubtractShape(Vector2[] path){
		Vector2[] res = path;
		//loop through points
		foreach(Vector2 vert in digVertices){
			//check if point is in the shape
			//add the point to the shape
		}
		return res;
	}

	//Set vertices list along the bound of hole
	void PrepareDigVertices(Vector2 pos){
		float Radius = transform.localScale.x / 10;
		// // Horizontal through center
		// digVertices.Add(new Vector2(pos.x, pos.y + Radius));
		// digVertices.Add(new Vector2(pos.x, pos.y - Radius));
		// // Vertical through center
		// digVertices.Add(new Vector2(pos.x + Radius, pos.y));
		// digVertices.Add(new Vector2(pos.x - Radius, pos.y));
		for (int i = 0; i <verticesDivision; i++){
			float dx = Mathf.Sin(i*dividedAngle)*Radius;
			float dy = Mathf.Cos(i*dividedAngle)*Radius;
			// First corner in the graph
			digVertices.Add(new Vector2(pos.x + dx,
										pos.y + dy));
			// Second corner in the graph
			digVertices.Add(new Vector2(pos.x + dx,
										pos.y - dy));
			// Third corner in the graph
			digVertices.Add(new Vector2(pos.x - dx,
										pos.y - dy));
			// Fourth corner in the graph
			digVertices.Add(new Vector2(pos.x - dx,
										pos.y + dy));
		}
	}


    // Start is called before the first frame update
    void Start()
    {
    	dividedAngle = 90/verticesDivision;
        foreController = GameObject.Find("Foreground").GetComponent<ForegroundController>();
    }
}
