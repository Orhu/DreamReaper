using System.Collections;
using System.Collections.Generic;
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

    public float hitboxLength;

    // Start is called before the first frame update
    void Start() {
        spriteChild = this.transform.GetChild(0).gameObject;        //animate sprite
        originalY = spriteChild.transform.position.y;

        mainBody = GetComponent<BoxCollider2D>();       //hitbox for eye
        lightObject = this.transform.GetChild(1).gameObject;    //object for light

        lightBody = lightObject.GetComponent<SpriteRenderer>();  //light sprite
        originalYL = lightBody.transform.position.y;
        originalXL = lightBody.transform.position.x;

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
        lightObject.transform.localPosition = new Vector3(horizontal * 1.5f, vertical * 1.5f, transform.localPosition.z);
    }

    // Update is called once per frame
    void Update() {   
        Vector2 floatY = spriteChild.transform.position;                    //bobbing motion
        floatY.y = originalY + .2f * (Mathf.Sin(Time.time) * .5f);
        spriteChild.transform.position = new Vector3(floatY.x, floatY.y, transform.position.z);

        Vector2 positonVector = lightBody.transform.position;     
        switch (facing){
            case 0:
                positonVector.y = (originalYL + .9f) + .2f * (Mathf.Sin(Time.time) * .5f);
                lightBody.transform.position = new Vector3(positonVector.x, positonVector.y, transform.position.z);
                break;
            case 1:                   //bobbing motion
                positonVector.y = originalYL + .2f * (Mathf.Sin(Time.time) * .5f);
                lightBody.transform.position = new Vector3(positonVector.x, positonVector.y, transform.position.z);
                break;
            case 2:               //bobbing motion
                positonVector.y = (originalYL -.9f) + .2f * (Mathf.Sin(Time.time) * .5f);
                lightBody.transform.position = new Vector3(transform.position.x, positonVector.y, transform.position.z);
                break;
            case 3:               //bobbing motion
                positonVector.y = originalYL + .2f * (Mathf.Sin(Time.time) * .5f);
                lightBody.transform.position = new Vector3(positonVector.x, positonVector.y, transform.position.z);
                break;
        }

        if (!playerCaught && gameObject.activeSelf) {
            CheckForPlayer();
        }
    }

    public void OnTick() {
        if (gameObject.activeSelf) {
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
            lightObject.transform.localPosition = new Vector3(horizontal * 1.5f, vertical * 1.5f, transform.localPosition.z);
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
        return (playerCaught);
    }
}

