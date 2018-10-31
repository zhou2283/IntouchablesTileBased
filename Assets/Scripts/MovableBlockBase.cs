using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovableBlockBase : MonoBehaviour
{
	//ray origin need to add transform.position
	private Ray2D[] leftRayArray;
	private RaycastHit2D[] leftRayHitArray;
	private Ray2D[] rightRayArray;
	private RaycastHit2D[] rightRayHitArray;
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
	
	// Use this for initialization
	void Start ()
	{
		gridSize = GameConst.GRID_SIZE;
		
		sideDetectableLayer = solidBlockLayer | glassBlockLayer | solidBoxLayer | glassBoxLayer | outlineLayer;
		sideDetectableLayerIncludePlayer = sideDetectableLayer | playerTriggerLayer;
		
		width = Mathf.RoundToInt(transform.localScale.x);
		height = Mathf.RoundToInt(transform.localScale.y);
		upRayArray = new Ray2D[width];
		upRayHitArray = new RaycastHit2D[width];
		downRayArray = new Ray2D[width];
		downRayHitArray = new RaycastHit2D[width];
		leftRayArray = new Ray2D[height];
		leftRayHitArray = new RaycastHit2D[width];
		rightRayArray = new Ray2D[height];
		rightRayHitArray = new RaycastHit2D[width];

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
		
		
		
	}

	public bool CheckDown()
	{
		for (int i = 0; i < width; i++)
		{
			downRayHitArray[i] = Physics2D.Raycast(downRayArray[i].origin + (Vector2)(transform.position), downRayArray[i].direction, gridSize, sideDetectableLayer);
		}

		foreach (RaycastHit2D child in downRayHitArray)
		{
			//print(child.transform.gameObject.name);
			if (child)
			{
				return false;
			}
		}
		return true;
	}

	public bool CheckAndMoveLeft()
	{
		//initialize recursive states
		boxGroupScript.InitializeBoxGroupAndPlayer();
		bool result = CheckLeft(true);//check box include player
		if (result == false)
		{
			//initialize recursive states
			boxGroupScript.InitializeBoxGroupAndPlayer();
			result = CheckLeft(false);//check box exclude player(smash player)
		}
		if (result)
		{
			boxGroupScript.MoveAllBoxAndPlayer();//move all marked obj
		}
		return result;
	}
	
	public bool CheckAndMoveRight(bool includePlayer = true)
	{
		return true;
	}
	
	public bool CheckAndMoveUp(bool includePlayer = true)
	{
		return true;
	}

	public bool CheckLeft(bool includePlayer = true)
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
			leftRayHitArray[i] = Physics2D.Raycast(leftRayArray[i].origin + (Vector2)(transform.position), leftRayArray[i].direction, gridSize, _checkLayer);
		}
		bool[] isPushableArray = new bool[height];
		for(int i = 0; i < height; i++)
		{
			if (leftRayHitArray[i])
			{
				if(leftRayHitArray[i].transform.gameObject.layer == 11 || leftRayHitArray[i].transform.gameObject.layer == 12)//if it is a box
				{
					isPushableArray[i] = leftRayHitArray[i].transform.GetComponent<BoxBase>().CheckBox(false, includePlayer);
				}
				else if(leftRayHitArray[i].transform.gameObject.layer == 14)//if it is a player
				{
					isPushableArray[i] = leftRayHitArray[i].transform.GetComponent<PlayerBase>().CheckPlayer(false);
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
					upRayHitArray[i].transform.GetComponent<BoxBase>().CheckBox(false, includePlayer);
				}
				else if (upRayHitArray[i].transform.gameObject.layer == 14)
				{
					upRayHitArray[i].transform.GetComponent<PlayerBase>().CheckPlayer(false);
				}
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
			DisableNeedMoveOnNextLeft(includePlayer);
			needMove = false;
		}

		return needMove;
	}
	
	public bool CheckRight(bool includePlayer = true)
	{
		return true;
	}
	
	public bool CheckUp(bool includePlayer = true)
	{
		return true;
	}

	void DisableNeedMoveOnNextLeft(bool includePlayer = true)
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
			leftRayHitArray[i] = Physics2D.Raycast(leftRayArray[i].origin + (Vector2)(transform.position), leftRayArray[i].direction, gridSize, _checkLayer);
		}
		for (int i = 0; i < width; i++)
		{
			upRayHitArray[i] = Physics2D.Raycast(upRayArray[i].origin + (Vector2)(transform.position), upRayArray[i].direction, gridSize, _checkLayer);
		}
		foreach(RaycastHit2D child in leftRayHitArray)
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
