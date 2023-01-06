using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IObstacle {
    bool phasable {get;}
    int type {get;} // type key: 0 = wall, 1 = door, 2 = enemy, 3 = prop

    bool IsPhasable();
    int GetType();
}
