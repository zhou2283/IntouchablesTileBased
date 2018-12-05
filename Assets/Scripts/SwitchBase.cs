using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwitchBase : MonoBehaviour {

    public Transform connectedLight;
	
	
	//FMOD
	public string switchSound = "event:/Interactable/SwitchSound";
	
	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}
    
    public void Interact()
    {
	    if (connectedLight.GetComponent<Light2DBaseControl>().LightSwitch())
	    {
		    GameControlSingleton.Instance.PlayOneShotSound(switchSound);
	    }
    }
}
