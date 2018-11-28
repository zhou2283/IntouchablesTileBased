using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class GoalBase : MonoBehaviour {

	
	public enum GoalType
	{
		LightPlayerOnly,
		DarkPlayerOnly
	}
	
	
	
	public GoalType goalType;
	private string targetTag;
	private GoalGroup goalGroupScript;

	private Transform anotherGoal;


	public float stayTime = 0.2f;
	float stayTimeCount = 0f;
	private bool isStay = false;

	public Transform readyEffectGroup;
	

	private void Start()
	{
		if (goalType == GoalType.LightPlayerOnly)
		{
			targetTag = "PlayerLight";
			anotherGoal = transform.parent.Find("GoalDark");
		}
		else if (goalType == GoalType.DarkPlayerOnly)
		{
			targetTag = "PlayerDark";
			anotherGoal = transform.parent.Find("GoalLight");
		}

		goalGroupScript = transform.parent.GetComponent<GoalGroup>();
		readyEffectGroup = transform.Find("ReadyEffectGroup");
	}


	private void OnTriggerEnter2D(Collider2D col)
	{
		if (col.CompareTag(targetTag))
		{
			goalGroupScript.reachedGoalNum++;
			stayTimeCount = 0f;
			isStay = false;

			if (goalGroupScript.reachedGoalNum == 1)
			{
				NotGotEffect();
				anotherGoal.GetComponent<GoalBase>().NotGotEffect();
			}
		}		
	}

	private void OnTriggerStay2D(Collider2D col)
	{
		if (col.CompareTag(targetTag) && !isStay)
		{
			stayTimeCount += Time.fixedDeltaTime;
			if (stayTimeCount >= stayTime)
			{
				isStay = true;
				EnableReadyEffect();
			}
		}		
	}

	private void OnTriggerExit2D(Collider2D col)
	{
		if (col.CompareTag(targetTag))
		{
			stayTimeCount = 0;
			isStay = false;
			DisableReadyEffect();
			goalGroupScript.reachedGoalNum--;
		}	
	}

	void EnableReadyEffect()
	{
		foreach (Transform child in readyEffectGroup)
		{
			var _em = child.GetComponent<ParticleSystem>().emission;
			_em.enabled = true;
		}
	}

	void DisableReadyEffect()
	{
		foreach (Transform child in readyEffectGroup)
		{
			var _em = child.GetComponent<ParticleSystem>().emission;
			_em.enabled = false;
		}
	}

	public void LevelClearBlink()
	{
		var _ps = transform.Find("BlinkEffectGroup").Find("GoalBlinkParticle").GetComponent<ParticleSystem>();
		var _main = _ps.main;
		var _em = _ps.emission;
		_main.startSize = 2.5f;
		_ps.Emit(1);
		_em.rateOverTime = 4f;
		var _psM = transform.Find("BlinkEffectGroup").Find("GoalBlinkMaskParticle").GetComponent<ParticleSystem>();
		var _mainM = _psM.main;
		var _emM = _psM.emission;
		_mainM.startSize = 2.5f;
		_psM.Emit(1);
		_emM.rateOverTime = 4f;
	}

	public void NotGotEffect()
	{
		var _ps = transform.Find("BlinkEffectGroup").Find("GoalBlinkParticle").GetComponent<ParticleSystem>();
		//var _em = _ps.emission;
		_ps.Emit(1);
		//_em.rateOverTime = 0.5f;
	}
}
