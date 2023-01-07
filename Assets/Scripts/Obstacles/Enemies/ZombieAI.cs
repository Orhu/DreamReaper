using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ZombieAI : MonoBehaviour, IObstacle, IUpdateOnTick  {
    public bool phasable {get; private set;} = false;
    public int type {get; private set;} = 2; // type key: 0 = wall, 1 = door, 2 = enemy, 3 = prop

    [SerializeField] int facing = 0; // 0 = up, 1 = right, 2 = down, 3 = left
    [SerializeField] LayerMask playerMask;
    [SerializeField] LayerMask obstacleLayerMask;

    private BoxCollider2D _box;

    private float horizontal = 0f;
    private float vertical = 0f;

    private bool playerCaught = false;

    // Start is called before the first frame update
    void Start() {
        _box = GetComponent<BoxCollider2D>();
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
        if (!playerCaught) {
            CheckForPlayer();
        }
    }

    public void OnTick() {
        ZombieMove();
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
        Vector2 currentCoord = transform.position;
        Vector2 nextCoord = new Vector2(currentCoord.x + .64f * horizontal, currentCoord.y + .64f * vertical);

        Debug.Log(currentCoord);
        Debug.Log(nextCoord);

        Collider2D overlapCheck = Physics2D.OverlapPoint(nextCoord, obstacleLayerMask);
        if (overlapCheck != null) {
            horizontal = -horizontal;
            vertical = -vertical;
            nextCoord.x = currentCoord.x + .64f*horizontal;
            nextCoord.y = currentCoord.y + .64f*vertical;
        }
        transform.position = nextCoord;
    }
     
    public bool IsPhasable() {
        return phasable;
    }
    public int GetObsType() {
        return type;
    }
}
