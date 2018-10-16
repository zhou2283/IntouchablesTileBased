using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DynamicRope : MonoBehaviour
{

	public GameObject connectedItem;
	float elapsedTime = 0;
	Rope rope; 
	Transform firstSegment;
	public bool isDynamic = false;
	// Use this for initialization
	void Start () {
		if (!rope) 
		{
			rope = GetComponent<Rope> ();
			if(rope)
			{
				firstSegment = rope.transform.GetChild(0);
				firstSegment.GetComponent<Rigidbody2D>().isKinematic=true;
			}
		}

	}
	
	
	// Update is called once per frame
	// Update is called once per frame
	void Update () {
		if(rope && isDynamic)
		{
			//its better to make the first segment kinematic but it isn't required
			firstSegment.GetComponent<Rigidbody2D>().MovePosition(connectedItem.transform.position);
		}
	}
}
