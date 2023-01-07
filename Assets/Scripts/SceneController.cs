using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneController : MonoBehaviour {
    public static int levelNum = 0;

    private static ZombieAI[] Zombies;
    private static WatcherAI[] Watchers;

    public static GameObject _player;
    public static UIController _ui;
    public static AudioSource _audioSource;

    private static int freezeTimer = 0;

    private static int enemyCount;
    private static int enemiesDone;

    private static bool playerCaught = false;


    void Start(){
        _player = GameObject.Find("Player");
        _ui = GameObject.Find("UI").GetComponent<UIController>();
        _audioSource = GetComponent<AudioSource>();
        freezeTimer = 0;
        Zombies = FindObjectsOfType<ZombieAI>();
        Watchers = FindObjectsOfType<WatcherAI>();

        enemyCount = Zombies.Length + Watchers.Length;
        playerCaught = false;
    }

    public static void Tick() { // each time the player takes an action
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
        freezeTimer = 3;
    }

    public static void PlayerRestartLevel() {

    }

    public static void PlayerCaught() {
        playerCaught = true;
    }

    public static void RestartLevel() {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public static void ClearLevel() {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex+1);
    }

    public static void EnemyMoveDone() {
        enemiesDone += 1;
        if (enemiesDone == enemyCount) {
            _player.GetComponent<Player>().canAct = true;
            enemiesDone = 0;
        }
    }

    public static void DecreaseEnemyCount() {
        enemyCount--;
    }
}

