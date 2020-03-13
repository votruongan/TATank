using UnityEngine;

public class AnExplosion : DelayedSelfDestroy {
    protected void OnEnable() {
        base.OnEnable();
        // this.GetComponent<AudioSource>().Play();        
    }
}