using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class MeshTwister : MonoBehaviour {

	Mesh mesh;
	Vector3[] vertices;
	Vector3[] verticesOri;

	private Transform bodyPivotTop;
	private Transform bodyMesh;
	
	private int sideCount = 11;
	public float twisterPower = 0;
	
	// Use this for initialization
	void Start ()
	{
		bodyPivotTop = transform.Find("BodyPivotTop");
		bodyMesh = bodyPivotTop.Find("BodyMesh");
		mesh = bodyMesh.GetComponent<MeshFilter>().mesh;
		vertices = mesh.vertices;
		verticesOri = new Vector3[vertices.Length];
		for (int i = 0; i < vertices.Length; i++)
		{
			verticesOri[i] = vertices[i];
		}
	}
	
	// Update is called once per frame
	void Update () {

		for (int row = 0; row < sideCount; row++)
		{
			for (int col = 0; col < sideCount; col++)
			{
				vertices[row*sideCount + col] = verticesOri[row*sideCount + col] + new Vector3(twisterPower * ((float)(row*row)/(float)(sideCount*sideCount)),0,0);
			}
		}

		if (Input.GetKeyDown(KeyCode.A))
		{
			
		}
		
		if (Input.GetKeyDown(KeyCode.S)||Input.GetKeyDown(KeyCode.W))
		{

		}
		

		
		mesh.vertices = vertices;
		//mesh.RecalculateBounds();
	}

	public void MoveHorizontalTwist()
	{
		transform.DOKill();
		DOTween.To(() => twisterPower, x => twisterPower = x, 3f, 0.2f).SetEase(Ease.Linear);
		transform.DOScaleY(0.9f,0.2f).SetEase(Ease.Linear);
		DOTween.To(() => twisterPower, x => twisterPower = x, 0, 1f).SetEase(Ease.OutElastic).SetDelay(0.2f);
		transform.DOScaleY(1f, 1f).SetEase(Ease.OutElastic).SetDelay(0.2f);
	}
	
	public void MoveHorizontalTwistBack()
	{
		transform.DOKill();
		DOTween.To(() => twisterPower, x => twisterPower = x, 0, 1f).SetEase(Ease.OutElastic);
		transform.DOScaleY(1f, 1f).SetEase(Ease.OutElastic);
	}
	
	public void MoveUpTwist()
	{
		bodyPivotTop.transform.DOKill();
		transform.DOScaleY(1.2f,0.2f).SetEase(Ease.Linear);
		bodyPivotTop.transform.DOScaleX(0.9f,0.2f).SetEase(Ease.Linear);
		transform.DOScaleY(1f, 1f).SetEase(Ease.OutElastic).SetDelay(0.2f);
		bodyPivotTop.transform.DOScaleX(1f, 1f).SetEase(Ease.OutElastic).SetDelay(0.2f);
	}
	
	public void MoveDownTwist()
	{
		bodyPivotTop.transform.DOKill();
		transform.DOScaleY(0.8f,0.2f).SetEase(Ease.Linear);
		bodyPivotTop.transform.DOScaleX(1.1f,0.2f).SetEase(Ease.Linear);
		transform.DOScaleY(1f, 1f).SetEase(Ease.OutElastic).SetDelay(0.2f);
		bodyPivotTop.transform.DOScaleX(1f, 1f).SetEase(Ease.OutElastic).SetDelay(0.2f);
	}
	
	public void MoveVerticalTwistBack()
	{
		bodyPivotTop.transform.DOKill();
		transform.DOScaleY(1f, 1f).SetEase(Ease.OutElastic);
		bodyPivotTop.transform.DOScaleX(1f, 1f).SetEase(Ease.OutElastic);
	}
	
	public void FaceLeft()
	{
		transform.localScale = new Vector3(1,1,1);
	}
	
	public void FaceRight()
	{
		transform.localScale = new Vector3(-1,1,1);
	}
	
}
