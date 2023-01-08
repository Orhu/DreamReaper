using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIController : MonoBehaviour {
    [Header("Game Overlay UI")]
    //[SerializeField] Image[] phaseCounters;
    [SerializeField] Image itemBox;
    [SerializeField] TMP_Text moveCounter;

    [Header("Item Animation Frames")]
    [SerializeField] Sprite[] keyAnimationFrames;
    [SerializeField] Sprite[] scytheAnimationFrames;
    [SerializeField] Sprite[] hourglassAnimationFrames;
    [SerializeField] int animLength = 3;

    [SerializeField] float animationSpeed = 0.5f; // time for each frame

    [Header("Pause Menu")]
    [SerializeField] GameObject pausePanel;
    [SerializeField] GameObject pauseMenuBase;
    [SerializeField] GameObject optionsMenuBase;
    [SerializeField] GameObject controlsMenuBase;

    [Header("Warning Screens")]
    [SerializeField] GameObject warningPanel;
    [SerializeField] GameObject resetConfirmBase;
    [SerializeField] GameObject titleConfirmBase;

    private bool gamePaused = false;

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

        // Pause Menu
        controlsMenuBase.SetActive(false);
        optionsMenuBase.SetActive(false);
        pauseMenuBase.SetActive(false);
        pausePanel.SetActive(false);

        // Warning Screens
        resetConfirmBase.SetActive(false);
        titleConfirmBase.SetActive(false);
        warningPanel.SetActive(false);

        gamePaused = false;
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

    // Pause Menu Navigation
    public void OpenPauseMenu() {
        // opened menu in scene controller
        gamePaused = true;
        pausePanel.SetActive(true);
        pauseMenuBase.SetActive(true);
    }
    public void ClosePauseMenu() { // resume button
        gamePaused = false;
        pausePanel.SetActive(false);
        pauseMenuBase.SetActive(false);
        SceneController.CloseMenu();
    }

    public void OpenOptionsMenu() { // options button
        pauseMenuBase.SetActive(false);
        optionsMenuBase.SetActive(true);
    }
    public void CloseOptionsMenu() {
        optionsMenuBase.SetActive(false);
        pauseMenuBase.SetActive(true);
    }

    public void OpenControlsMenu() { // controls button
        pauseMenuBase.SetActive(false);
        controlsMenuBase.SetActive(true);
    }
    public void CloseControlsMenu() {
        controlsMenuBase.SetActive(false);
        pauseMenuBase.SetActive(true);
    }

    // Warnings
    public void PromptResetConfirm() { // also on reset button press
        // opened menu in scene controller if we needed to
        if (gamePaused) {
            pausePanel.SetActive(false);
            pauseMenuBase.SetActive(false);
        }
        warningPanel.SetActive(true);
        resetConfirmBase.SetActive(true);
    }
    public void ConfirmReset() {
        warningPanel.SetActive(false);
        resetConfirmBase.SetActive(false);
        SceneController.CloseMenu();
        SceneController.RestartLevel();
    }
    public void CancelReset() {
        warningPanel.SetActive(false);
        resetConfirmBase.SetActive(false);
        if (gamePaused) {
            pausePanel.SetActive(true);
            pauseMenuBase.SetActive(true);
        } else {
            SceneController.CloseMenu();
        }
    }

    public void PromptTitleConfirm() { // back to title button
        pauseMenuBase.SetActive(false);
        warningPanel.SetActive(true);
        titleConfirmBase.SetActive(true);
    }
    public void ConfirmReturnToTitle() {
        titleConfirmBase.SetActive(false);
        warningPanel.SetActive(false);
        pausePanel.SetActive(false);
        SceneController.CloseMenu();
        SceneController.BackToTitle();
    }
    public void CancelReturnToTitle() {
        titleConfirmBase.SetActive(false);
        warningPanel.SetActive(false);
        pausePanel.SetActive(true);
        pauseMenuBase.SetActive(true);
    }
}
