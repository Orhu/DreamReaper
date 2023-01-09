using System.Collections;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using UnityEngine.SceneManagement;

public class TitleScreenController : MonoBehaviour
{

    [SerializeField] GameObject levelSelect;
    [SerializeField] GameObject startScreen;

    [SerializeField] GameObject startButton;
    [SerializeField] GameObject levelSelectButton;

    [SerializeField] GameObject level1;
    [SerializeField] GameObject level2;
    [SerializeField] GameObject level3;
    [SerializeField] GameObject level4;
    [SerializeField] GameObject level5;
    [SerializeField] GameObject level6;
    [SerializeField] GameObject level7;
    [SerializeField] GameObject level8;
    [SerializeField] GameObject level9;
    [SerializeField] GameObject level10;
    [SerializeField] GameObject level11;
    [SerializeField] GameObject level12;

    public GameObject[] levels;

    // Start is called before the first frame update
    void Start()
    {
        levels = new GameObject[]{level1, level2, level3, level4, level5, level6, level7, level8, level9, level10, level11, level12};
        levelSelect.SetActive(false);
        startButton.SetActive(true);
        levelSelectButton.SetActive(true);
    }

    public void startGame(){
        SceneManager.LoadScene("Level1");
    }

    public void openStart(){
        startButton.SetActive(true);
        levelSelectButton.SetActive(true);
        levelSelect.SetActive(false);
    }

    public void LevelSelectOpen(){
        startButton.SetActive(false);
        levelSelectButton.SetActive(false);
        levelSelect.SetActive(true);
        foreach (int i in Enumerable.Range(0, 12)){
            if (!ProgressManager.unlockedLevels[i]){
                levels[i].GetComponent<Button>().enabled = false;
                levels[i].transform.GetChild(2).gameObject.SetActive(true);
            }
            else{
                levels[i].transform.GetChild(2).gameObject.SetActive(false);
                if (ProgressManager.bestMoves[i] == -1){
                    levels[i].transform.GetChild(1).gameObject.GetComponent<TMP_Text>().text = "Best:\nn/a";
                }
                else{
                    levels[i].transform.GetChild(1).gameObject.GetComponent<TMP_Text>().text = "Best:\n" + ProgressManager.bestMoves[i].ToString(); 
                }
            }
        }
    }

    public void selectThisLevel(){
        SceneManager.LoadScene(EventSystem.current.currentSelectedGameObject.name);
    }
}
