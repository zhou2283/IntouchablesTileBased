using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SliderBase : MonoBehaviour
{
	public float gridSize = 0.4f;
	public GameObject connectedLight;
	private bool isXDirection;
	private SpriteRenderer spriteRenderer;
	private Vector3 position;
	public int currentLength = 0;
	public int totalLength;
	public Vector3 minPosition;
	public Vector3 maxPosition;
	
	// Use this for initialization
	void Start ()
	{
		spriteRenderer = GetComponent<SpriteRenderer>();
	}
	
	// Update is called once per frame
	void Update () {
		connectedLight.transform.position = GetConnectedItemPosition();
	}

	public void UpdateInEditor()
	{
		spriteRenderer = GetComponent<SpriteRenderer>();
		//connectedLight.transform.parent = transform;
		if (spriteRenderer.size.x > spriteRenderer.size.y)
		{
			totalLength = Mathf.RoundToInt(spriteRenderer.size.x / gridSize) - 1;
			minPosition = transform.position - new Vector3(totalLength / 2f * gridSize, 0, 0);
			maxPosition = transform.position + new Vector3(totalLength / 2f * gridSize, 0, 0);
		}
		else if (spriteRenderer.size.x < spriteRenderer.size.y)
		{
			totalLength = Mathf.RoundToInt(spriteRenderer.size.y / gridSize) - 1;
			minPosition = transform.position - new Vector3(0, totalLength / 2f * gridSize, 0);
			maxPosition = transform.position + new Vector3(0, totalLength / 2f * gridSize, 0);
		}
		else
		{
			Debug.Log("Slider size cannot be 1:1");
		}

		connectedLight.transform.position = GetConnectedItemPosition();
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
}

