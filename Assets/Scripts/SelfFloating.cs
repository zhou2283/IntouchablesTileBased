using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class SelfFloating : MonoBehaviour
{

	public float radius = 0.02f;

	private float durationCount = 1f;
	private float duration = 1f;

	private Vector3 localPos;
	// Use this for initialization
	void Start ()
	{
		localPos = transform.localPosition;
	}
	
	// Update is called once per frame
	void Update () {
		if (durationCount > duration)
		{
			transform.DOKill();
			durationCount = 0;
			Vector2 randomV2 = Random.insideUnitCircle * radius;
			Vector3 randomV3 = new Vector3(randomV2.x,randomV2.y,0) + localPos;
			transform.DOLocalMove(randomV3, duration).SetEase(Ease.InOutCubic);
		}
		durationCount += Time.deltaTime;

		transform.localRotation = Quaternion.Euler(new Vector3(90f + 10f * Mathf.Sin(Time.time), -90f, 90f));
	}
}
