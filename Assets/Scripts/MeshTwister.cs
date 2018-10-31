using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class MeshTwister : MonoBehaviour {

	Mesh mesh;
	Vector3[] vertices;
	Vector3[] verticesOri;

	private Transform bodyPivotHorizontal;
	private Transform bodyPivotVertical;
	private Transform bodyMesh;
	
	private int sideCount = 11;
	public float twisterPower = 0;
	
	// Use this for initialization
	void Start ()
	{
		bodyPivotVertical = transform;
		bodyPivotHorizontal = transform.Find("BodyPivotHorizontal");
		bodyMesh = bodyPivotHorizontal.Find("BodyMesh");
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
		bodyPivotVertical.transform.DOKill();
		DOTween.To(() => twisterPower, x => twisterPower = x, 3f, 0.2f).SetEase(Ease.Linear);
		bodyPivotVertical.transform.DOScaleY(0.9f,0.2f).SetEase(Ease.Linear);
		DOTween.To(() => twisterPower, x => twisterPower = x, 0, 1f).SetEase(Ease.OutElastic).SetDelay(0.2f);
		bodyPivotVertical.transform.DOScaleY(1f, 1f).SetEase(Ease.OutElastic).SetDelay(0.2f);
	}
	
	public void MoveHorizontalTwistBack()
	{
		bodyPivotVertical.transform.DOKill();
		DOTween.To(() => twisterPower, x => twisterPower = x, 0, 1f).SetEase(Ease.OutElastic);
		bodyPivotVertical.transform.DOScaleY(1f, 1f).SetEase(Ease.OutElastic);
	}
	
	public void MoveUpTwist()
	{
		bodyPivotHorizontal.transform.DOKill();
		bodyPivotVertical.transform.DOScaleY(1.2f,0.2f).SetEase(Ease.Linear);
		bodyPivotHorizontal.transform.DOScaleX(0.9f,0.2f).SetEase(Ease.Linear);
		bodyPivotVertical.transform.DOScaleY(1f, 1f).SetEase(Ease.OutElastic).SetDelay(0.2f);
		bodyPivotHorizontal.transform.DOScaleX(1f, 1f).SetEase(Ease.OutElastic).SetDelay(0.2f);
	}
	
	public void ActiveTwist()
	{
		bodyPivotHorizontal.transform.DOKill();
		bodyPivotVertical.transform.DOKill();
		bodyPivotVertical.transform.DOScaleY(1.2f,0.2f).SetEase(Ease.Linear);
		bodyPivotHorizontal.transform.DOScaleX(0.9f,0.2f).SetEase(Ease.Linear);
		bodyPivotVertical.transform.DOScaleY(1f, 1f).SetEase(Ease.OutElastic).SetDelay(0.2f);
		bodyPivotHorizontal.transform.DOScaleX(1f, 1f).SetEase(Ease.OutElastic).SetDelay(0.2f);
	}
	
	public void DisactiveTwist()
	{

	}
	
	public void MoveDownTwist()
	{
		bodyPivotHorizontal.transform.DOKill();
		bodyPivotVertical.transform.DOScaleY(0.8f,0.2f).SetEase(Ease.Linear);
		bodyPivotHorizontal.transform.DOScaleX(1.1f,0.2f).SetEase(Ease.Linear);
		bodyPivotVertical.transform.DOScaleY(1f, 1f).SetEase(Ease.OutElastic).SetDelay(0.2f);
		bodyPivotHorizontal.transform.DOScaleX(1f, 1f).SetEase(Ease.OutElastic).SetDelay(0.2f);
	}
	
	public void DropTwist()
	{
		bodyPivotHorizontal.transform.DOKill();
		bodyPivotVertical.transform.DOScaleY(1.3f,0.2f).SetEase(Ease.Linear);
		bodyPivotHorizontal.transform.DOScaleX(0.8f,0.2f).SetEase(Ease.Linear);
		bodyPivotVertical.transform.DOScaleY(1f, 1f).SetEase(Ease.OutElastic).SetDelay(0.2f);
		bodyPivotHorizontal.transform.DOScaleX(1f, 1f).SetEase(Ease.OutElastic).SetDelay(0.2f);
	}
	
	public void MoveVerticalTwistBack()
	{
		bodyPivotHorizontal.transform.DOKill();
		bodyPivotVertical.transform.DOScaleY(1f, 1f).SetEase(Ease.OutElastic);
		bodyPivotHorizontal.transform.DOScaleX(1f, 1f).SetEase(Ease.OutElastic);
	}
	
	public void DeadTwist()
	{
		bodyPivotVertical.transform.DOKill();
		bodyPivotHorizontal.transform.DOKill();
		bodyPivotVertical.transform.DOScale(0f,0f).SetEase(Ease.Linear);
	}
	
	public void ReviveTwist(float delay = 0f)
	{
		bodyPivotVertical.transform.DOKill();
		bodyPivotHorizontal.transform.DOKill();
		bodyPivotVertical.transform.DOScale(1f, 0f).SetDelay(delay);
		bodyPivotHorizontal.transform.DOScale(1f, 0f).SetDelay(delay);
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
