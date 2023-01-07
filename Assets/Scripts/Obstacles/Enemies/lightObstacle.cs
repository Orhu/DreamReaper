using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class lightObstacle : MonoBehaviour, IObstacle, IEnemy{
    public bool phasable {get; private set;} = true;
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

    public void Kill() {
        gameObject.SetActive(false);
    }
}
