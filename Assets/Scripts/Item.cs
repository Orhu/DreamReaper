using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Item : MonoBehaviour {
    [SerializeField] int itemNum = 0;

    public int PickupItem() {
        return itemNum;
    }
}
