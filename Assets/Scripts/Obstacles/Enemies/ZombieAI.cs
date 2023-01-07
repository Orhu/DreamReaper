using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ZombieAI : MonoBehaviour, IObstacle, IUpdateOnTick  {
    public bool phasable {get; private set;} = false;
    public int type {get; private set;} = 2; // type key: 0 = wall, 1 = door, 2 = enemy, 3 = prop

    [SerializeField] int facing = 0; // 0 = up, 1 = right, 2 = down, 3 = left
    [SerializeField] LayerMask PlayerMask;

    private float horizontal = 0f;
    private float vertical = 0f;

    // Start is called before the first frame update
    void Start() {
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
    }

    public void OnTick() {
        
        Vector2 currentCoord = transform.position;
        Vector2 nextCoord;
        
        nextCoord.x = currentCoord.x + .64f*horizontal;
        nextCoord.y = currentCoord.y + .64f*vertical;

        Collider2D playerCheck = Physics2D.OverlapPoint(currentCoord, PlayerMask);
        if (playerCheck != null){
            Player pl = playerCheck.GetComponent<Player>();
            if (pl != null) {
                Debug.Log("player is dead");
                transform.position = nextCoord;
            }
        }

        Debug.Log(nextCoord);
        Debug.Log(currentCoord);

        Collider2D overlapCheck = Physics2D.OverlapPoint(nextCoord);
        if (overlapCheck != null){
            Player pl = overlapCheck.GetComponent<Player>();
            if (pl != null) {
                transform.position = nextCoord;
            }
            else{
                horizontal = -horizontal;
                vertical = -vertical;
            }
        }
        else{
            transform.position = nextCoord;
            playerCheck = Physics2D.OverlapPoint(currentCoord, PlayerMask);
            if (playerCheck != null){
                Player pl = playerCheck.GetComponent<Player>();
                if (pl != null) {
                    Debug.Log("player is dead");
                    transform.position = nextCoord;
                }
            }
        }

    }
    
    public bool IsPhasable() {
        return phasable;
    }
    public int GetObsType() {
        return type;
    }
}
