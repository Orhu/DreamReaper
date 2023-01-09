using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneController : MonoBehaviour {
    public static int levelNum = 0;
    public static int numMoves = 0;

    private static ZombieAI[] Zombies;
    private static WatcherAI[] Watchers;

    public static GameObject _player;
    public static UIController _ui;
    public static AudioSource _audioSource;
    public static AudioSource _soundSource;

    private static int freezeTimer = 0;

    private static int enemyCount;
    private static int enemiesDone;

    private static bool playerCaught = false;

    public static bool allowKills = false;

    public static GameState gameState = GameState.GAME;

    void Awake() {
        SceneManager.LoadScene(1, LoadSceneMode.Additive);
    }

    void Start(){
        _player = GameObject.Find("Player");
        _ui = GameObject.Find("UI").GetComponent<UIController>();
        _audioSource = GetComponent<AudioSource>(); // this audio source will play background music in each level
        _soundSource = transform.GetChild(0).GetComponent<AudioSource>();
        
        Zombies = FindObjectsOfType<ZombieAI>();
        Watchers = FindObjectsOfType<WatcherAI>();
        enemyCount = Zombies.Length + Watchers.Length;

        levelNum = SceneManager.GetActiveScene().buildIndex-2;
        numMoves = 0;
        freezeTimer = 0;
        gameState = GameState.GAME;
        playerCaught = false;
        allowKills = false;

        _audioSource.volume = SettingsManager.masterVolume * SettingsManager.musicVolume;
        _soundSource.volume = SettingsManager.masterVolume * SettingsManager.soundsVolume;

        _ui.UpdatePhaseCounter(_player.GetComponent<Player>().maxPhases);
    }

    void Update() {
        /*if (gameState == GameState.MENU) {
            if (Input.GetKeyDown(KeyCode.Escape)) {
                _ui.GoBack();
            }
        }*/
    }

    public static void Tick() { // each time the player takes an action
        numMoves++;
        _ui.UpdateMovesCounter();
        allowKills = true;
        if (playerCaught) {
            return;
        }
        if (freezeTimer > 0 || enemyCount == 0) {
            freezeTimer--;
            _player.GetComponent<Player>().canAct = true;
        } else {
            foreach (ZombieAI zombie in Zombies){
                zombie.OnTick();
            }
            foreach (WatcherAI watcher in Watchers){
                watcher.OnTick();
            }
            // call OnTick() on every game object in the scene
        }
    }

    public static void ActivateFreeze() {
        freezeTimer = 4;
    }

    public static void getObjectName(GameObject gm, int enemyType){
        Debug.Log(gm.name);
        switch (enemyType) {
            case 0:
                foreach (ZombieAI zombie in Zombies){
                    if (zombie.name == gm.name){
                        Destroy(zombie.gameObject);
                        Zombies = FindObjectsOfType<ZombieAI>();
                        enemyCount = Watchers.Length + Zombies.Length;
                    }
                }
                break;
            case 1:
                foreach (WatcherAI watcher in Watchers){
                    if (watcher.name == gm.name){
                        Destroy(watcher.gameObject);
                        Watchers = FindObjectsOfType<WatcherAI>();
                        enemyCount = Watchers.Length + Zombies.Length;
                    }
                }
                break;
        }

    }

    public static void PlayerRestartLevel() {
        gameState = GameState.MENU;
        if (UIController.confirmReset) {
            _ui.PromptResetConfirm();
        } else {
            RestartLevel();
        }
    }

    public static void PlayerCaught() {
        playerCaught = true;
        allowKills = false;
        _player.GetComponent<Player>().KillPlayer();
    }

    public static void RestartLevel() {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public static void ClearLevel() {
        ProgressManager.ClearLevel(levelNum, numMoves);
        if (levelNum == 11) {
            SceneManager.LoadScene(0);
        } else {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex+1);
        }
    }

    public static void EnemyMoveDone() {
        enemiesDone += 1;
        if (enemiesDone == enemyCount) {
            if(enemyCount != 0) {
                var enemyMoveSound = Resources.Load<AudioClip>("Sounds/enemyMove");
                _soundSource.PlayOneShot(enemyMoveSound);
            }
            _player.GetComponent<Player>().canAct = true;
            enemiesDone = 0;
            allowKills = false;
        }
    }

    public static void DecreaseEnemyCount(int enemyType) { // 0 = zombo, 1 = watcher
        //enemyCount--;
        switch (enemyType) {
            case 0:
                _soundSource.PlayOneShot(Resources.Load<AudioClip>("Sounds/zombieKill"));
                break;
            case 1:
                _soundSource.PlayOneShot(Resources.Load<AudioClip>("Sounds/watcherKill"));
                break;
        }
    }

    // GameState Stuff
    public static void OpenMenu() {
        gameState = GameState.MENU;
        _audioSource.Pause();
    }

    public static void CloseMenu() {
        gameState = GameState.GAME;
        _ui.RefreshUI();
        _audioSource.UnPause();
    }

    public static void OpenPauseMenu() {
        OpenMenu();
        _ui.OpenPauseMenu();
    }

    public static void BackToTitle() {
        SceneManager.LoadScene(0);
    }

    public static void LoadLevel(int levelNum) { // for loading a scene from title
        SceneManager.LoadScene(levelNum+1); // level nums start at 1, level scenes start at 2
    }
}

