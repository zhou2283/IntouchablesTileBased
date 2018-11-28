using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RuneBlink : MonoBehaviour
{

	public float minAlpha = 0.2f;

	public float maxAlpha = 1f;

	public float minAlphaLight = 0.05f;

	public float maxAlphaLight = 0.1f;

	public float minSize = 0.1f;

	public float maxSize = 0.2f;

	public float speed = 5f;

	private SpriteRenderer runeSR;
	private SpriteRenderer lightSR;
	
	// Use this for initialization
	void Start ()
	{
		runeSR = GetComponent<SpriteRenderer>();
		lightSR = transform.Find("RuneLight").GetComponent<SpriteRenderer>();
	}
	
	// Update is called once per frame
	void Update ()
	{
		var _color = runeSR.color;
		runeSR.color = new Color(_color.r, _color.g, _color.b, (Mathf.Sin(Time.time * speed) + 1)/2f * (maxAlpha - minAlpha) + minAlpha);
		
		var _lightColor = lightSR.color;
		lightSR.color = new Color(_lightColor.r, _lightColor.g, _lightColor.b, (Mathf.Sin(Time.time * speed) + 1)/2f * (maxAlphaLight - minAlphaLight) + minAlphaLight);
		lightSR.transform.localScale =
			Vector3.one * ((Mathf.Sin(Time.time * speed) + 1) / 2f * (maxSize - minSize) + minSize);

	}
}
