using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class OutsideGroupControl : MonoBehaviour
{
	private int currentLevelNum;
	
	// Use this for initialization
	void Start ()
	{
		
		currentLevelNum = int.Parse(SceneManager.GetActiveScene().name.Split('_')[1]);
		//print(GameControlSingleton.Instance.photoMArray[currentLevelNum].name);
		//print(currentLevelNum);
		//Center
		transform.Find("CenterGroup").Find("PhotoGroup").Find("PhotoM").GetComponent<SpriteRenderer>().sprite =
			GameControlSingleton.Instance.photoMArray[currentLevelNum];
		transform.Find("CenterGroup").Find("PhotoGroup").Find("ColoredPhotoMaskGroup").Find("PhotoC").GetComponent<SpriteRenderer>().sprite =
			GameControlSingleton.Instance.photoCArray[currentLevelNum];
		transform.Find("CenterGroup").Find("PhotoGroup").Find("Shadow").gameObject.SetActive(true);
		//Left
		if (currentLevelNum > 0)
		{
			transform.Find("LeftGroup").Find("Photo").GetComponent<SpriteRenderer>().sprite =
				GameControlSingleton.Instance.photoCArray[currentLevelNum - 1];
		}
		else
		{
			//it is Level_0, special case
		}
		//Right
		if (currentLevelNum < GameControlSingleton.Instance.TOTAL_LEVEL_NUM - 1)
		{
			transform.Find("RightGroup").Find("Photo").GetComponent<SpriteRenderer>().sprite =
				GameControlSingleton.Instance.photoMArray[currentLevelNum + 1];
		}
		else
		{
			//it is Level_MAX, special case
		}
		//RightRight
		if (currentLevelNum < GameControlSingleton.Instance.TOTAL_LEVEL_NUM - 2)
		{
			transform.Find("RightRightGroup").Find("Photo").GetComponent<SpriteRenderer>().sprite =
				GameControlSingleton.Instance.photoMArray[currentLevelNum + 2];
		}
		else
		{
			//it is Level_MAX - 1, special case
		}
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
