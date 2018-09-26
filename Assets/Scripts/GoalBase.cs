using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GoalBase : MonoBehaviour {

	
	public enum GoalType
	{
		LightPlayerOnly,
		DarkPlayerOnly
	}
	public GoalType goalType;
	private string targetTag;
	private GoalGroup goalGroupScript;

	private void Start()
	{
		if (goalType == GoalType.LightPlayerOnly)
		{
			targetTag = "PlayerLight";
		}
		else if (goalType == GoalType.DarkPlayerOnly)
		{
			targetTag = "PlayerDark";
		}

		goalGroupScript = transform.parent.GetComponent<GoalGroup>();
	}


	private void OnTriggerEnter2D(Collider2D col)
	{
		if (col.CompareTag(targetTag))
		{
			goalGroupScript.reachedGoalNum++;
		}		
	}

	private void OnTriggerExit2D(Collider2D col)
	{
		if (col.CompareTag(targetTag))
		{
			goalGroupScript.reachedGoalNum--;
		}	
	}
}
