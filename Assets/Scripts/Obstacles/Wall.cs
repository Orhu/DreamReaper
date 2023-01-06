using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Wall : MonoBehaviour, IObstacle {
    public bool phasable {get; [SerializeField] set;} = false;
    public int type {get; [SerializeField] set;} = 0; // type key: 0 = wall, 1 = door, 2 = enemy, 3 = prop

    public bool IsPhasable() {
        return phasable;
    }

    public int GetObsType() {
        return type;
    }
}
