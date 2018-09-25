using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwitchBase : MonoBehaviour {

    public Transform connectedLight;
	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}
    
    public void Interact()
    {
        connectedLight.GetComponent<Light2DBaseControl>().LightSwitch();
    }
}
