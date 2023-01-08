using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SettingsManager : MonoBehaviour {
    // This script holds the values for different settings (mostly audio). Values can be acquired in any script.
    public static float masterVolume {get; private set;} = 0.5f;
    public static float soundsVolume {get; private set;} = 1f;
    public static float musicVolume {get; private set;} = 0.8f;

    public static void ChangeShowMoves(bool newVal) {
        UIController.SetShowMoves(newVal);
    }

    public static void ChangeConfirmReset(bool newVal) {
        UIController.SetConfirmReset(newVal);
    }

    public static void ChangeMasterVolume(float newVal) {
        masterVolume = newVal;

        // update volume on all audio sources
        GameObject[] soundPlayers = GameObject.FindGameObjectsWithTag("SoundPlayer");
        GameObject[] musicPlayers = GameObject.FindGameObjectsWithTag("MusicPlayer");

        foreach (GameObject player in soundPlayers) {
            AudioSource audio = player.GetComponent<AudioSource>();
            if (audio != null) {
                audio.volume = masterVolume * soundsVolume;
            }
        }
        foreach (GameObject player in musicPlayers) {
            AudioSource audio = player.GetComponent<AudioSource>();
            if (audio != null) {
                audio.volume = masterVolume * musicVolume;
            }
        }
    }

    public static void ChangeSoundsVolume(float newVal) {
        soundsVolume = newVal;

        // update volume on all Sound Players
        GameObject[] soundPlayers = GameObject.FindGameObjectsWithTag("SoundPlayer");
        foreach (GameObject player in soundPlayers) {
            AudioSource audio = player.GetComponent<AudioSource>();
            if (audio != null) {
                audio.volume = masterVolume * soundsVolume;
            } else {
                Debug.Log("couldn't find an audio source on this guy");
            }
        }
    }

    public static void ChangeMusicVolume(float newVal) {
        musicVolume = newVal;

        // update volume on all music players
        GameObject[] musicPlayers = GameObject.FindGameObjectsWithTag("MusicPlayer");
        foreach (GameObject player in musicPlayers) {
            AudioSource audio = player.GetComponent<AudioSource>();
            if (audio != null) {
                audio.volume = masterVolume * musicVolume;
            }
        }
    }
}
