using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovableBlockBase : MonoBehaviour
{
	public bool isFalling = false;//this will be used for box falling checking

	private float posY;
	private float _posYLastFrame;
	
	//ray origin need to add transform.position
	private Ray2D[] leftRayArray;
	private Ray2D[] rightRayArray;
	private RaycastHit2D[] sideRayHitArray;//used for left and right
	private Ray2D[] upRayArray;
	private RaycastHit2D[] upRayHitArray;
	private Ray2D[] downRayArray;
	private RaycastHit2D[] downRayHitArray;
	private int width;
	private int height;
	
	LayerMask solidBlockLayer = 1 << 9;
	LayerMask glassBlockLayer = 1 << 10;
	LayerMask solidBoxLayer = 1 << 11;
	LayerMask glassBoxLayer = 1 << 12;
	LayerMask playerTriggerLayer = 1 << 14;
	LayerMask outlineLayer = 1 << 21;
	
	LayerMask sideDetectableLayerIncludePlayer;
	LayerMask sideDetectableLayer;

	private float gridSize;
	
	BoxGroup boxGroupScript;

	public bool needMove = false;
	public bool needMoveDown = false;
	
	// Use this for initialization
	void Start ()
	{
		gridSize = GameConst.GRID_SIZE;
		
		sideDetectableLayer = solidBlockLayer | glassBlockLayer | solidBoxLayer | glassBoxLayer | outlineLayer;
		sideDetectableLayerIncludePlayer = sideDetectableLayer | playerTriggerLayer;
		
		width = Mathf.RoundToInt(transform.GetComponent<SpriteRenderer>().size.x/gridSize);
		
		height = Mathf.RoundToInt(transform.GetComponent<SpriteRenderer>().size.y/gridSize);
		
		upRayArray = new Ray2D[width];
		upRayHitArray = new RaycastHit2D[width];
		downRayArray = new Ray2D[width];
		downRayHitArray = new RaycastHit2D[width];
		leftRayArray = new Ray2D[height];
		rightRayArray = new Ray2D[height];
		sideRayHitArray = new RaycastHit2D[width];

		for (int i = 0; i < width; i++)
		{
			float _x = - (float) (width) / 2f * gridSize + gridSize / 2f + gridSize * i;
			float _yUp = + (float) (height) / 2f * gridSize - gridSize / 2f;
			float _yDown = - (float) (height) / 2f * gridSize + gridSize / 2f;
			upRayArray[i] = new Ray2D(new Vector2(_x, _yUp), Vector2.up);
			downRayArray[i] = new Ray2D(new Vector2(_x, _yDown), Vector2.down);
		}
		for (int i = 0; i < height; i++)
		{
			float _xLeft = - (float) (width) / 2f * gridSize + gridSize / 2f;
			float _xRight = + (float) (width) / 2f * gridSize - gridSize / 2f;
			float _y = - (float) (height) / 2f * gridSize + gridSize / 2f + gridSize * i;
			leftRayArray[i] = new Ray2D(new Vector2(_xLeft, _y), Vector2.left);
			rightRayArray[i] = new Ray2D(new Vector2(_xRight, _y), Vector2.right);
		}

		posY = _posYLastFrame = transform.position.y;

		boxGroupScript = GameObject.Find("BoxGroup").GetComponent<BoxGroup>();
	}
	
	// Update is called once per frame
	void Update () {
		//test ray draw
		foreach (Ray2D child in upRayArray)
		{
			Debug.DrawRay(child.origin + (Vector2)(transform.position), child.direction, Color.yellow);
		}
		foreach (Ray2D child in downRayArray)
		{
			Debug.DrawRay(child.origin + (Vector2)(transform.position), child.direction, Color.cyan);
		}
		foreach (Ray2D child in leftRayArray)
		{
			Debug.DrawRay(child.origin + (Vector2)(transform.position), child.direction, Color.magenta);
		}
		foreach (Ray2D child in rightRayArray)
		{
			Debug.DrawRay(child.origin + (Vector2)(transform.position), child.direction, Color.magenta);
		}

		posY = transform.position.y;

		if (_posYLastFrame - posY > 0.01f)
		{
			isFalling = true;
		}
		else
		{
			isFalling = false;
		}
		_posYLastFrame = posY;


	}

	public bool CheckDownMove()
	{
		for (int i = 0; i < width; i++)
		{
			downRayHitArray[i] = Physics2D.Raycast(downRayArray[i].origin + (Vector2)(transform.position), downRayArray[i].direction, gridSize, sideDetectableLayerIncludePlayer);
		}

		foreach (RaycastHit2D child in downRayHitArray)
		{
			//print(child.transform.gameObject.name);
			if (child)
			{
				needMoveDown = false;
				return false;
			}
		}
		
		
		//drag all boxes above down
		needMoveDown = true;
		for (int i = 0; i < width; i++)
		{
			upRayHitArray[i] = Physics2D.Raycast(upRayArray[i].origin + (Vector2)(transform.position), upRayArray[i].direction, gridSize, sideDetectableLayerIncludePlayer);
		}
		
		for(int i = 0; i < width; i++)
		{
			if (upRayHitArray[i])
			{
				if(upRayHitArray[i].transform.gameObject.layer == 11 || upRayHitArray[i].transform.gameObject.layer == 12)//if it is a box
				{
					upRayHitArray[i].transform.GetComponent<BoxBase>().CheckBoxDown();
				}
				else if(upRayHitArray[i].transform.gameObject.layer == 14)//if it is a player
				{
					upRayHitArray[i].transform.GetComponent<PlayerBase>().CheckPlayerDown();
				}
			}
		}
		
		return true;
	}

	public bool CheckAndMoveDown(bool includePlayer = true)
	{
		//initialize recursive states
		boxGroupScript.InitializeBoxGroupAndPlayer();
		bool result = CheckDownMove();
		if (result == false)
		{
			return result;
		}
		else
		{
			boxGroupScript.MoveAllBoxAndPlayer(); //move all marked obj
		}

		return result;
	}

	public bool CheckAndMoveSide(bool isRight)
	{
		//initialize recursive states
		boxGroupScript.InitializeBoxGroupAndPlayer();
		bool result = CheckSide(isRight, true);//check box include player
		if (result == false)
		{
			//initialize recursive states
			boxGroupScript.InitializeBoxGroupAndPlayer();
			result = CheckSide(isRight, false);//check box exclude player(smash player)
		}
		if (result)
		{
			boxGroupScript.MoveAllBoxAndPlayer();//move all marked obj
		}
		return result;
	}
	
	public bool CheckAndMoveUp(bool includePlayer = true)
	{
		//initialize recursive states
		boxGroupScript.InitializeBoxGroupAndPlayer();
		bool result = CheckUpMove(true);//check box include player
		if (result == false)
		{
			//initialize recursive states
			boxGroupScript.InitializeBoxGroupAndPlayer();
			result = CheckUpMove(false);//check box exclude player(smash player)
		}
		if (result)
		{
			boxGroupScript.MoveAllBoxAndPlayer();//move all marked obj
		}
		return result;
	}

	public bool CheckUpMove(bool includePlayer = true)
	{
		needMove = true;
		LayerMask _checkLayer;
		if (includePlayer)
		{
			_checkLayer = sideDetectableLayerIncludePlayer;
		}
		else
		{
			_checkLayer = sideDetectableLayer;
		}
		
		for (int i = 0; i < width; i++)
		{
			upRayHitArray[i] = Physics2D.Raycast(upRayArray[i].origin + (Vector2)(transform.position), upRayArray[i].direction, gridSize, _checkLayer);
		}
		bool[] isPushableArray = new bool[width];
		for(int i = 0; i < width; i++)
		{
			if (upRayHitArray[i])
			{
				if(upRayHitArray[i].transform.gameObject.layer == 11 || upRayHitArray[i].transform.gameObject.layer == 12)//if it is a box
				{
					isPushableArray[i] = upRayHitArray[i].transform.GetComponent<BoxBase>().CheckBoxUp(includePlayer);
				}
				else if(upRayHitArray[i].transform.gameObject.layer == 14)//if it is a player
				{
					isPushableArray[i] = upRayHitArray[i].transform.GetComponent<PlayerBase>().CheckPlayerUp();
				}
				else//it is block
				{
					isPushableArray[i] = false;
				}
			}
			else//nothing
			{
				isPushableArray[i] = true;
			}
		}
		
		bool isAllPushable = true;
		bool isNothingPushable = false;
		foreach (bool child in isPushableArray)
		{
			isAllPushable = (isAllPushable && child);
			isNothingPushable = (isNothingPushable || child);
		}

		if (isAllPushable)//there is nothing on the way, need to move
		{
			needMove = true;
		}
		else if (isNothingPushable == false)//isNothingPushable   //this is trick since it is FALSE
		{
			needMove = false;
		}
		else//some of them is pushable
		{
			DisableNeedMoveOnNext(includePlayer);
			needMove = false;
		}
		return needMove;
		/////////////////////
	}

	public bool CheckSide(bool isRight, bool includePlayer = true)
	{
		bool needMove = true;
		LayerMask _checkLayer;
		if (includePlayer)
		{
			_checkLayer = sideDetectableLayerIncludePlayer;
		}
		else
		{
			_checkLayer = sideDetectableLayer;
		}
		
		//check side first, then check up
		for (int i = 0; i < height; i++)
		{
			if (isRight)
			{
				sideRayHitArray[i] = Physics2D.Raycast(rightRayArray[i].origin + (Vector2)(transform.position), rightRayArray[i].direction, gridSize, _checkLayer);
			}
			else
			{
				sideRayHitArray[i] = Physics2D.Raycast(leftRayArray[i].origin + (Vector2)(transform.position), leftRayArray[i].direction, gridSize, _checkLayer);
			}
		}
		bool[] isPushableArray = new bool[height];
		for(int i = 0; i < height; i++)
		{
			if (sideRayHitArray[i])
			{
				if(sideRayHitArray[i].transform.gameObject.layer == 11 || sideRayHitArray[i].transform.gameObject.layer == 12)//if it is a box
				{
					isPushableArray[i] = sideRayHitArray[i].transform.GetComponent<BoxBase>().CheckBox(isRight, includePlayer);
				}
				else if(sideRayHitArray[i].transform.gameObject.layer == 14)//if it is a player
				{
					isPushableArray[i] = sideRayHitArray[i].transform.GetComponent<PlayerBase>().CheckPlayer(isRight);
				}
				else//it is block
				{
					isPushableArray[i] = false;
				}
			}
			else//nothing
			{
				isPushableArray[i] = true;
			}
		}
		
		bool isAllPushable = true;
		bool isNothingPushable = false;
		foreach (bool child in isPushableArray)
		{
			isAllPushable = (isAllPushable && child);
			isNothingPushable = (isNothingPushable || child);
		}

		if (isAllPushable)//there is nothing on the way, need to move
		{
			needMove = true;
		}
		else if (isNothingPushable == false)//isNothingPushable   //this is trick since it is FALSE
		{
			needMove = false;
		}
		else//some of them is pushable
		{
			DisableNeedMoveOnNext(includePlayer);
			needMove = false;
		}

		if (needMove)
		{
			//check boxes above
			for (int i = 0; i < width; i++)
			{
				upRayHitArray[i] = Physics2D.Raycast(upRayArray[i].origin + (Vector2)(transform.position), upRayArray[i].direction, gridSize, _checkLayer);
			}

			for (int i = 0; i < width; i++)
			{
				if (upRayHitArray[i])
				{
					if (upRayHitArray[i].transform.gameObject.layer == 11 || upRayHitArray[i].transform.gameObject.layer == 12)
					{
						upRayHitArray[i].transform.GetComponent<BoxBase>().CheckBox(isRight, includePlayer);
					}
					else if (upRayHitArray[i].transform.gameObject.layer == 14)
					{
						upRayHitArray[i].transform.GetComponent<PlayerBase>().CheckPlayer(isRight);
					}
				}
			}
		}
		
		

		/*
		bool isAllPushable = true;
		bool isNothingPushable = false;
		foreach (bool child in isPushableArray)
		{
			isAllPushable = (isAllPushable && child);
			isNothingPushable = (isNothingPushable || child);
		}

		if (isAllPushable)//there is nothing on the way, need to move
		{
			needMove = true;
		}
		else if (isNothingPushable == false)//isNothingPushable   //this is trick since it is FALSE
		{
			needMove = false;
		}
		else//some of them is pushable
		{
			DisableNeedMoveOnNext(includePlayer);
			needMove = false;
		}
		*/

		return needMove;
	}
	
	public bool CheckUp(bool includePlayer = true)
	{
		return true;
	}

	void DisableNeedMoveOnNext(bool isRight, bool includePlayer = true)
	{
		LayerMask _checkLayer;
		if (includePlayer)
		{
			_checkLayer = sideDetectableLayerIncludePlayer;
		}
		else
		{
			_checkLayer = sideDetectableLayer;
		}
		for (int i = 0; i < height; i++)
		{
			if (isRight)
			{
				sideRayHitArray[i] = Physics2D.Raycast(rightRayArray[i].origin + (Vector2)(transform.position), rightRayArray[i].direction, gridSize, _checkLayer);
			}
			else
			{
				sideRayHitArray[i] = Physics2D.Raycast(leftRayArray[i].origin + (Vector2)(transform.position), leftRayArray[i].direction, gridSize, _checkLayer);
			}
			
		}
		for (int i = 0; i < width; i++)
		{
			upRayHitArray[i] = Physics2D.Raycast(upRayArray[i].origin + (Vector2)(transform.position), upRayArray[i].direction, gridSize, _checkLayer);
		}
		foreach(RaycastHit2D child in sideRayHitArray)
		{
			if (child)
			{
				if (child.transform.gameObject.layer == 11 || child.transform.gameObject.layer == 12)//if it is a box
				{
					child.transform.GetComponent<BoxBase>().DisableNeedMoveOnNext();
				}
				else if(child.transform.gameObject.layer == 14)
				{
					child.transform.GetComponent<PlayerBase>().DisableNeedMoveOnNext();
				}
				else
				{
					//nothing happend
				}
			}
			
		}
		foreach(RaycastHit2D child in upRayHitArray)
		{
			if (child)
			{
				if (child.transform.gameObject.layer == 11 || child.transform.gameObject.layer == 12)//if it is a box
				{
					child.transform.GetComponent<BoxBase>().DisableNeedMoveOnNext();
				}
				else if(child.transform.gameObject.layer == 14)
				{
					child.transform.GetComponent<PlayerBase>().DisableNeedMoveOnNext();
				}
				else
				{
					//nothing happend
				}
			}
		}
	}
}
