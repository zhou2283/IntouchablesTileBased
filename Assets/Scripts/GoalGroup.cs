using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GoalGroup : MonoBehaviour
{
	public int reachedGoalNum = 0;

	public bool levelIsCleared = false;
	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		if (reachedGoalNum == 2 && !levelIsCleared)
		{
			levelIsCleared = true;
			LevelCleared();
		}
	}

	void LevelCleared()
	{
		print("Level Cleared");
	}


}
