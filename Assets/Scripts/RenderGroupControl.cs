using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using UnityEngine.SceneManagement;

public class RenderGroupControl : MonoBehaviour
{
	public enum SHAKE_DIR
	{
		CENTER = 0,
		LEFT = 1,
		RIGHT = 2,
		UP = 3,
		DOWN = 4
	};

	public SHAKE_DIR shakeDir = SHAKE_DIR.CENTER;
	private SHAKE_DIR _shakeDirLastFrame = SHAKE_DIR.CENTER;
	
	private Transform mainCamera;

	private Transform lightCaptureCameraBlur;

	private Transform outlineCamera;

	private float zoomTime = 1.5f;
	private float moveTime = 1.5f;
	private float sceneWidth = 12.8f;

	private float shakeDistance = 0.06f;
	private float shakeDuration = 0.4f;
	
	private AsyncOperation asyncOperation;
	private int levelIndex;
	private string levelName;
	
	
	//FMOD
	string sceneOpenSound = "event:/SceneChange/SceneOpen";

	
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
		if (shakeDir != _shakeDirLastFrame)
		{
			Shake(shakeDir);
		}

		_shakeDirLastFrame = shakeDir;

	}

	public void MoveIn()
	{
		//zoom in
		mainCamera.GetComponent<Camera>().DOOrthoSize(3.6f, zoomTime).SetEase(Ease.InOutCubic);
		outlineCamera.GetComponent<Camera>().DOOrthoSize(3.6f, zoomTime).SetEase(Ease.InOutCubic);
		
		//open photo
		GameObject.Find("CenterGroup").transform.Find("PhotoGroup").DOMoveX(-sceneWidth, zoomTime).SetEase(Ease.InOutCubic);
		//FMOD
		GameControlSingleton.Instance.PlayOneShotSound(sceneOpenSound);
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

	public void ShakeRight()
	{
		mainCamera.transform.DOLocalMoveX(-shakeDistance, shakeDuration).SetEase(Ease.OutCubic);
		outlineCamera.transform.DOLocalMoveX(-shakeDistance, shakeDuration).SetEase(Ease.OutCubic);
	}
	
	public void ShakeUp()
	{
		mainCamera.transform.DOLocalMoveY(-shakeDistance, shakeDuration).SetEase(Ease.OutCubic);
		outlineCamera.transform.DOLocalMoveY(-shakeDistance, shakeDuration).SetEase(Ease.OutCubic);
	}
	
	public void ShakeLeft()
	{
		mainCamera.transform.DOLocalMoveX(shakeDistance, shakeDuration).SetEase(Ease.OutCubic);
		outlineCamera.transform.DOLocalMoveX(shakeDistance, shakeDuration).SetEase(Ease.OutCubic);
	}
	
	public void ShakeDown()
	{
		mainCamera.transform.DOLocalMoveY(shakeDistance, shakeDuration).SetEase(Ease.OutCubic);
		outlineCamera.transform.DOLocalMoveY(shakeDistance, shakeDuration).SetEase(Ease.OutCubic);
	}

	public void Shake(SHAKE_DIR sd)
	{
		if (sd == SHAKE_DIR.UP)
		{
			ShakeUp();
		}
		else if (sd == SHAKE_DIR.DOWN)
		{
			ShakeDown();
		}
		else if (sd == SHAKE_DIR.LEFT)
		{
			ShakeLeft();
		}
		else if (sd == SHAKE_DIR.RIGHT)
		{
			ShakeRight();
		}
		else
		{
			ShakeBack();
		}
	}
	
	public void ShakeBack()
	{
		mainCamera.transform.DOLocalMoveX(0, shakeDuration).SetEase(Ease.OutCubic);
		outlineCamera.transform.DOLocalMoveX(0, shakeDuration).SetEase(Ease.OutCubic);
		mainCamera.transform.DOLocalMoveY(0, shakeDuration).SetEase(Ease.OutCubic);
		outlineCamera.transform.DOLocalMoveY(0, shakeDuration).SetEase(Ease.OutCubic);
	}
	

	void ActiveNextLevel()
	{
		levelIndex = int.Parse(SceneManager.GetActiveScene().name.Split('_')[1]) + 1;
		levelName = "Level_" + levelIndex.ToString();
		SceneManager.LoadScene(levelName);
	}
	
	
}
