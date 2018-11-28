using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ReflectionSurfaceLevelChange : MonoBehaviour
{
	private Material glassReflection;
	// Use this for initialization
	void Start ()
	{
		glassReflection = GetComponent<SpriteRenderer>().material;
		print(glassReflection.name);
	}
	
	// Update is called once per frame
	void Update () {
		//GetComponent<PIDI_2DReflection>().surfaceLevel = CalculateSurfaceLevel(transform.parent.position.y);
		//print(GetComponent<PIDI_2DReflection>().surfaceLevel);
	}

	float CalculateSurfaceLevel(float _posY)
	{
		return -_posY * 2f - 0.2f;
		//-0.2 0.2
		//-2.6 5.0
		//-3.0 5.8
	}
}
