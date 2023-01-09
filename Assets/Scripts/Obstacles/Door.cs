using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Door : MonoBehaviour, IObstacle {
    public bool phasable {get; [SerializeField] set;} = true;
    public int type {get; [SerializeField] set;} = 1; // type key: 0 = wall, 1 = door, 2 = enemy, 3 = prop
    [SerializeField] bool blocked = false;

    private Animator _anim;

    void Start() {
        if (blocked) { // update this
            phasable = false;
        }
        _anim = GetComponent<Animator>();

        _anim.SetBool("locked", blocked);
    }

    public bool IsPhasable() {
        return phasable;
    }

    public int GetObsType() {
        return type;
    }

    public bool IsBlocked() {
        return blocked;
    }

    public void Unblock() {
        blocked = false;
        phasable = true;
        _anim.SetBool("locked", blocked);
        _anim.SetTrigger("unlock");
    }
}
