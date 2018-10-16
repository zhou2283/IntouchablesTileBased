using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GoalGroup : MonoBehaviour
{
	public int reachedGoalNum = 0;

	public bool levelIsCleared = false;

	private AsyncOperation asyncOperation;
	private int levelIndex;
	private string levelName;

	private Transform goalLight;
	private Transform goalDark;
	
	// Use this for initialization
	void Start ()
	{
		goalLight = transform.Find("GoalLight");
		goalDark = transform.Find("GoalDark");
	}
	
	// Update is called once per frame
	void Update () {
		if (Input.GetKeyDown(KeyCode.M))
		{
			LevelCleared();
		}
		
		if (reachedGoalNum == 2 && !levelIsCleared)
		{

			LevelCleared();
		}
	}

	void LevelCleared()
	{
		levelIsCleared = true;
		goalLight.GetComponent<GoalBase>().LevelClearBlink();
		goalDark.GetComponent<GoalBase>().LevelClearBlink();
		GameObject.Find("RenderGroup").GetComponent<RenderGroupControl>().MoveToNextLevel();
	}
	


}
