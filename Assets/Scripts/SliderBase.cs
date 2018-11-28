using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using UnityEngine.Serialization;

public class SliderBase : MonoBehaviour
{
	public bool isTweening = false;
	public bool isTweeningBuffer = false;//a little bit delay
	public bool isRewinding = false;
	public float moveTime = 0.15f;
	
	
	public float gridSize = 0.4f;
	[FormerlySerializedAs("connectedLight")] public GameObject connectedItem;
	private bool isBlock = false;
	public bool isXDirection = true;
	private SpriteRenderer spriteRenderer;
	private Vector3 position;
	public int currentLength = 0;
	public int totalLength;
	public Vector3 minPosition;
	public Vector3 maxPosition;

	private MovableBlockBase movableBlockBaseScript;
	private PlayerControl playerControlScript;
	
	// Use this for initialization
	void Start ()
	{
		playerControlScript = GameObject.Find("PlayerControl").GetComponent<PlayerControl>();
		spriteRenderer = GetComponent<SpriteRenderer>();
		if (connectedItem.CompareTag("MovableBlock"))
		{
			isBlock = true;
			movableBlockBaseScript = connectedItem.GetComponent<MovableBlockBase>();
		}
		else
		{
			isBlock = false;
		}
	}
	
	// Update is called once per frame
	void Update () {
		//connectedItem.transform.position = GetConnectedItemPosition();
		if (Input.GetKey(KeyCode.RightArrow))
		{
			MoveToMax();
		}
		else if (Input.GetKey(KeyCode.LeftArrow))
		{
			MoveToMin();
		}

	}

	public void UpdateInEditor()
	{
		spriteRenderer = GetComponent<SpriteRenderer>();
		//connectedLight.transform.parent = transform;
		if (spriteRenderer.size.x > spriteRenderer.size.y)
		{
			isXDirection = true;
			totalLength = Mathf.RoundToInt(spriteRenderer.size.x / gridSize) - 1;
			minPosition = transform.position - new Vector3(totalLength / 2f * gridSize, 0, 0);
			maxPosition = transform.position + new Vector3(totalLength / 2f * gridSize, 0, 0);
		}
		else if (spriteRenderer.size.x < spriteRenderer.size.y)
		{
			isXDirection = false;
			totalLength = Mathf.RoundToInt(spriteRenderer.size.y / gridSize) - 1;
			minPosition = transform.position - new Vector3(0, totalLength / 2f * gridSize, 0);
			maxPosition = transform.position + new Vector3(0, totalLength / 2f * gridSize, 0);
		}
		else
		{
			Debug.Log("Slider size cannot be 1:1");
		}

		connectedItem.transform.position = GetConnectedItemPosition();
	}

	public Vector3 GetConnectedItemPosition()
	{
		if (currentLength > totalLength)
		{
			currentLength = totalLength;
		}

		if (currentLength < 0)
		{
			currentLength = 0;
		}
		return minPosition + ((float) currentLength / (float) totalLength) * (maxPosition - minPosition);
	}

	public Vector3 GetConnectedItemPosition(float _currentLength)
	{
		if (_currentLength > totalLength)
		{
			_currentLength = totalLength;
		}

		if (_currentLength < 0)
		{
			_currentLength = 0;
		}
		return minPosition + ((float) _currentLength / (float) totalLength) * (maxPosition - minPosition);
	}

	public void MoveToMax()//move up or right
	{
		if (!playerControlScript.isWaiting && !playerControlScript.isDead && currentLength < totalLength && !isTweeningBuffer)
		{
			if (!isBlock)
			{
				isTweening = true;
				isTweeningBuffer = true;
				connectedItem.transform.DOMove(GetConnectedItemPosition(currentLength + 1), moveTime).SetEase(Ease.Linear).OnComplete(DisableIsTweeningWhenMoveToMax);
				return;
			}
			//if it is block
			if (isXDirection)//move right
			{
				if (movableBlockBaseScript.CheckAndMoveSide(true))
				{
					isTweening = true;
					isTweeningBuffer = true;
					connectedItem.transform.DOMove(GetConnectedItemPosition(currentLength + 1), moveTime).SetEase(Ease.Linear).OnComplete(DisableIsTweeningWhenMoveToMax);
					return;
				}
				else
				{
					print("cannot slide");
				}
			}
			else//move up
			{
				if (movableBlockBaseScript.CheckAndMoveUp())
				{
					isTweening = true;
					isTweeningBuffer = true;
					connectedItem.transform.DOMove(GetConnectedItemPosition(currentLength + 1), moveTime).SetEase(Ease.Linear).OnComplete(DisableIsTweeningWhenMoveToMax);
					return;
				}
				else
				{
					print("cannot slide");
				}
			}
			
		}
	}

	public void MoveToMin()//move down or left
	{
		
		if (!playerControlScript.isWaiting && !playerControlScript.isDead && currentLength > 0 && !isTweeningBuffer)
		{
			if (!isBlock)
			{
				isTweening = true;
				isTweeningBuffer = true;
				connectedItem.transform.DOMove(GetConnectedItemPosition(currentLength - 1), moveTime).SetEase(Ease.Linear).OnComplete(DisableIsTweeningWhenMoveToMin);
				return;
			}
			//if it is block
			if (isXDirection)//move left
			{
				if (movableBlockBaseScript.CheckAndMoveSide(false))
				{
					isTweening = true;
					isTweeningBuffer = true;
					connectedItem.transform.DOMove(GetConnectedItemPosition(currentLength - 1), moveTime).SetEase(Ease.Linear).OnComplete(DisableIsTweeningWhenMoveToMin);
					return;
				}
				else
				{
					print("cannot slide");
				}
			}
			else//move down
			{
				if (movableBlockBaseScript.CheckAndMoveDown())
				{
					isTweening = true;
					isTweeningBuffer = true;
					connectedItem.transform.DOMove(GetConnectedItemPosition(currentLength - 1), moveTime).SetEase(Ease.Linear).OnComplete(DisableIsTweeningWhenMoveToMin);
					return;
				}
				else
				{
					print("cannot slide");
				}
			}
			
		}
	}

	void DisableIsTweeningWhenMoveToMax()
	{
		currentLength++;
		isTweening = false;
		isTweeningBuffer = false;
		//StartCoroutine(DelayToDisableIsTweeningBuffer(0.005f));
	}
	
	void DisableIsTweeningWhenMoveToMin()
	{
		currentLength--;
		isTweening = false;
		isTweeningBuffer = false;
		//StartCoroutine(DelayToDisableIsTweeningBuffer(0.005f));
	}

	
	public IEnumerator DelayToDisableIsTweeningBuffer(float delaySeconds)
	{
		yield return new WaitForSeconds(delaySeconds);
		isTweeningBuffer = false;
	}
	
	public void KillTweening()
	{
		isTweening = false;
		connectedItem.transform.DOKill();
	}
}

