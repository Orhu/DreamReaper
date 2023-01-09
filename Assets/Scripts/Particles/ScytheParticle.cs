using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScytheParticle : MonoBehaviour {
    public void Kill() {
        Destroy(this.gameObject);
    }
}
