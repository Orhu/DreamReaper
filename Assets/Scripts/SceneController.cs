using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneController : MonoBehaviour {
    public static int levelNum = 0;

    private static ZombieAI[] Zombies;
    private static WatcherAI[] Watchers;

    public static GameObject _player;

    private static int freezeTimer = 0;

    void Start(){
        _player = GameObject.Find("Player");
        freezeTimer = 0;
        Zombies = FindObjectsOfType<ZombieAI>();
        Watchers = FindObjectsOfType<WatcherAI>();
    }

    public static void Tick() { // each time the player takes an action
        if (freezeTimer > 0) {
            freezeTimer--;
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

    public static void RestartLevel() {
        // load this scene again SceneManager.LoadScene();
    }

    public static void WinLevel() {
        // load next scene SceneManager.LoadScene();
    }
}
