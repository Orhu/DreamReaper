using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour {
    private bool phasing = false;

    public int item {get; private set;} = 0;
    private bool usingItem = false; // do you want to make them confirm direction? Could part of the puzzle be approaching something in the correct direction?

    public int facing = 1; // 0 = up, 1 = right, 2 = down, 3 = left

    [Tooltip("Layers to check for obstacles the player can't stand on")]
    [SerializeField] LayerMask obstacleLayerMask;
    [Tooltip("Layers to check if you can phase through")]
    [SerializeField] LayerMask phaseLayerMask;
    [Tooltip("Layers to check for item usages")]
    [SerializeField] LayerMask itemLayerMask;
    [Tooltip("Layers to check for enemies only")]
    [SerializeField] LayerMask enemyLayerMask;
    [Tooltip("Layers to check for doors only")]
    [SerializeField] LayerMask doorLayerMask;

    private Animator _anim;

    // Start is called before the first frame update
    void Start() {
        _anim = GetComponent<Animator>();
        _anim.SetInteger("facing", facing);
    }

    // Update is called once per frame
    void Update() {
        int moveDirection = -1; // 0 = up, 1 = right, 2 = down, 3 = left
        if (Input.GetKeyDown(KeyCode.Z)) { // toggle phase mode
            if(!usingItem /*&& phaseAvailable*/) {
                phasing = !phasing;
            } else {
                Debug.Log("Player cannot phase while using an item");
            }
            // else play a "wrong" sound and give some kind of feedback
        } else if (Input.GetKeyDown(KeyCode.X)) { // toggle item mode
            if(!phasing && item != 0) {
                UseItem();
            } else {
                if (item == 0) {
                    Debug.Log("Player cannot use an item if they have none");
                } else if (phasing) {
                    Debug.Log("Player cannot use an item while phasing");
                }
            }
            
        } else if (Input.GetKeyDown(KeyCode.UpArrow)) { // check for arrow keys to move
            moveDirection = 0;
            facing = 0;
        } else if (Input.GetKeyDown(KeyCode.RightArrow)) {
            moveDirection = 1;
            facing = 1;
        } else if (Input.GetKeyDown(KeyCode.DownArrow)) {
            moveDirection = 2;
            facing = 2;
        } else if (Input.GetKeyDown(KeyCode.LeftArrow)) {
            moveDirection = 3;
            facing = 3;
        }

        if (moveDirection != -1) {
            _anim.SetInteger("facing",facing);
            // case for phase, items, moving
            if (phasing) {
                PhaseMove(moveDirection);
            } else {
                PlayerMove(moveDirection);
            }
        }
    }

    public void PlayerMove(int moveDirection) {
        Vector2 posUpdate = new Vector2(0f,0f);
        switch (moveDirection) {
            case 0:
                posUpdate = new Vector2(0f, 0.64f);
                break;
            case 1:
                posUpdate = new Vector2(0.64f, 0f);
                break;
            case 2:
                posUpdate = new Vector2(0f, -0.64f);
                break;
            case 3:
                posUpdate = new Vector2(-0.64f, 0f);
                break;
        }

        // check to make sure no walls or obstacles (not enemies, goals, or items) are in the way
        Vector3 currentPos = transform.position;
        Collider2D col = Physics2D.OverlapPoint(new Vector2(posUpdate.x + currentPos.x, posUpdate.y + currentPos.y), obstacleLayerMask);

        if (col != null) {
            // play invalid action sound
            Debug.Log("Player cannot move to new position due to obstacle at destination");
            return;
        }

        transform.position = new Vector3(posUpdate.x + currentPos.x, posUpdate.y + currentPos.y, 0f); // temporary, will do in a proper coroutine later

        Debug.Log("Player Moved");
        CheckItem();
        SceneController.Tick();
    }

    public void PhaseMove(int moveDirection) {
        // phasing requires a phasable obstacle or nothing be between the player and their destination tile
        Vector2 posUpdate = new Vector2(0f,0f);
        switch (moveDirection) {
            case 0:
                posUpdate = new Vector2(0f, 1.28f);
                break;
            case 1:
                posUpdate = new Vector2(1.28f, 0f);
                break;
            case 2:
                posUpdate = new Vector2(0f, -1.28f);
                break;
            case 3:
                posUpdate = new Vector2(-1.28f, 0f);
                break;
        }

        // check for phasable obstacle
        // take player position, add half of posUpdate, perform a point cast, check if whats there is phasable
        // if you can phase through, check if the square on the other side of the phasable thing is valid and move to it.

        Vector3 currentPos = transform.position;

        Collider2D phaseCol = Physics2D.OverlapPoint(new Vector2(posUpdate.x/2f + currentPos.x, posUpdate.y/2f + currentPos.y), phaseLayerMask);
        if (phaseCol != null) {
            IObstacle phaseObj = phaseCol.GetComponent<IObstacle>();
            if (phaseObj != null) {
                if (!phaseObj.IsPhasable()) { // if object isn't phasable
                    phasing = false;
                    Debug.Log("Player cannot phase through object");
                    // play invalid phase sound/relevant animation to show you can't phase through the thing
                    return;
                }
            }
        }

        Collider2D destCol = Physics2D.OverlapPoint(new Vector2(posUpdate.x + currentPos.x, posUpdate.y + currentPos.y), obstacleLayerMask);
        if (destCol != null) {
            // play invalid action sound
            Debug.Log("Player cannot move to new position due to obstacle at destination, trying to move to next square instead");
            PlayerMove(moveDirection);
            phasing = false;
            return;
        }

        phasing = false;
        transform.position = new Vector3(posUpdate.x + currentPos.x, posUpdate.y + currentPos.y, 0f); // temporary, will do in a proper coroutine later

        Debug.Log("Player Phased");
        CheckItem();
        SceneController.Tick();
    }

    public void CheckItem() {
        Collider2D itemCol = Physics2D.OverlapPoint(transform.position, itemLayerMask);
        if (itemCol != null) {
            Item it = itemCol.GetComponent<Item>();
            if (it != null) {
                item = it.PickupItem();
                Destroy(it.gameObject);

                Debug.Log($"Player picked up item: {item}");
                SceneController._ui.RefreshUI();
            }
        }

    }

    public void UseItem() {
        // TO DO
        Debug.Log("Player should use item here");

        switch (item) {
            case 1: // key
                UseKey();
                return;
            case 2: // scythe
                UseScythe();
                return;
            case 3: // hourglass
                UseHourglass();
                return;
            default:
                Debug.Log($"No item to use or invalid item num: {item}");
                return;
        }
    }

    private void UseKey() {
        // TO DO
        // check if thing in front of player is a locked door
        Debug.Log("Use key");
        Vector2 curPos = new Vector2(transform.position.x, transform.position.y);
        Vector2 frontPos = new Vector2(0f,0f);
        switch (facing) {
            case 0:
                frontPos = new Vector2(curPos.x, curPos.y + 0.64f);
                break;
            case 1:
                frontPos = new Vector2(curPos.x + 0.64f, curPos.y);
                break;
            case 2:
                frontPos = new Vector2(curPos.x, curPos.y - 0.64f);
                break;
            case 3:
                frontPos = new Vector2(curPos.x - 0.64f, curPos.y);
                break;
        }
        Collider2D frontCol = Physics2D.OverlapPoint(frontPos, doorLayerMask);
        // if door, unlock door and remove item
        if (frontCol != null) {
            Door hitdoor = frontCol.GetComponent<Door>();
            if (hitdoor.IsBlocked()) {
                Debug.Log("Unlocking door with key");
                hitdoor.Unblock();
                item = 0;
                SceneController._ui.RefreshUI();
            }
        }
        // else, play animation
        //tick forward
        SceneController.Tick();
    }

    private void UseScythe() {
        // TO DO
        // Check if thing in front of player is an enemy
        // find front
        Debug.Log("Use scythe");
        Vector2 curPos = new Vector2(transform.position.x, transform.position.y);
        Vector2 frontPos = new Vector2(0f,0f);
        switch (facing) {
            case 0:
                frontPos = new Vector2(curPos.x, curPos.y + 0.64f);
                break;
            case 1:
                frontPos = new Vector2(curPos.x + 0.64f, curPos.y);
                break;
            case 2:
                frontPos = new Vector2(curPos.x, curPos.y - 0.64f);
                break;
            case 3:
                frontPos = new Vector2(curPos.x - 0.64f, curPos.y);
                break;
        }
        Collider2D frontCol = Physics2D.OverlapPoint(frontPos, enemyLayerMask);
        // if enemy, kill and remove item
        if (frontCol != null) {
            IEnemy hitEnemy = frontCol.GetComponent<IEnemy>();
            Debug.Log("Attempting to kill enemy");
            hitEnemy.Kill();
            item = 0;
            SceneController._ui.RefreshUI();
        }
        // else, play animation
        // tick forward
        SceneController.Tick();
    }

    private void UseHourglass() {
        Debug.Log("Freezing Time for 3 turns");
        item = 0;
        SceneController.ActivateFreeze();
        SceneController._ui.RefreshUI();
    }

    public void KillPlayer() {
        // play death animation and reload level
        Debug.Log("Player should die here");
    }
}
