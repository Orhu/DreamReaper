using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIController : MonoBehaviour {
    [SerializeField] Image itemBox;

    [Header("Animation Frames")]
    [SerializeField] Sprite[] keyAnimationFrames;
    [SerializeField] Sprite[] scytheAnimationFrames;
    [SerializeField] Sprite[] hourglassAnimationFrames;
    [SerializeField] int animLength = 3;

    [SerializeField] float animationSpeed = 0.5f; // time for each frame

    private float timeRemaining;
    private int frame = 0;

    private int curSprite = 0;

    public static bool showMoves {get; private set;} = false;
    public static bool confirmReset {get; private set;} = true;
    
    void Start() {
        curSprite = 0;
        frame = 0;
        timeRemaining = animationSpeed;
        itemBox.gameObject.SetActive(false);
    }

    void Update() {
        if (curSprite != 0) {
            timeRemaining -= Time.deltaTime;
            if (timeRemaining <= 0f) {
                frame += 1;
                if (frame >= animLength) {
                    frame = 0;
                }
                switch (curSprite) {
                    case 1:
                        itemBox.sprite = keyAnimationFrames[frame];
                        break;
                    case 2:
                        itemBox.sprite = scytheAnimationFrames[frame];
                        break;
                    case 3:
                        itemBox.sprite = hourglassAnimationFrames[frame];
                        break;
                }
                timeRemaining = animationSpeed;
            }
        }
    }

    public void ChangeItemSprite(int newSprite) {
        curSprite = newSprite;
        frame = 0;
        if (curSprite == 0) {
            itemBox.gameObject.SetActive(false);
        } else {
            switch (curSprite) {
                case 1:
                    itemBox.sprite = keyAnimationFrames[frame];
                    break;
                case 2:
                    itemBox.sprite = scytheAnimationFrames[frame];
                    break;
                case 3:
                    itemBox.sprite = hourglassAnimationFrames[frame];
                    break;
            }
            itemBox.gameObject.SetActive(true);
        }
    }

    public void UpdateMovesCounter() {
        if (showMoves) {
            //moveCounter.text = $"Moves: {SceneController.numMoves}";
        }
    }

    public void RefreshUI() {
        Player plyr = SceneController._player.GetComponent<Player>();
        ChangeItemSprite(plyr.item);

        // show moves counter if turned on
    }

    public static void SetShowMoves(bool newVal) {
        showMoves = newVal;
    }

    public static void SetConfirmReset(bool newVal) {
        confirmReset = newVal;
    }
}
