using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using UnityEngine.SceneManagement;

public class RenderGroupControl : MonoBehaviour
{
	private Transform mainCamera;

	private Transform lightCaptureCameraBlur;

	private Transform outlineCamera;

	private float zoomTime = 1.5f;
	private float moveTime = 1.5f;
	private float sceneWidth = 12.8f;
	
	private AsyncOperation asyncOperation;
	private int levelIndex;
	private string levelName;
	
	// Use this for initialization
	void Start ()
	{
		mainCamera = transform.Find("Main Camera");
		lightCaptureCameraBlur = transform.Find("LightCaptureCameraBlur");
		outlineCamera = transform.Find("OutlineCamera");
		MoveIn();
	}
	
	// Update is called once per frame
	void Update () {

	}

	public void MoveIn()
	{
		//zoom in
		mainCamera.GetComponent<Camera>().DOOrthoSize(3.6f, zoomTime).SetEase(Ease.InOutCubic);
		outlineCamera.GetComponent<Camera>().DOOrthoSize(3.6f, zoomTime).SetEase(Ease.InOutCubic);
		
		//open photo
		GameObject.Find("CenterGroup").transform.Find("PhotoGroup").DOMoveX(-sceneWidth, zoomTime).SetEase(Ease.InOutCubic);
	}

	public void MoveToNextLevel()
	{
		//zoom out
		mainCamera.GetComponent<Camera>().DOOrthoSize(5f, zoomTime).SetEase(Ease.InOutCubic);
		//close photo
		outlineCamera.GetComponent<Camera>().DOOrthoSize(5f, zoomTime).SetEase(Ease.InOutCubic);
		GameObject.Find("CenterGroup").transform.Find("PhotoGroup").transform.DOMoveX(0, zoomTime).SetEase(Ease.InOutCubic);
		//color photo
		GameObject.Find("GoalLight").transform.Find("PhotoMask").DOScale(10, zoomTime).SetDelay(1.5f);
		GameObject.Find("GoalDark").transform.Find("PhotoMask").DOScale(10, zoomTime).SetDelay(1.5f);
		
		GameObject.Find("GoalLight").transform.Find("PhotoMask").Find("PhotoMaskExplosion").gameObject.SetActive(true);
		GameObject.Find("GoalDark").transform.Find("PhotoMask").Find("PhotoMaskExplosion").gameObject.SetActive(true);

		mainCamera.DOShakeRotation(1f, 2f,10).SetDelay(1.5f);
		outlineCamera.DOShakeRotation(1f, 2f,10).SetDelay(1.5f);
		
		//move to the next scene
		mainCamera.DOMoveX(sceneWidth, moveTime).SetEase(Ease.InOutCubic).SetDelay(zoomTime);
		outlineCamera.DOMoveX(sceneWidth, moveTime).SetEase(Ease.InOutCubic).SetDelay(zoomTime).OnComplete(ActiveNextLevel);
	}
	
	public void MoveBack()
	{
		//zoom out
		mainCamera.GetComponent<Camera>().DOOrthoSize(5f, zoomTime).SetEase(Ease.InOutCubic);
		//close photo
		outlineCamera.GetComponent<Camera>().DOOrthoSize(5f, zoomTime).SetEase(Ease.InOutCubic);
		GameObject.Find("CenterGroup").transform.Find("PhotoGroup").transform.DOMoveX(0, zoomTime).SetEase(Ease.InOutCubic);
	}

	public void LoadNextLevel()
	{
		levelIndex = int.Parse(SceneManager.GetActiveScene().name.Split('_')[1]) + 1;
		levelName = "Level_" + levelIndex.ToString();
		if (Application.CanStreamedLevelBeLoaded(levelName))
		{
			
			asyncOperation = SceneManager.LoadSceneAsync(levelName);
			asyncOperation.allowSceneActivation = false;
		}
		else
		{
			Debug.Log("Scene: " + levelName + " --- dose not exist");
		}
	}

	void ActiveNextLevel()
	{
		levelIndex = int.Parse(SceneManager.GetActiveScene().name.Split('_')[1]) + 1;
		levelName = "Level_" + levelIndex.ToString();
		SceneManager.LoadScene(levelName);
	}
}
