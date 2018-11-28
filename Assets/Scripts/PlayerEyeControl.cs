using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class PlayerEyeControl : MonoBehaviour
{
	private Transform eyelid;

	private Transform eyeball;

	private float oriY;

	private PlayerBase playerBaseScript;

	private float blinkTimer = 0f;
	private float blinkTimeLimit = 0f;

	private bool _isActiveLastFrame = false;
	
	// Use this for initialization
	void Start ()
	{
		eyelid = transform.Find("Eyelid");
		eyeball = transform.Find("EyeballPivot").Find("Eyeball");
		oriY = transform.localPosition.y;
		playerBaseScript = transform.parent.GetComponent<PlayerBase>();
		//print(transform.parent.name);
	}
	
	// Update is called once per frame
	void Update () {

		if (Input.GetKeyDown(KeyCode.C))
		{
			ShakeEyes();
		}
		
		if (!playerBaseScript.activeSelf)
		{
			CloseEyes();
		}
		else
		{
			if (blinkTimer > blinkTimeLimit)
			{
				blinkTimer = 0;
				blinkTimeLimit = Random.Range(0.5f, 5f);
				BlinkEyes();
			}
			blinkTimer += Time.deltaTime;
		}

		if (!_isActiveLastFrame && playerBaseScript.activeSelf)
		{
			OpenEyes();
		}
		
		_isActiveLastFrame = playerBaseScript.activeSelf;
	}
	
	public void CloseEyes()
	{
		eyelid.DOKill();
		eyelid.transform.DOLocalMoveY(0.11f, 0.2f);
	}
	
	public void OpenEyes()
	{
		eyelid.DOKill();
		eyelid.transform.DOLocalMoveY(0.22f, 0.2f);
	}

	public void BlinkEyes()
	{
		eyelid.DOKill();
		eyelid.transform.DOLocalMoveY(0, 0.2f);
		eyelid.transform.DOLocalMoveY(0.22f, 0.2f).SetDelay(0.2f);
	}
	
	public void ShakeEyes()
	{
		transform.DOKill();
		transform.transform.DOShakePosition(0.5f,0.1f);
	}

	public void LookLeft()
	{
		eyeball.DOKill();
		eyeball.transform.DOLocalMove(new Vector3(-0.05f,0,0), 0.2f);
	}
	
	public void LookRight()
	{
		eyeball.DOKill();
		eyeball.transform.DOLocalMove(new Vector3(0.05f,0,0), 0.2f);
	}
	
	public void LookUp()
	{
		eyeball.DOKill();
		eyeball.transform.DOLocalMove(new Vector3(0, 0.05f,0 ), 0.2f);
	}
	
	public void LookDown()
	{
		eyeball.DOKill();
		eyeball.transform.DOLocalMove(new Vector3(0, -0.05f,0 ), 0.2f);
	}
	
	public void LookCenter()
	{
		eyeball.DOKill();
		eyeball.transform.DOLocalMove(new Vector3(0,0,0), 0.2f);
	}

	public void LookAtTarget(Vector3 targetPos)
	{
		eyeball.DOKill();
		Vector3 dirV3 = Vector3.Normalize(targetPos - eyeball.position);
		dirV3 = new Vector3(dirV3.x,dirV3.y,0);
		eyeball.transform.DOLocalMove(dirV3 * 0.05f, 0.2f);
	}
	
	

	public void MoveLeft()
	{
		//transform.DOKill();
		transform.DOLocalMoveX(-0.06f,0.2f).SetEase(Ease.Linear);
		transform.DOLocalMoveX(0, 1f).SetEase(Ease.OutElastic).SetDelay(0.2f);
	}
	
	public void MoveRight()
	{
		//transform.DOKill();
		transform.DOLocalMoveX(0.06f,0.2f).SetEase(Ease.Linear);
		transform.DOLocalMoveX(0, 1f).SetEase(Ease.OutElastic).SetDelay(0.2f);
	}
	
	public void MoveUp()
	{
		//transform.DOKill();
		transform.DOLocalMoveY(0.06f + oriY,0.2f).SetEase(Ease.Linear);
		transform.DOLocalMoveY(oriY, 1f).SetEase(Ease.OutElastic).SetDelay(0.2f);
	}
	
	public void MoveDown()
	{
		//transform.DOKill();
		transform.DOLocalMoveY(-0.04f + oriY,0.2f).SetEase(Ease.Linear);
		transform.DOLocalMoveY(oriY, 1f).SetEase(Ease.OutElastic).SetDelay(0.2f);
	}
	
	public void MoveBackToCenter()
	{
		transform.DOKill();
		transform.DOLocalMoveX(0, 1f).SetEase(Ease.OutElastic);
		transform.DOLocalMoveY(oriY, 1f).SetEase(Ease.OutElastic);
	}


	public void DeadEyes()
	{
		transform.DOKill();
		transform.DOScale(0f,0f).SetEase(Ease.Linear);
	}
	
	public void ReviveEyes(float delay = 0f)
	{
		transform.DOKill();
		transform.DOLocalMoveX(0, 0f).SetEase(Ease.OutElastic);
		transform.DOLocalMoveY(oriY, 0f).SetEase(Ease.OutElastic);
		transform.DOScale(1f,0f).SetEase(Ease.Linear).SetDelay(delay);
		
		eyeball.DOKill();
		eyeball.transform.DOLocalMove(new Vector3(0,0,0), 0.0f);
	}
}
