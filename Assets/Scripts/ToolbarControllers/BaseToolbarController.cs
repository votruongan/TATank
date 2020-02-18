using UnityEngine;
using UnityEngine.UI;

public class BaseToolbarController : BaseObjectController {
    public Button CloseButton;

    public virtual void Close(){
        this.gameObject.SetActive(false);
    }

    protected virtual void Start(){
        if (CloseButton == null)
            CloseButton = this.FindChildObject("Close_Button").GetComponent<Button>();
        CloseButton.onClick.AddListener(Close);
    }
}