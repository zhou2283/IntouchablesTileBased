using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoxGroup : MonoBehaviour {


	Transform playerLight;
	Transform playerDark;

    // Use this for initialization
    void Start () {
	    playerLight = GameObject.Find("PlayerLight").transform;
	    playerDark = GameObject.Find("PlayerDark").transform;
	}
	
	// Update is called once per frame
	void Update () {
		
	}
	
	public void InitializeBoxGroupAndPlayer()
	{
		foreach (Transform child in transform)
		{
			child.GetComponent<BoxBase>().visited = false;
			child.GetComponent<BoxBase>().needMove = false;
		}
		playerDark.GetComponent<PlayerBase>().visited = false;
		playerDark.GetComponent<PlayerBase>().needMove = false;
		playerLight.GetComponent<PlayerBase>().visited = false;
		playerLight.GetComponent<PlayerBase>().needMove = false;
	}

	public void MoveAllBoxAndPlayer()
	{
		foreach (Transform child in transform)
		{
			if (child.GetComponent<BoxBase>().needMove)
			{
				child.GetComponent<BoxBase>().MoveBox();
			}
		}
		if (playerDark.GetComponent<PlayerBase>().needMove)
		{
			playerDark.GetComponent<PlayerBase>().MovePlayer();
		}
		if (playerLight.GetComponent<PlayerBase>().needMove)
		{
			playerLight.GetComponent<PlayerBase>().MovePlayer();
		}
	}
}
