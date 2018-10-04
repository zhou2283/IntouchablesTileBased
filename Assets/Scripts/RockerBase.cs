using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RockerBase : MonoBehaviour {

	public Transform connectedSlider;
	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}
	
	public void Interact(bool isToMax = true)
	{
		if (isToMax)
		{
			connectedSlider.GetComponent<SliderBase>().MoveToMax();
		}
		else
		{
			connectedSlider.GetComponent<SliderBase>().MoveToMin();
		}
	}
}
