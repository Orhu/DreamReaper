using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour {
    private bool phasing = false;

    public int item {get; private set;} = 0;
    private bool usingItem = false; // do you want to make them confirm direction? Could part of the puzzle be approaching something in the correct direction?

    public int facing = 1; // 0 = up, 1 = right, 2 = down, 3 = left

    public int maxPhases = 3;
    public int curPhases;

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

    [SerializeField] GameObject smokeParticlePrefab;
    [SerializeField] GameObject scytheParticlePrefab;

    private Animator _anim;
    private SpriteRenderer _sprite;
    private AudioSource _audioSource;

    public bool canAct = true;

    // Start is called before the first frame update
    void Start() {
        curPhases = maxPhases;
        _anim = GetComponent<Animator>();
        _sprite = GetComponent<SpriteRenderer>();
        _audioSource = GetComponent<AudioSource>(); // this audio source will play all player-related sounds

        _anim.SetInteger("facing", facing);
        _audioSource.volume = SettingsManager.masterVolume * SettingsManager.soundsVolume; // sets volume on attached audio source

        canAct = true;
    }

    // Update is called once per frame
    void Update() {
        if (Input.GetKeyDown(KeyCode.Escape)) { // open pause menu
            if (SceneController.gameState == GameState.MENU) {
                SceneController._ui.GoBack();
                Debug.Log("back");
            } else if (SceneController.gameState == GameState.GAME) {
                SceneController.OpenPauseMenu();
                Debug.Log("open menu");
            } 
        }
        if (SceneController.gameState == GameState.GAME) {
            if (Input.GetKeyDown(KeyCode.Backspace)) { // restart level
                SceneController.PlayerRestartLevel();
            }
            if (canAct) {
                int moveDirection = -1; // 0 = up, 1 = right, 2 = down, 3 = left
                if (Input.GetKeyDown(KeyCode.Z)) { // toggle phase mode
                    if(!usingItem && curPhases > 0) {
                        phasing = !phasing;
                        // phase sound
                        if (phasing) {
                            GameObject particle = Instantiate<GameObject>(smokeParticlePrefab);
                            particle.transform.position = new Vector3(transform.position.x, transform.position.y, -1f);
                            particle.GetComponent<Animator>().SetBool("out", true);
                            _audioSource.PlayOneShot(Resources.Load<AudioClip>("Sounds/playerActivatePhaseMix"));
                        } else {
                            GameObject particle = Instantiate<GameObject>(smokeParticlePrefab);
                            particle.transform.position = new Vector3(transform.position.x, transform.position.y, -1f);
                            particle.GetComponent<Animator>().SetBool("out", false);
                            _audioSource.PlayOneShot(Resources.Load<AudioClip>("Sounds/playerDeactivatePhaseMix"));
                        }
                        _anim.SetBool("phasing", phasing);
                    } else if (usingItem) {
                        Debug.Log("Player cannot phase while using an item");
                    } else if (curPhases <= 0) { // no phasing at all (even if you're not phasing through anything) when out of charge
                        Debug.Log("Player is out of phases");
                        StartCoroutine(AnimateOutOfPhases());
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
                } else if (Input.GetKeyDown(KeyCode.C)) {// wait
                    Debug.Log("Waiting a turn");
                    canAct = false;
                    SceneController.Tick();
                } else if (Input.GetKey(KeyCode.UpArrow)) { // check for arrow keys to move
                    moveDirection = 0;
                    facing = 0;
                } else if (Input.GetKey(KeyCode.RightArrow)) {
                    moveDirection = 1;
                    facing = 1;
                } else if (Input.GetKey(KeyCode.DownArrow)) {
                    moveDirection = 2;
                    facing = 2;
                } else if (Input.GetKey(KeyCode.LeftArrow)) {
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
        }
    }

    public void PlayerMove(int moveDirection) {
        bool failFlag = false; // becomes true if attempting an invalid move

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
            failFlag = true;
        }

        Vector2 dest = new Vector2(posUpdate.x + currentPos.x, posUpdate.y + currentPos.y);

        Debug.Log($"Player Moved. Fail? {failFlag}");
        StartCoroutine(AnimateMove(dest, failFlag));

        //transform.position = new Vector3(posUpdate.x + currentPos.x, posUpdate.y + currentPos.y, 0f); temporary, will do in a proper coroutine later

        //Debug.Log("Player Moved");
        //CheckItem();
        //SceneController.Tick();
    }

    public void PhaseMove(int moveDirection) {
        bool phaseFlag = false; // to indicate if the player is trying to phase through something
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
        Vector3 dest = new Vector3(posUpdate.x + currentPos.x, posUpdate.y + currentPos.y, 0f); // temporary, will do in a proper coroutine later

        Collider2D phaseCol = Physics2D.OverlapPoint(new Vector2(posUpdate.x/2f + currentPos.x, posUpdate.y/2f + currentPos.y), phaseLayerMask);
        if (phaseCol != null) {
            IObstacle phaseObj = phaseCol.GetComponent<IObstacle>();
            if (phaseObj != null) {
                if (!phaseObj.IsPhasable()) { // if object isn't phasable
                    Debug.Log("Player cannot phase through object");
                    // check for blocked door
                    Door doorCheck = phaseCol.GetComponent<Door>();
                    if (doorCheck != null) {
                        if (doorCheck.IsBlocked()) {
                            StartCoroutine(AnimatePhase(dest, 3));
                            return;
                        }
                    }
                    // play invalid phase sound/relevant animation to show you can't phase through the thing
                    StartCoroutine(AnimatePhase(dest, 2));
                    return;
                }
                phaseFlag = true;
            }
        }

        Collider2D destCol = Physics2D.OverlapPoint(new Vector2(posUpdate.x + currentPos.x, posUpdate.y + currentPos.y), obstacleLayerMask);
        if (destCol != null) {
            // play invalid action sound
            Debug.Log("Player cannot move to new position due to obstacle at destination, trying to move to next square instead");
            PlayerMove(moveDirection);
            phasing = false;

            _anim.SetBool("phasing", phasing);
            return;
        }

        if (phaseFlag) {
            StartCoroutine(AnimatePhase(dest, 0));
        } else {
            StartCoroutine(AnimatePhase(dest, 1));
        }

        Debug.Log("Player Phased");
        //CheckItem(); // check for item/goal under player
        //SceneController.Tick();
    }

    public void CheckItem() { // checks for both items AND the goal
        Collider2D itemCol = Physics2D.OverlapPoint(transform.position, itemLayerMask);
        if (itemCol != null) {
            if (itemCol.gameObject.name == "Goal") {
                StartCoroutine(ReachGoal());
            }
            Item it = itemCol.GetComponent<Item>();
            if (it != null) {
                int hold = it.PickupItem();
                if (hold == 4) { // if phase restore
                    _audioSource.PlayOneShot(Resources.Load<AudioClip>("Sounds/eatFood"));
                    RestorePhases(1);
                } else {
                    _audioSource.PlayOneShot(Resources.Load<AudioClip>("Sounds/playerPickupItem"));
                    item = hold;
                }
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
        canAct = false;
        _anim.SetTrigger("useItem");
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
                _audioSource.PlayOneShot(Resources.Load<AudioClip>("Sounds/UseKey"));
                item = 0;
                SceneController._ui.RefreshUI();
            }
        }
        // else, play animation
        _audioSource.PlayOneShot(Resources.Load<AudioClip>("Sounds/UseScythe"));
        //tick forward
        SceneController.Tick();
    }

    private void UseScythe() {
        // TO DO
        // Check if thing in front of player is an enemy
        // find front
        Debug.Log("Use scythe");
        canAct = false;
        _anim.SetTrigger("useItem");
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
            _audioSource.PlayOneShot(Resources.Load<AudioClip>("Sounds/UseScytheB"));
            item = 0;
            SceneController._ui.RefreshUI();
        }
        GameObject scytheParticle = Instantiate<GameObject>(scytheParticlePrefab);
        scytheParticle.transform.position = frontPos;
        if (frontCol == null) {
            _audioSource.PlayOneShot(Resources.Load<AudioClip>("Sounds/UseScythe"));
        }
        // else, play animation
        // tick forward
        SceneController.Tick();
    }

    private void UseHourglass() {
        Debug.Log("Freezing Time for 3 turns");
        canAct = false;
        _anim.SetTrigger("useItem");
        item = 0;
        _audioSource.PlayOneShot(Resources.Load<AudioClip>("Sounds/UseHourglass"));
        SceneController.ActivateFreeze();
        SceneController._ui.RefreshUI();
        SceneController.Tick();
    }

    private void RestorePhases(int numRest) {
        curPhases += numRest;
        if (curPhases > maxPhases) {
            curPhases = maxPhases;
        }
    }

    public void KillPlayer() {
        // play death animation and reload level
        _anim.SetTrigger("death");
        _audioSource.PlayOneShot(Resources.Load<AudioClip>("Sounds/playerDeathB"));
        Debug.Log("Player dead");
    }

    public IEnumerator ReachGoal() {
        // play win animation and show level clear screen
        //_audioSource.PlayOneShot(Resources.Load<AudioClip>("Sounds/playerPickupItem"));
        //yield return new WaitForSeconds(0.2f);
        _audioSource.PlayOneShot(Resources.Load<AudioClip>("Sounds/reaperLaugh"));
        yield return new WaitForSeconds(1.7f);
        SceneController.ClearLevel();
        Debug.Log("Player has reached goal");
    }

    public void callMinWait(){
        StartCoroutine(waitMinimumSeconds());
    }

    //dont let player move to quick
    public IEnumerator waitMinimumSeconds(){
        yield return new WaitForSeconds(.25f);
        canAct = true;
    }

    // code-based animation coroutines (this could also just be one large function but breaking it up is much nicer, just be sure to avoid redundancy)
    private IEnumerator AnimateMove(Vector2 dest, bool isFail) {
        // TO DO
        // play player move sound
        

        // quick linear interpolation between current position and destination position
        canAct = false;
        Vector2 start = transform.position;
        if (!isFail) { // successful move
            _audioSource.PlayOneShot(Resources.Load<AudioClip>("Sounds/playerMove"));
            float i = 0f;
            while (i < 1f) {
                transform.position = new Vector3(Mathf.Lerp(start.x, dest.x, i), Mathf.Lerp(start.y, dest.y, i), 0f);
                i += 10f * Time.deltaTime; // 0.1s
                yield return null;
            }

            transform.position = new Vector3(dest.x, dest.y, 0f);

            CheckItem();
            SceneController.Tick();
        } else {
            _audioSource.PlayOneShot(Resources.Load<AudioClip>("Sounds/playerMoveFail"));
            Vector2 deltaP = new Vector2((start.x - dest.x)/4f, (start.y - dest.y)/4f); // short move (16px)
            float i = 0f;
            while (i < 1f) {
                transform.position = new Vector3(Mathf.Lerp(start.x, (start.x - deltaP.x), i), Mathf.Lerp(start.y, (start.y - deltaP.y), i), 0f);
                i += 20f * Time.deltaTime; // 0.05s
                yield return null;
            }
            // play bad move sound
            _sprite.color = new Vector4(0.3f, 0.3f, 0.3f, 1f);
            i = 0f;
            while (i < 1f) {
                transform.position = new Vector3(Mathf.Lerp((start.x - deltaP.x), start.x, i), Mathf.Lerp((start.y - deltaP.y), start.y, i), 0f);
                i += 20f * Time.deltaTime; // 0.05s
                yield return null;
            }
            transform.position = new Vector3(start.x, start.y, 0f);
            yield return new WaitForSeconds(0.05f);
            _sprite.color = new Vector4(1f, 1f, 1f, 1f);
            canAct = true;
        }
    }

    private IEnumerator AnimatePhase(Vector2 dest, int phaseType) { 
        // 0 = success (phase consumed), 1 = success (no phase), 2 = fail (phase through unphasable object), 3 = fail (phase through blocked door)
        // TO DO, need a few animations

        // Note that dest is a change of 1.28 px (0.64 = 1/2, 0.32 = 1/4)
        canAct = false;
        Vector2 start = transform.position;
        float i = 0f;

        Vector2 delta32 = new Vector2((dest.x - start.x)/4f, (dest.y - start.y)/4f); // 32 pixel move
        switch (phaseType) {
            case 0: // success, phase consumed
                _audioSource.PlayOneShot(Resources.Load<AudioClip>("Sounds/playerPhaseThrough")); // play phase sound
                // 1) move 0.32px forward in 0.2s
                i = 0f;
                while (i < 1f) {
                    transform.position = new Vector3(Mathf.Lerp(start.x, (start.x + delta32.x), i), Mathf.Lerp(start.y, (start.y + delta32.y), i), 0f);
                    i += 5f * Time.deltaTime; // 0.2s
                    yield return null;
                }
                
                // 2) wait for 0.5s
                yield return new WaitForSeconds(0.5f); 

                // 3) move remaining distance to destination in 0.1s
                i = 0f;
                while (i < 1f) {
                    transform.position = new Vector3(Mathf.Lerp((start.x + delta32.x), dest.x, i), Mathf.Lerp((start.y + delta32.y), dest.y, i), 0f);
                    i += 10f * Time.deltaTime; // 0.1s
                    yield return null;
                }
                transform.position = new Vector3(dest.x, dest.y, 0f); // make sure you land on the center of the tile (sometimes gets messed up due to funky math)
                
                // 4) play phase success animation
                // need phase success animation
                yield return new WaitForSeconds(0.2f);
                phasing = false;
                _anim.SetBool("phasing", phasing);

                // 5) decrement curPhases, tick scene controller, check for items
                curPhases--;
                SceneController._ui.UpdatePhaseCounter(curPhases);
                Debug.Log($"Phases Remaining: {curPhases}");
                CheckItem();
                SceneController.Tick();

                break;
            case 1: // success, no phase consumed
                // 1) play sound and move to destination in 0.25s
                _audioSource.PlayOneShot(Resources.Load<AudioClip>("Sounds/playerPhaseNothing"));
                i = 0f;
                while (i < 1f) {
                    transform.position = new Vector3(Mathf.Lerp(start.x, dest.x, i), Mathf.Lerp(start.y, dest.y, i), 0f);
                    i += 4f * Time.deltaTime; // 0.25s
                    yield return null;
                }
                transform.position = new Vector3(dest.x, dest.y, 0f); // make sure you land on the center of the tile (sometimes gets messed up due to funky math)

                // 2) unset phasing and anim stuff
                yield return new WaitForSeconds(0.1f);
                phasing = false;
                _anim.SetBool("phasing", phasing);

                // 3) tick scene controller, check for items
                CheckItem();
                SceneController.Tick();
                break;
            case 2: // fail, phase through unphasable object
                
                // 1) move 0.32px forward in 0.2s
                i = 0f;
                while (i < 1f) {
                    transform.position = new Vector3(Mathf.Lerp(start.x, (start.x + delta32.x), i), Mathf.Lerp(start.y, (start.y + delta32.y), i), 0f);
                    i += 5f * Time.deltaTime; // 0.2s
                    yield return null;
                }

                // 2) wait for 0.5s
                yield return new WaitForSeconds(0.5f);

                // 3) move forward 0.32px in 0.05s
                i = 0f;
                while (i < 1f) {
                    transform.position = new Vector3(Mathf.Lerp((start.x + delta32.x), (start.x + (delta32.x * 2f)), i), Mathf.Lerp((start.y + delta32.y), (start.y + (delta32.y * 2f)), i), 0f);
                    i += 20f * Time.deltaTime; // 0.05s
                    yield return null;
                }

                // 4) play sound, update animator, and change sprite color to red
                // play sound
                _audioSource.PlayOneShot(Resources.Load<AudioClip>("Sounds/playerPhaseWall"));
                phasing = false;
                _anim.SetBool("phasing", phasing);
                _sprite.color = new Vector4(0.3f, 0.3f, 0.3f, 1f);

                // 5) move backward to start in 0.05s
                i = 0f;
                while (i < 1f) {
                    transform.position = new Vector3(Mathf.Lerp((start.x + (delta32.x * 2f)), start.x, i), Mathf.Lerp((start.y + (delta32.y * 2f)), start.y, i), 0f);
                    i += 20f * Time.deltaTime; // 0.05s
                    yield return null;
                }
                transform.position = new Vector3(start.x, start.y, 0f); // make sure you land on the center of the tile (sometimes gets messed up due to funky math)

                // 6) play fail animation and revert sprite color
                // need animation
                yield return new WaitForSeconds(0.2f);
                _sprite.color = new Vector4(1f, 1f, 1f, 1f);

                canAct = true;
                break;
            case 3: // fail, phase through blocked door
                // 1) move 0.32px forward in 0.2s
                i = 0f;
                while (i < 1f) {
                    transform.position = new Vector3(Mathf.Lerp(start.x, (start.x + delta32.x), i), Mathf.Lerp(start.y, (start.y + delta32.y), i), 0f);
                    i += 5f * Time.deltaTime; // 0.2s
                    yield return null;
                }

                // 2) wait for 0.3s
                yield return new WaitForSeconds(0.3f);

                // 3) move forward 0.32px in 0.05s
                i = 0f;
                while (i < 1f) {
                    transform.position = new Vector3(Mathf.Lerp((start.x + delta32.x), (start.x + (delta32.x * 2f)), i), Mathf.Lerp((start.y + delta32.y), (start.y + (delta32.y * 2f)), i), 0f);
                    i += 20f * Time.deltaTime; // 0.05s
                    yield return null;
                }

                // 4) play sound and shock animation for 0.6s
                for (int j = 0; j < 3; j++) {
                    _audioSource.PlayOneShot(Resources.Load<AudioClip>("Sounds/playerPhaseDoorFail"));
                    _sprite.color = new Vector4(0f, 1f, 1f, 1f); // cyan
                    yield return new WaitForSeconds(0.1f);
                    _sprite.color = new Vector4(1f, 1f, 1f, 1f);
                    yield return new WaitForSeconds(0.1f);
                } // 0.2s per iteration for 3 interations, total of 0.6s
                _audioSource.PlayOneShot(Resources.Load<AudioClip>("Sounds/playerMoveFail"));

                // 5) update animator, move backward to start in 0.05s
                phasing = false;
                _anim.SetBool("phasing", phasing);
                _sprite.color = new Vector4(0.3f, 0.3f, 0.3f, 1f);
                i = 0f;
                while (i < 1f) {
                    transform.position = new Vector3(Mathf.Lerp((start.x + (delta32.x * 2f)), start.x, i), Mathf.Lerp((start.y + (delta32.y * 2f)), start.y, i), 0f);
                    i += 20f * Time.deltaTime; // 0.05s
                    yield return null;
                }
                transform.position = new Vector3(start.x, start.y, 0f); // make sure you land on the center of the tile (sometimes gets messed up due to funky math)

                // 6) play shock recover animation for 0.75s
                // need animation
                yield return new WaitForSeconds(0.75f);
                _sprite.color = new Vector4(1f, 1f, 1f, 1f);

                canAct = true;
                break;
        }
    }

    private IEnumerator AnimateOutOfPhases() {
        // TO DO, need proper animation
        canAct = false;
        _audioSource.PlayOneShot(Resources.Load<AudioClip>("Sounds/playerOutOfPhases"));
        for (int i = 0; i < 3; i++) {
            _sprite.color = new Vector4(1f, 0.1f, 1f, 0.5f); // magenta
            yield return new WaitForSeconds(0.1f);
            _sprite.color = new Vector4(1f, 1f, 1f, 1f);
            yield return new WaitForSeconds(0.1f);
        }
        canAct = true;
    }

    public void FinishDeathAnim() {
        gameObject.SetActive(false);
        SceneController.RestartLevel();
    }
}
