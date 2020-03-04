using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealthBarSprite : BaseObjectController {

	bool isInited = false;
	public Vector3 localScale;
	public SpriteRenderer spriteRenderer;
	public TextMesh playerName;
	public SpriteRenderer getTurnSprite;
	Transform parentTf;
	private void Update() {
		Vector3 vRot = parentTf.rotation.eulerAngles;
		if (vRot.y>1f || vRot.y < -1f){
			this.transform.localEulerAngles = new Vector3(0f,180f,0f);
		} else {
			this.transform.localEulerAngles = new Vector3(0f,0f,0f);
		}
	}
	// Use this for initialization
	void Start () {
		parentTf = this.transform.parent;
		if (!isInited){
			localScale = new Vector3(1f,2f,0f);
		}
		BaseChangeColor();
		isInited = true;
	}
	
	public void BaseChangeColor(){
		if (spriteRenderer == null){
			spriteRenderer = this.FindChildObject("ForeHealth").GetComponent<SpriteRenderer>();
		}
		if (playerName == null){
			playerName = this.FindChildObject("PlayerName").GetComponent<TextMesh>();
		}
		if (getTurnSprite == null){
			getTurnSprite = this.FindChildObject("GetTurnSprite").GetComponent<SpriteRenderer>();
			getTurnSprite.gameObject.SetActive(false);
		}
	}
	void ExecChangeColor(Color col){
		playerName.color = col;
		spriteRenderer.color = col;
		getTurnSprite.color = col;
	}
	public void ChangeToRed(){
		BaseChangeColor();
		ExecChangeColor(Color.red);
	}
	public void ChangeToBlue(){
		BaseChangeColor();
		ExecChangeColor(Color.blue);
	}

	public void ChangeColor(int R, int G, int B){
		BaseChangeColor();
		ExecChangeColor(new Color(R/255,G/255,B/255, 1f));
	}

	// Update is called once per frame
	public void UpdateHealthBar (float percent) {
		if (!isInited){
			spriteRenderer.transform.localScale = new Vector3(1f,2f,0f);
		}
		spriteRenderer.transform.localScale = new Vector3(percent,2f,0f);
	}
}
