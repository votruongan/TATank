using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ForegroundController : MonoBehaviour
{
	public Rect mapRect;
	public PolygonCollider2D foreCollider;
	public Vector3 bottomLeftPosition;
	[Header("FOR DEBUG")]
	public bool isUpdatedBLP;
	public Vector3 topLeftPosition;
	public GameObject deadLayer;
	public SpriteRenderer deadSprite;
	public PolygonCollider2D deadCollider;
	public SpriteRenderer foreSprite;

	public bool IsForegroundLoaded(){
		return (foreSprite.sprite != null);
	}


	//Get pixel position on Map from Unity unit
	public Vector2 WorldPositionToPixel (float x, float y, bool isInverseY){
		if (!(isUpdatedBLP)){
			UpdateBottomLeftPosition();
		}
		//subtract world position to offset and convert to pixel
		int px = (int)((float)((float)x - bottomLeftPosition.x) * 100);
		int py = (int)((float)((float)y - bottomLeftPosition.y) * 100);
		//inverseY
		if (isInverseY){
			py = (int)mapRect.height - py;
		}
		//return result
		return new Vector2(px,py);
	}

	//Get pixel position on Map from Unity unit
	public Vector2 WorldPositionToPixel (Vector3 pos, bool isInverseY){
		if (!(isUpdatedBLP)){
			UpdateBottomLeftPosition();
		}
		//subtract offset
		pos -= bottomLeftPosition;
		pos *= 100;
		//convert to pixel
		int x = (int) pos.x;
		int y = (int) pos.y;
		//inverse Y
		if (isInverseY){
			y = (int)mapRect.height - y;
		}
		//return result
		return new Vector2(x,y);
	}

	//Get Unity unit of pixel on Map
	public Vector3 PixelToWorldPosition (int x, int y, bool isInverseY){
		Vector2 pPos = new Vector2((float)x,(float)y);
    	return PixelToWorldPosition(pPos,isInverseY);
	}

	//Get Unity unit of pixel on Map
	public Vector3 PixelToWorldPosition (Vector2 pos, bool isInverseY){
		if (!(isUpdatedBLP)){
			UpdateBottomLeftPosition();
		}
		// if (pos.x > mapRect.x || pos.y > mapRect.y){
		// 	Debug.Log("requested point is out of Map range");
		// 	return new Vector3(-1f,-1f,-1f);
		// }	
		if (isInverseY){
			pos.y = mapRect.height - pos.y;
		}
		// 100 pixel = 1 Unity unit
		pos /= 100;
		Vector3 res = new Vector3 (bottomLeftPosition.x + pos.x, 
							bottomLeftPosition.y + pos.y,
							 0f);
		// Debug.Log(" Pixel " + pos.ToString() + " -> world: " + res.ToString()+ 
			// "BLP: "+ bottomLeftPosition.ToString());
		return res;
	}

	void UpdateMapRect(){
		//wait for fully loaded
		if (foreSprite.sprite == null && deadSprite.sprite == null){
			System.Threading.Thread.Sleep(200);
		}
		if (foreSprite.sprite != null){
			mapRect = foreSprite.sprite.rect;
			Debug.Log("Updated BottomLeftPosition ");
			return;				
		}
		mapRect = deadSprite.sprite.rect;
	}

	void Start(){
		isUpdatedBLP = false;
		deadLayer = this.transform.GetChild(0).gameObject;
		deadSprite = deadLayer.GetComponent<SpriteRenderer>();
		deadCollider = deadLayer.GetComponent<PolygonCollider2D>();
		foreSprite = this.GetComponent<SpriteRenderer>();
	}

	void UpdateBottomLeftPosition(){
		UpdateMapRect();
		bottomLeftPosition = transform.position;
		bottomLeftPosition -= new Vector3 (mapRect.width/200, mapRect.height/200, 0f);
		isUpdatedBLP = true;
	}

	public void LoadSprite(Sprite sp){
		foreSprite.sprite = sp;
		RefreshCollider(this.gameObject);
	}

	public void LoadMap(int mapId){
		string path = "map/"+mapId.ToString()+"/";
		this.LoadMap(path);
	}

	public void LoadMap(string mapPath){
		this.isUpdatedBLP = false;
		this.LoadSprite(Resources.Load<Sprite>(mapPath+"fore"));
		deadSprite.sprite = Resources.Load<Sprite>(mapPath+"dead");
		RefreshCollider(this.gameObject);
		RefreshCollider(deadLayer);
		UpdateMapRect();
	}


	public void RefreshCollider(GameObject gObject){
		PolygonCollider2D col = gObject.GetComponent<PolygonCollider2D>();
		if (col != null){
			Destroy(gObject.GetComponent<PolygonCollider2D>());
		}
		//No sprite to draw on screen -> don't add polygon
		if (gObject.GetComponent<SpriteRenderer>().sprite == null){
			return;
		}
		foreCollider = gObject.AddComponent<PolygonCollider2D>();
	}
}
