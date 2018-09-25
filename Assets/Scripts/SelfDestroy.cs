using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SelfDestroy : MonoBehaviour {

    public float delay = 1f;
	// Use this for initialization
	void Start () {
        StartCoroutine(DelayToDestroySelf(delay));
	}

    public IEnumerator DelayToDestroySelf(float delaySeconds)
    {
        yield return new WaitForSeconds(delaySeconds);
        Destroy(gameObject);
    }
}
