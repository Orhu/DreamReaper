using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WatcherAI : MonoBehaviour, IObstacle {
    public bool phasable {get; private set;} = false;
    public int type {get; private set;} = 2; // type key: 0 = wall, 1 = door, 2 = enemy, 3 = prop

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public bool IsPhasable() {
        return phasable;
    }
    public int GetObsType() {
        return type;
    }
}
