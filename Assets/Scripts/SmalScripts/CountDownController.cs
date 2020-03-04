using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CountDownController : MonoBehaviour
{
    public int elapsed = 0;
    public int currentPos;
    [Header("SETTINGS")]
    public int initSeconds = 15;
    public List<Texture> numberSprites;
    [Header("FOR DEBUGGING")]
    Texture nullTexture;
    public GameController gameController;
    public RawImage thisImage;
    public float timer;

    // Start is called before the first frame update
    void Start()
    {
        if(gameController == null){
            gameController = GameObject.Find("GameController").GetComponent<GameController>();
        }
        if(thisImage == null){
            thisImage = this.gameObject.GetComponent<RawImage>();
        }
        // ToggleObject(false);
        nullTexture = thisImage.texture;
        currentPos = 10;
    }

    public void ToggleObject(bool value){
        elapsed = 0;
        currentPos = initSeconds;        
        thisImage.texture = nullTexture;
        this.gameObject.SetActive(value);
        if(gameController == null){
            gameController = GameObject.Find("GameController").GetComponent<GameController>();
        }
    }


    // Update is called once per frame
    private void FixedUpdate() {
        if (!(gameController.mainPlayerController.isOnTurn)
            ||(gameController.mainPlayerController.isFiring)){
            return;
        }
        timer += Time.deltaTime;
        if (timer >= 1f){
            timer = 0f;
            elapsed++;
            if (currentPos == 0){
                currentPos = initSeconds;
                gameController.MainPlayerSkip();
            }
            currentPos--;
            if (currentPos < numberSprites.Count){
                thisImage.texture = numberSprites[currentPos];
            }
        }
    }
}
