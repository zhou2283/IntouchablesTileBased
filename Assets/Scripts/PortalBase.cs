using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PortalBase : MonoBehaviour {

	protected RewindControl rewindControlScript;
	public Transform connectedPortal;

	private int count = 0;
	public bool isEmpty = true;
	

	// Use this for initialization
	void Start () {
		rewindControlScript = GameObject.Find("RewindControl").GetComponent<RewindControl>();
	}
	
	// Update is called once per frame
	void Update () {
		
		if (count > 0)
		{
			isEmpty = false;
		}
		else if (count == 0)
		{
			isEmpty = true;
		}
		if (Input.GetKeyDown(KeyCode.Y))
		{
			print(count);
		}
	}
	
	public void Interact()
	{
		
	}

	private void OnTriggerEnter2D(Collider2D col)
	{
		count++;
	}

	private void OnTriggerExit2D(Collider2D col)
	{
		count--;
	}
}
