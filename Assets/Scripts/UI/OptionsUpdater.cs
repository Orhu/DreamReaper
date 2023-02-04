using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class OptionsUpdater : MonoBehaviour {
    [SerializeField] Toggle confirmResetToggle;
    [SerializeField] Toggle moveCounterToggle;
    [SerializeField] Slider masterVolumeSlide;
    [SerializeField] Slider soundVolumeSlide;
    [SerializeField] Slider musicVolumeSlide;
    // Start is called before the first frame update
    void Start() {
        confirmResetToggle.isOn = UIController.confirmReset;
        moveCounterToggle.isOn = UIController.showMoves;
        masterVolumeSlide.value = SettingsManager.masterVolume;
        soundVolumeSlide.value = SettingsManager.soundsVolume;
        musicVolumeSlide.value = SettingsManager.musicVolume;
    }
}
