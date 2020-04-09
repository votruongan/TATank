using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using ConnectorSpace;

public class ForegroundController : MonoBehaviour
{	
	public bool isTest;
	public Rect mapRect;
	public PolygonCollider2D foreCollider;
	public Vector3 bottomLeftPosition;
	public Vector3 topRightPosition;
	[Header("FOR DEBUG")]
	public bool isUpdatedBLP;
	public GameObject deadLayer;
	public SpriteRenderer backgroundSprite;
	public SpriteRenderer deadSprite;
	public PolygonCollider2D deadCollider;
	
	int inLoading;
	public SpriteRenderer foreSprite;
	public GameController gameController;

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
			return;
		}
		if (deadSprite.sprite!= null)
			mapRect = deadSprite.sprite.rect;
	}

	void Start(){
		isUpdatedBLP = false;
		gameController = GameObject.Find("GameController").GetComponent<GameController>();
		deadLayer = this.transform.GetChild(0).gameObject;
		deadSprite = deadLayer.GetComponent<SpriteRenderer>();
		deadCollider = deadLayer.GetComponent<PolygonCollider2D>();
		foreSprite = this.GetComponent<SpriteRenderer>();
		if (backgroundSprite == null)
			backgroundSprite = GameObject.Find("Background").GetComponent<SpriteRenderer>();
		// if (isTest)
		// 	LoadMapOnline(1028);
	}

	void UpdateBottomLeftPosition(){
		UpdateMapRect();
		bottomLeftPosition = transform.position;
		bottomLeftPosition -= new Vector3 (mapRect.width/200, mapRect.height/200, 0f);
		topRightPosition = transform.position;
		topRightPosition += new Vector3 (mapRect.width/200, mapRect.height/200, 0f); 
		isUpdatedBLP = true;
		Debug.Log("Updated BottomLeftPosition " + bottomLeftPosition);
	}

	// public void LoadMap(int mapId){
	// 	string path = "map/"+mapId.ToString()+"/";
	// 	this.LoadMap(path);
	// }
	public void LoadMapOnline(int id){
		string mapId = id.ToString();
		StartCoroutine(GetMapTexture(mapId,"fore.png",foreSprite));
		StartCoroutine(GetMapTexture(mapId,"dead.png",deadSprite));
		StartCoroutine(GetMapTexture(mapId,"back.jpg",backgroundSprite,true));
	}

	// public void LoadMap(string mapPath){
	// 	this.isUpdatedBLP = false;
	// 	this.LoadSprite(Resources.Load<Sprite>(mapPath+"fore"));
	// 	deadSprite.sprite = Resources.Load<Sprite>(mapPath+"dead");
	// 	ForceUpdate();
	// }

	Sprite makeSimpleSrite(Texture tex){
		return Sprite.Create(tex as Texture2D,new Rect(0.0f, 0.0f, tex.width, tex.height),new Vector2(0.5f,0.5f),100.0f);
	}

    IEnumerator GetMapTexture(string mapId, string name, SpriteRenderer applyTo, bool forceUpdate=false) {
		if (applyTo == null){
			yield break;
		}
		inLoading++;
		string foreDir = ConfigMgr.ResourcesUrl + "/image/map/" + mapId +"/"+ name + "?lv=14&";
		// Debug.Log(foreDir);
        UnityWebRequest www = UnityWebRequestTexture.GetTexture(foreDir);
        yield return www.SendWebRequest();
		Texture tex;
        if(www.isNetworkError || www.isHttpError) {
            Debug.Log("get "+name+" error: " + www.error);
        }
        else {
            tex = ((DownloadHandlerTexture)www.downloadHandler).texture;
			while(foreSprite == null){
				yield return null;
			}
			applyTo.sprite = makeSimpleSrite(tex);
        }
		inLoading--;
		if (forceUpdate){
			while(inLoading > 0){
				yield return new WaitForSeconds(0.04f);
			}
			ForceUpdate();
			gameController.LoadComplete();
		}
    }
	public void ForceUpdate(){
		RefreshCollider(this.gameObject);
		RefreshCollider(deadLayer);
		UpdateBottomLeftPosition();
	}
	public void RefreshCollider(GameObject gObject){
		PolygonCollider2D col = gObject.GetComponent<PolygonCollider2D>();
		if (col != null){
			Destroy(gObject.GetComponent<PolygonCollider2D>());
		}
		//No sprite to draw on screen -> don't add polygon
		if (gObject.GetComponent<SpriteRenderer>().sprite == null){
			Debug.Log("[RefreshCollider] go name " + gObject.name + " null sprite");
			return;
		}
		foreCollider = gObject.AddComponent<PolygonCollider2D>();
	}
}
