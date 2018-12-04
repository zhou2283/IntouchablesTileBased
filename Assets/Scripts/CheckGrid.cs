using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class CheckGrid : MonoBehaviour
{
	public GameObject checkGridUnitPrefab;
	
	GameObject[,] checkGrid = new GameObject[30,16];
	private float gridSize;
	
	Ray ray;
	RaycastHit rayhit;
	float rayDistance = 10f;
	LayerMask lightLayer = 1 << 20;
	
	// Use this for initialization
	void Start ()
	{
		gridSize = GameConst.GRID_SIZE;
		for (int i = 0; i < 30; i++)
		{
			for (int j = 0; j < 16; j++)
			{
				checkGrid[i, j] = GameObject.Instantiate(checkGridUnitPrefab, transform);
				checkGrid[i, j].transform.position = new Vector3(-5.8f + i*gridSize, -3 + j*gridSize, 0);
				checkGrid[i, j].transform.localScale = new Vector3(1f, 1, 1) / transform.localScale.x * 0.95f;
			}
		}
	}
	
	// Update is called once per frame
	void Update () {
		if (Input.GetKeyDown(KeyCode.G))
		{
			ShowCheckGrid();
		}
		else if (Input.GetKeyUp(KeyCode.G))
		{
			HideCheckGrid();
		}
	}

	public void ShowCheckGrid()
	{
		RenewCheckGrid();
		for (int i = 0; i < 30; i++)
		{
			for (int j = 0; j < 16; j++)
			{
				checkGrid[i, j].GetComponent<SpriteRenderer>().DOFade(0.6f, 0.5f);
			}
		}
	}
	
	public void HideCheckGrid()
	{
		for (int i = 0; i < 30; i++)
		{
			for (int j = 0; j < 16; j++)
			{
				checkGrid[i, j].GetComponent<SpriteRenderer>().DOFade(0.0f, 0.5f);
			}
		}
	}

	public void RenewCheckGrid()
	{
		for (int i = 0; i < 30; i++)
		{
			for (int j = 0; j < 16; j++)
			{
				ray = new Ray(checkGrid[i, j].transform.position + new Vector3(0, 0, -rayDistance / 2f), new Vector3(0, 0, 1f));
				if (Physics.Raycast(ray, out rayhit, rayDistance, lightLayer))
				{
					checkGrid[i, j].GetComponent<SpriteRenderer>().color = new Color(1,1,1,0);
				}
				else
				{
					checkGrid[i, j].GetComponent<SpriteRenderer>().color = new Color(0,0,0,0);
				}
				
			}
		}
	}
}
