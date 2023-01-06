using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ZombieAI : MonoBehaviour, IObstacle {
    public bool phasable {get; private set;} = false;
    public int type {get; private set;} = 2; // type key: 0 = wall, 1 = door, 2 = enemy, 3 = prop

    [SerializeField] int facing = 0; // 0 = up, 1 = right, 2 = down, 3 = left

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
        OnTick();
    }

    void OnTick() {
        Vector2 currentCoord = transform.position;
        Vector2 nextCoord;
        nextCoord.x = currentCoord.x + .64f*horizontal;
        nextCoord.y = currentCoord.y + .64f*vertical;

        Debug.Log(nextCoord);
        Debug.Log(currentCoord);


        Collider2D overlapCheck = Physics2D.OverlapPoint(nextCoord);
        if (overlapCheck != null){
            horizontal = -horizontal;
            vertical = -vertical;
        }
        else{
            transform.position = nextCoord;
        }

    }
    
    public bool IsPhasable() {
        return phasable;
    }
    public int GetObsType() {
        return type;
    }
}
