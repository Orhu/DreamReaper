using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIController : MonoBehaviour {
    [SerializeField] Image itemBox;
    [SerializeField] Sprite[] itemSprites;

    private int curSprite = 0;
    private bool itemActive = false;
    
    void Start() {
        UpdateSprite();
    }

    public void ChangeItemSprite(int newSprite) {
        curSprite = newSprite;
        UpdateSprite();
    }

    public void ToggleItemActive() {
        if (itemActive) {
            curSprite = curSprite + 3;
        } else {
            curSprite = curSprite - 3;
        }
        itemActive = !itemActive;
        UpdateSprite();
    }

    private void UpdateSprite() {
        itemBox.sprite = itemSprites[curSprite];
    }

    public void RefreshUI() {
        Player plyr = SceneController._player.GetComponent<Player>();
        ChangeItemSprite(plyr.item);
    }
}
