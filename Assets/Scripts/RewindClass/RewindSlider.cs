using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class RewindSlider : RewindObjectBase {

	List<int> currentLengthHistory = new List<int>();
	private SliderBase sliderBase;
	private Transform _connectedItem;

	private void Start()
	{
		sliderBase = transform.GetComponent<SliderBase>();
		_connectedItem = sliderBase.connectedItem.transform;
	}

	public override void Record()
	{
		if(currentLengthHistory.Count >= MaxLength)
		{
			currentLengthHistory.RemoveAt(MaxLength - 1);
		}
		currentLengthHistory.Insert(0, sliderBase.currentLength);
	}

	public override void Rewind(float rewindTime)
	{
		//if it is rewinding
		if (isRewinding)
		{
			//ignore all
			return;
		}
		//if there is nothing to rewind
		if (currentLengthHistory.Count == 0) 
		{
			//nothing to rewind, return
			return;
		}
		
		//change back currentLength
		sliderBase.currentLength = currentLengthHistory[0];
		//calculate the distance difference
		var yDist = Mathf.Abs(_connectedItem.position.y - sliderBase.GetConnectedItemPosition().y);
		var xDist = Mathf.Abs(_connectedItem.position.x - sliderBase.GetConnectedItemPosition().x);
		//enable isRewinding
		EnableIsRewinding();
		//initialize sequence
		var rewindSequence = DOTween.Sequence();
		//if it is not moved
		if (yDist+xDist <= 0.001f)
		{
			//tween the transform and delay to disable isRewinding
			rewindSequence.AppendInterval(rewindTime)
				.AppendCallback(DisableIsRewinding);
			//remove history
			currentLengthHistory.RemoveAt(0);
			return;
		}
		//if it is moved
		var yTimeFactor = yDist / (xDist + yDist);
		var xTimeFactor = xDist / (xDist + yDist);
		//tween the transform and delay to disable isRewinding
		rewindSequence.Append(_connectedItem.DOMoveY(sliderBase.GetConnectedItemPosition().y, yTimeFactor * rewindTime).SetEase(Ease.Linear))
			.Append(_connectedItem.DOMoveX(sliderBase.GetConnectedItemPosition().x, xTimeFactor * rewindTime).SetEase(Ease.Linear))
			.AppendCallback(DisableIsRewinding);
		
		//remove history
		currentLengthHistory.RemoveAt(0);
	}
}
