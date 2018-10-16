using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameControlSingleton : UnitySingleton<GameControlSingleton>
{

	public int TOTAL_LEVEL_NUM = 150;

	public Sprite photoMDefault;
	public Sprite photoCDefault;
	public Sprite[] photoMArray;// mono photo array
	public Sprite[] photoCArray;// colored photo array
	
	// Use this for initialization
	private void Awake()
	{
		PreLoadLevelPhotos();
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	public void InitializeSingleton()
	{
		
	}

	void PreLoadLevelPhotos()
	{
		photoMDefault = Resources.Load<Sprite>("LevelPhotos/Level_Default_M");
		photoCDefault = Resources.Load<Sprite>("LevelPhotos/Level_Default_C");
		photoMArray = new Sprite[TOTAL_LEVEL_NUM];
		photoCArray = new Sprite[TOTAL_LEVEL_NUM];
		for (int i = 0; i < TOTAL_LEVEL_NUM; i++)
		{
			Sprite photoM = Resources.Load<Sprite>("LevelPhotos/Level_" + i.ToString() + "_M");
			if (photoM != null)
			{
				photoMArray[i] = photoM;
			}
			else
			{
				photoMArray[i] = photoMDefault;
			}
			
			Sprite photoC = Resources.Load<Sprite>("LevelPhotos/Level_" + i.ToString() + "_C");
			if (photoC != null)
			{
				photoCArray[i] = photoC;
			}
			else
			{
				photoCArray[i] = photoCDefault;
			}
		}
	}
}
