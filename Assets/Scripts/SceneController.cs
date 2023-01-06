using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneController : MonoBehaviour {
    public static int levelNum = 0;

    public static void Tick() { // each time the player takes an action
        Debug.Log("Tick");
        // call OnTick() on every game object in the scene
    }

    public static void RestartLevel() {
        // load this scene again SceneManager.LoadScene();
    }

    public static void WinLevel() {
        // load next scene SceneManager.LoadScene();
    }
}
