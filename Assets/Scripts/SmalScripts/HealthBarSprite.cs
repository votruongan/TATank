using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealthBarSprite : MonoBehaviour {

	Vector3 localScale;
	SpriteRenderer spriteRenderer;

	// Use this for initialization
	void Start () {
		localScale = transform.localScale;
		spriteRenderer = this.gameObject.GetComponent<SpriteRenderer>();
	}
	public void ChangeToRed(){
		spriteRenderer.color = Color.red;
	}
	public void ChangeToBlue(){
		spriteRenderer.color = Color.blue;
	}
	public void ChangeColor(int R, int G, int B){
		spriteRenderer.color = new Color(R/255,G/255,B/255, 1f);
	}

	// Update is called once per frame
	public void UpdateHealthBar (float percent) {
		localScale.x = percent;
		transform.localScale = localScale;
	}
}
