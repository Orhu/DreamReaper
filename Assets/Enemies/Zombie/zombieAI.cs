using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class zombieAI : MonoBehaviour
{

    public float horizontal;
    public float vertical;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        OnTick();
    }

    void OnTick()
    {
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
}
