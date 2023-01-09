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
    [SerializeField] LayerMask obstacleLayerMask;

    [SerializeField] Sprite lightShort;
    [SerializeField] Sprite lightMid;
    [SerializeField] Sprite lightLong;

    
    private float originalY;
    private float originalYL;
    private float originalXL;
    private bool playerCaught = false;

    private GameObject spriteChild;
    private GameObject lightObject;

    private BoxCollider2D mainBody;
    private SpriteRenderer lightBody;

    private Animator _anim;
    private AudioSource _audioSource;

    private bool blinkwait = true;

    public float hitboxLength;
    public float hitboxDistance;
    private bool checking = true;

    private float timeToBlink = 3f;

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
        _audioSource = GetComponent<AudioSource>();

        playerCaught = false;

        //hitboxLength = lightObject.transform.localScale.x;

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
        for (int i = 1; i < 1 + hitboxLength; i++){
            if(UpdateRange(i)){
                break;
            }
            else{
                switch (hitboxLength){
                    case 1: //there is an object right in front
                        //all light sprites should be off
                        lightBody.sprite = lightShort;
                        hitboxDistance = .64f;
                        break;
                    case 2:     //there is an object 1 with 1 empty space between
                        //light 1 should be on
                        lightBody.sprite = lightMid;
                        hitboxDistance = .96f;
                        break;
                    case 3:     //there is an object 1 with 2 empty space between
                        //light 2 should be on
                        lightBody.sprite = lightLong;
                        hitboxDistance = 1.28f;
                        break;
                }
                lightObject.SetActive(true);
                lightObject.transform.localPosition = new Vector3(horizontal * hitboxDistance, vertical * hitboxDistance, transform.localPosition.z);
            }
        }
        

        StartCoroutine(BlinkLoop());
    }

    // Update is called once per frame
    void Update() {   
        AnimateIdle();
        if (!playerCaught && gameObject.activeSelf && SceneController.allowKills) {
            if (checking) {
                if (CheckForPlayer()){
                    StopAllCoroutines();
                    SceneController.PlayerCaught(); // Calls function that waits two seconds then resets the level
                }
            }
        }
    }

    public void OnTick() {
        bool caught = false;
        if (!playerCaught && gameObject.activeSelf && SceneController.allowKills) {
            if (checking) {
                if (CheckForPlayer()){
                    caught = true;
                    StopAllCoroutines();
                    SceneController.PlayerCaught(); // Calls function that waits two seconds then resets the level
                }
            }
        }
        
        if (caught == false){
            if (gameObject.activeSelf) {
                StartCoroutine(AnimateWatcherMove());
            }
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
        SceneController.DecreaseEnemyCount(1);
        SceneController.getObjectName(gameObject, 1);
        StopAllCoroutines();
    }

    public bool UpdateRange(int i){
        Vector2 coordCheck = new Vector2(transform.position.x + horizontal * i * .64f, transform.position.y + vertical * i * .64f);
        Collider2D wallCheck = Physics2D.OverlapPoint(coordCheck, obstacleLayerMask);
        if (wallCheck != null){
            switch (i){
                case 1: //there is an object right in front
                    lightObject.SetActive(false);
                    break;
                case 2:     //there is an object 1 with 1 empty space between
                    lightBody.sprite = lightShort;
                    hitboxDistance = .64f;
                    lightObject.SetActive(true);
                    break;
                case 3:     //there is an object 1 with 2 empty space between
                    lightBody.sprite = lightMid;
                    hitboxDistance = .96f;
                    lightObject.SetActive(true);
                    break;
                case 4:     //there is an object 1 with 3 empty space between
                    lightBody.sprite = lightLong;
                    hitboxDistance = 1.28f;
                    lightObject.SetActive(true);
                    break;
            }
            lightObject.transform.localPosition = new Vector3(horizontal * hitboxDistance, vertical * hitboxDistance, transform.localPosition.z);
            return(true);
        }
        return(false);
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

        //check if player hit watcher
        Collider2D pl = Physics2D.OverlapPoint(transform.position, playerMask);
        if (pl != null) {
            _audioSource.PlayOneShot(Resources.Load<AudioClip>("Sounds/watcherDeath"));
            playerCaught = true;
        }
        
        //check light beam
        for (int i = 1; i < 1 + hitboxLength; i++){
            if(UpdateRange(i)){
                break;
            }
            else{
                switch (hitboxLength){
                    case 1: //there is an object right in front
                        //all light sprites should be off
                        lightBody.sprite = lightShort;
                        hitboxDistance = .64f;
                        break;
                    case 2:     //there is an object 1 with 1 empty space between
                        //light 1 should be on
                        lightBody.sprite = lightMid;
                        hitboxDistance = .96f;
                        break;
                    case 3:     //there is an object 1 with 2 empty space between
                        //light 2 should be on
                        lightBody.sprite = lightLong;
                        hitboxDistance = 1.28f;
                        break;
                }
                lightObject.SetActive(true);
                lightObject.transform.localPosition = new Vector3(horizontal * hitboxDistance, vertical * hitboxDistance, transform.localPosition.z);
                Vector2 coordCheck = new Vector2(transform.position.x + horizontal * i * .64f, transform.position.y + vertical * i * .64f);
                Collider2D col = Physics2D.OverlapPoint(coordCheck, playerMask);
                if (col != null) {
                    _audioSource.PlayOneShot(Resources.Load<AudioClip>("Sounds/watcherBeamDeath"));
                    playerCaught = true;
                }
            }
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
        timeToBlink = 3f;
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
        checking = false;
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
        lightObject.transform.localPosition = new Vector3(horizontal * hitboxDistance, vertical * hitboxDistance, transform.localPosition.z);
        yield return new WaitForSeconds(0.2f);
        // 3) reveal light bar
        lightObject.SetActive(true);
        checking = true;
        // 4) send done message to SceneController
        timeToBlink = 3f;
        yield return null;
        SceneController.EnemyMoveDone();
    }
}

