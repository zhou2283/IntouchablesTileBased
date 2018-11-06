using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class PlayerIndicator : MonoBehaviour
{
	private float changeDuration = 0.1f;

	private Transform basic;
	private Transform gear;
	private Transform gearBody;//this is used for blink
	private Transform arrowUp;
	private Transform arrowDown;
	private Transform arrowLeft;
	private Transform arrowRight;
	
	// Use this for initialization
	void Start ()
	{
		basic = transform.Find("Basic");
		gear = transform.Find("Gear");
		gearBody = gear.Find("GearBody");
		arrowUp = transform.Find("ArrowUp");
		arrowDown = transform.Find("ArrowDown");
		arrowLeft = transform.Find("ArrowLeft");
		arrowRight = transform.Find("ArrowRight");
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	public void ChangeToBasic()
	{
		HideAll();
		basic.DOKill();
		basic.DOScale(1, changeDuration);
	}
	
	public void ChangeToGear()
	{
		HideAll();
		gear.DOKill();
		gear.DOScale(1f, changeDuration);
	}
	
	public void GearBlink()
	{
		gearBody.DOKill();
		gearBody.DOScale(0.8f, changeDuration);
		gearBody.DOScale(1f, changeDuration).SetDelay(changeDuration);
	}

	public void ChangeToXSlider()
	{
		gear.DOKill();
		arrowLeft.DOKill();
		arrowRight.DOKill();
		gear.DOScale(0.8f, changeDuration);
		arrowLeft.DOScale(new Vector3(0.7f, 0.45f, 1f), changeDuration);
		arrowRight.DOScale(new Vector3(0.7f, 0.45f, 1f), changeDuration);
	}
	
	public void ChangeToYSlider()
	{
		gear.DOKill();
		arrowUp.DOKill();
		arrowDown.DOKill();
		transform.Find("Gear").DOScale(0.8f, changeDuration);
		arrowUp.DOScale(new Vector3(0.7f, 0.45f, 1f), changeDuration);
		arrowDown.DOScale(new Vector3(0.7f, 0.45f, 1f), changeDuration);
	}
	
	void HideAll()
	{
		foreach (Transform child in transform)
		{
			child.DOScale(0, changeDuration);
		}
	}
}
