using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealthBarSprite : BaseObjectController {

	bool isInited = false;
	public Vector3 localScale;
	SpriteRenderer spriteRenderer;
	public TextMesh playerName;
	public SpriteRenderer getTurnSprite;

	// Use this for initialization
	void Start () {
		if (!isInited){
			localScale = new Vector3(1f,2f,0f);
		}
		BaseChangeColor();
		isInited = true;
	}
	public void BaseChangeColor(){
		if (spriteRenderer == null){
			spriteRenderer = this.gameObject.GetComponent<SpriteRenderer>();
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
			localScale = new Vector3(1f,2f,0f);
		}
		localScale.x = percent;
		transform.localScale = localScale;
	}
}
