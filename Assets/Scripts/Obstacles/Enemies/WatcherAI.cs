using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine;

public class WatcherAI : MonoBehaviour, IObstacle, IEnemy {
    public bool phasable {get; private set;} = false;
    public int type {get; private set;} = 2; // type key: 0 = wall, 1 = door, 2 = enemy, 3 = prop
    

    private float horizontal = 0f;
    private float vertical = 0f;


    [SerializeField] int facing = 0; // 0 = up, 1 = right, 2 = down, 3 = left
    [SerializeField] LayerMask playerMask;
    
    private float originalY;
    private float originalYL;
    private float originalXL;
    private bool playerCaught = false;

    private GameObject spriteChild;
    private GameObject lightObject;

    private BoxCollider2D mainBody;
    private SpriteRenderer lightBody;

    private Animator _anim;

    private bool blinkwait = true;

    public float hitboxLength;

    // Start is called before the first frame update
    void Start() {
        spriteChild = this.transform.GetChild(0).gameObject;        //animate sprite
        originalY = spriteChild.transform.position.y;

        mainBody = GetComponent<BoxCollider2D>();       //hitbox for eye
        lightObject = this.transform.GetChild(0).GetChild(0).gameObject;    //object for light

        lightBody = lightObject.GetComponent<SpriteRenderer>();  //light sprite
        originalYL = lightBody.transform.position.y;
        originalXL = lightBody.transform.position.x;

        _anim = this.transform.GetChild(0).GetComponent<Animator>();
        _anim.SetInteger("facing", facing);
        _anim.SetTrigger("start");

        hitboxLength = lightObject.transform.localScale.x;

        switch (facing) {
            case 0:
                vertical = 1f;
                horizontal = 0f;
                lightObject.transform.Rotate(Vector3.back * -90);
                break;
            case 1: 
                vertical = 0f;
                horizontal = 1f;
                break;
            case 2:
                vertical = -1f;
                horizontal = 0f;
                lightObject.transform.Rotate(Vector3.back * 90);
                break;
            case 3:
                vertical = 0f;
                horizontal = -1f;
                lightObject.transform.Rotate(Vector3.back * 180);
                break;
        }
        lightObject.transform.localPosition = new Vector3(horizontal * 0.88f, vertical * 0.88f, transform.localPosition.z);
        StartCoroutine(BlinkLoop());
    }

    // Update is called once per frame
    void Update() {   
        AnimateIdle();

        bool check = false;
        if (!playerCaught && gameObject.activeSelf) {
            check = CheckForPlayer();
            if (check == true){
                StartCoroutine(DeathOfPlayer()); // Calls function that waits two seconds then resets the level
            }
        }
    }

    public void OnTick() {
        if (gameObject.activeSelf) {
            StartCoroutine(AnimateWatcherMove());
        }
        
    }

    public bool IsPhasable() {
        return phasable;
    }
    public int GetObsType() {
        return type;
    }

    public void Kill() {
        gameObject.SetActive(false);
        SceneController.DecreaseEnemyCount();
        StopAllCoroutines();
    }

    private bool CheckForPlayer() {
        switch (facing) {
            case 0: //switch to facing right
                vertical = 1f;
                horizontal = 0f;
                break;
            case 1:     //switch to facing down
                vertical = 0f;
                horizontal = 1f;
                break;
            case 2:     //switch to facing left
                vertical = -1f;
                horizontal = 0f;
                break;
            case 3:     //switch to facing up
                vertical = 0f;
                horizontal = -1f;
                break;
        }
        for (int i = 1; i < 1 + hitboxLength; i++){
            Vector2 coordCheck = new Vector2(transform.position.x + horizontal * i * .64f, transform.position.y + vertical * i * .64f);
            Collider2D col = Physics2D.OverlapPoint(coordCheck, playerMask);
            if (col != null) {
                playerCaught = true;
                Debug.Log("Gotcha!");
            }
        }
        Collider2D watcherCol = Physics2D.OverlapPoint(transform.position, playerMask);
        if (watcherCol != null) {
            playerCaught = true;
            Debug.Log("Gotcha!");
        }
        return (playerCaught);
    }

    private void AnimateIdle() {
        Vector2 floatY = spriteChild.transform.position;                    //bobbing motion
        floatY.y = originalY + .2f * (Mathf.Sin(Time.time) * .3f);
        spriteChild.transform.position = new Vector3(floatY.x, floatY.y, transform.position.z);

        /*
        Vector2 positonVector = lightBody.transform.position;     
        switch (facing){
            case 0:
                positonVector.y = (originalYL + .9f) + .2f * (Mathf.Sin(Time.time) * .5f);
                //lightBody.transform.position = new Vector3(positonVector.x, positonVector.y, transform.position.z);
                break;
            case 1:                   //bobbing motion
                positonVector.y = originalYL + .2f * (Mathf.Sin(Time.time) * .5f);
                //lightBody.transform.position = new Vector3(positonVector.x, positonVector.y, transform.position.z);
                break;
            case 2:               //bobbing motion
                positonVector.y = (originalYL -.9f) + .2f * (Mathf.Sin(Time.time) * .5f);
                //lightBody.transform.position = new Vector3(transform.position.x, positonVector.y, transform.position.z);
                break;
            case 3:               //bobbing motion
                positonVector.y = originalYL + .2f * (Mathf.Sin(Time.time) * .5f);
                //lightBody.transform.position = new Vector3(positonVector.x, positonVector.y, transform.position.z);
                break;
        }
        */
    }

    private IEnumerator BlinkLoop() { // blink animation
        float timeToBlink = 3f;
        while (timeToBlink > 0f) {
            if (blinkwait) {
                timeToBlink -= Time.deltaTime;
            }
            yield return null;
        }
        _anim.SetTrigger("blink");
        yield return new WaitForSeconds(1f);
        StartCoroutine(BlinkLoop());
    }

    private IEnumerator AnimateWatcherMove() {
        // TO DO
        // 1) hide light bar
        lightObject.SetActive(false);
        // 2) play animation, wait for it to finish
        _anim.SetTrigger("tick");
        switch (facing) {
            case 0: //switch to facing right
                vertical = 0f;
                horizontal = 1f;
                facing = 1;
                break;
            case 1:     //switch to facing down
                vertical = -1f;
                horizontal = 0f;
                facing = 2;
                break;
            case 2:     //switch to facing left
                vertical = 0f;
                horizontal = -1f;
                facing = 3;
                break;
            case 3:     //switch to facing up
                vertical = 1f;
                horizontal = 0f;
                facing = 0;
                break;
        }
        lightObject.transform.Rotate(Vector3.back * 90);
        lightObject.transform.localPosition = new Vector3(horizontal * 0.88f, vertical * 0.88f, transform.localPosition.z);
        yield return new WaitForSeconds(0.4f);
        // 3) reveal light bar
        lightObject.SetActive(true);

        // 4) send done message to SceneController
        SceneController.EnemyMoveDone();
    }

    private IEnumerator DeathOfPlayer(){
        yield return new WaitForSeconds(1f);
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}

