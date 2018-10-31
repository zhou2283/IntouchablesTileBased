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
	
	LayerMask downDetectableLayer;
	LayerMask sideDetectableLayer;
	LayerMask sideDetectablePushableLayer;

	private float gridSize;
	
	// Use this for initialization
	void Start ()
	{
		gridSize = GameConst.GRID_SIZE;
		
		downDetectableLayer = solidBlockLayer | glassBlockLayer | solidBoxLayer | glassBoxLayer | outlineLayer;
		
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
			downRayHitArray[i] = Physics2D.Raycast(downRayArray[i].origin + (Vector2)(transform.position), downRayArray[i].direction, gridSize, downDetectableLayer);
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
}
