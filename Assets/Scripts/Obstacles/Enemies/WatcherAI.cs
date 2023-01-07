using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WatcherAI : MonoBehaviour, IObstacle, IUpdateOnTick {
    public bool phasable {get; private set;} = false;
    public int type {get; private set;} = 2; // type key: 0 = wall, 1 = door, 2 = enemy, 3 = prop
    

    private float horizontal = 0f;
    private float vertical = 0f;


    private BoxCollider2D hitbox;

    [SerializeField] int facing = 0; // 0 = up, 1 = right, 2 = down, 3 = left
    [SerializeField] SpriteRenderer watcherLight;
    
    public float originalY;

    private static float ls = 3f;
    
    private Vector2 verticalHit = new Vector2(.64f, ls);
    private Vector2 horizontHit = new Vector2(ls, .64f);

    private GameObject spriteChild;

    // Start is called before the first frame update
    void Start()
    {
        spriteChild = this.transform.GetChild(0).gameObject;
        originalY = spriteChild.transform.position.y;
        hitbox = GetComponent<BoxCollider2D>();
        switch (facing) {
            case 0:
                vertical = 1f;
                horizontal = 0f;
                watcherLight.size = verticalHit;
                break;
            case 1: 
                vertical = 0f;
                horizontal = 1f;
                watcherLight.size = horizontHit;
                break;
            case 2:
                vertical = -1f;
                horizontal = 0f;
                watcherLight.size = verticalHit;
                break;
            case 3:
                vertical = 0f;
                horizontal = -1f;
                watcherLight.size = horizontHit;
                break;
        }
    }

    // Update is called once per frame
    void Update()
    {   
        Vector2 floatY = spriteChild.transform.position;                    //bobbing motion
        floatY.y = originalY + (Mathf.Sin(Time.time) * .05f);
        spriteChild.transform.position = floatY;
    }

    public void OnTick(){
        switch (facing) {
            case 0: //switch to facing right
                vertical = 0f;
                horizontal = 1f;
                facing = 1;
                hitbox.size = horizontHit;
                break;
            case 1:     //switch to facing down
                vertical = -1f;
                horizontal = 0f;
                facing = 2;
                hitbox.size = verticalHit;
                break;
            case 2:     //switch to facing left
                vertical = 0f;
                horizontal = -1f;
                facing = 3;
                hitbox.size = horizontHit;
                break;
            case 3:     //switch to facing up
                vertical = 1f;
                horizontal = 0f;
                facing = 0;
                hitbox.size = verticalHit;
                break;
        }
        watcherLight.transform.Rotate(Vector3.back * 90);
        watcherLight.transform.localPosition = new Vector2(horizontal, vertical);
        hitbox.offset = new Vector2(horizontal, vertical);
    }

    public bool IsPhasable() {
        return phasable;
    }
    public int GetObsType() {
        return type;
    }
}
