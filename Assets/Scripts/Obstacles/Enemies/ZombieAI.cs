using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine;

public class ZombieAI : MonoBehaviour, IObstacle, IEnemy {
    public bool phasable {get; private set;} = false;
    public int type {get; private set;} = 2; // type key: 0 = wall, 1 = door, 2 = enemy, 3 = prop

    [SerializeField] int facing = 0; // 0 = up, 1 = right, 2 = down, 3 = left
    [SerializeField] LayerMask playerMask;
    [SerializeField] LayerMask obstacleLayerMask;

    private BoxCollider2D _box;
    private Animator _anim;

    private float horizontal = 0f;
    private float vertical = 0f;

    private bool playerCaught = false;
    private bool checking = true;

    // Start is called before the first frame update
    void Start() {
        _box = GetComponent<BoxCollider2D>();
        _anim = GetComponent<Animator>();
        _anim.SetInteger("facing", facing);
        switch (facing) {
            case 0:
                vertical = 1f;
                horizontal = 0f;
                break;
            case 1: 
                vertical = 0f;
                horizontal = 1f;
                break;
            case 2:
                vertical = -1f;
                horizontal = 0f;
                break;
            case 3:
                vertical = 0f;
                horizontal = -1f;
                break;
        }
    }

    // Update is called once per frame
    void Update() {
        if (!playerCaught && gameObject.activeSelf && SceneController.allowKills) {
            if (checking) {
                if (CheckForPlayer()){
                    StopAllCoroutines();
                    StartCoroutine(DeathOfPlayer()); // Calls function that waits two seconds then resets the level
                }
            }
        }
    }

    public void OnTick() {
        if (gameObject.activeSelf) {
            ZombieMove();
        }
    }

    private bool CheckForPlayer() {
        Collider2D col = Physics2D.OverlapPoint(transform.position, playerMask);
        if (col != null) {
            playerCaught = true;
            Debug.Log("Gotcha!");
        }
        return (col != null);
    }

    private void ZombieMove() {
        Vector3 currentCoord = transform.position;
        Vector2 nextCoord = new Vector3(currentCoord.x + .64f * horizontal, currentCoord.y + .64f * vertical);

        Debug.Log(currentCoord);
        Debug.Log(nextCoord);

        Collider2D overlapCheck = Physics2D.OverlapPoint(nextCoord, obstacleLayerMask);
        if (overlapCheck != null) { // need to change directions
            facing = (facing + 2) % 4;
            _anim.SetInteger("facing", facing);
            horizontal = -horizontal;
            vertical = -vertical;
            nextCoord.x = currentCoord.x + .64f*horizontal;
            nextCoord.y = currentCoord.y + .64f*vertical;
        }
        _anim.SetTrigger("tick");
        Vector3 dest = new Vector3(nextCoord.x, nextCoord.y, transform.position.z);
        StartCoroutine(AnimateZombieMove(dest));
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

    // Animation stuff
    private IEnumerator AnimateZombieMove(Vector3 dest) {
        // TO DO
        Vector2 start = transform.position;
        
        yield return new WaitForSeconds(1f/6f); // wait for start of animation
        checking = false;
        float i = 0f;
        while (i < 1f) {
            transform.position = new Vector3(Mathf.Lerp(start.x, dest.x, i), Mathf.Lerp(start.y, dest.y, i), 0f);
            i += 6f * Time.deltaTime;
            yield return null;
        }

        transform.position = new Vector3(dest.x, dest.y, 0f);
        checking = true;
        yield return null;
        SceneController.EnemyMoveDone();
    }

    private IEnumerator DeathOfPlayer(){
        SceneController.PlayerCaught();
        yield return new WaitForSeconds(1f);
        SceneController.RestartLevel();
    }
}
