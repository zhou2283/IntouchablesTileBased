using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;

public class MeshTwister : MonoBehaviour {

	Mesh mesh;
	Vector3[] vertices;

	public Mesh baseMesh;
	public Mesh baseMeshInair;
	private float inairPercentage = 0;

	private Transform bodyPivotHorizontal;
	private Transform bodyPivotVertical;
	private Transform bodyMesh;
	
	private int sideCount = 11;
	public float twisterPower = 0;

	private Tweener inairMeshChangeTweener;
	
	// Use this for initialization
	void Start ()
	{
		DOTween.SetTweensCapacity(500,200);
		bodyPivotVertical = transform;
		bodyPivotHorizontal = transform.Find("BodyPivotHorizontal");
		bodyMesh = bodyPivotHorizontal.Find("BodyMesh");
		mesh = bodyMesh.GetComponent<MeshFilter>().mesh;
		vertices = mesh.vertices;
	}
	
	// Update is called once per frame
	void Update () {

		
		for (int row = 0; row < sideCount; row++)
		{
			for (int col = 0; col < sideCount; col++)
			{
				vertices[row*sideCount + col] = inairPercentage * baseMeshInair.vertices[row*sideCount + col] 
				                                + (1 - inairPercentage) * baseMesh.vertices[row*sideCount + col]
				                                + new Vector3(twisterPower * ((float)(row*row)/(float)(sideCount*sideCount)),0,0);
			}
		}
		
		
		

		

		if (Input.GetKeyDown(KeyCode.Y))
		{
			FromGroundToAir();
		}
		
		if (Input.GetKeyDown(KeyCode.U))
		{
			FromAirToGround();
		}
		

		
		mesh.vertices = vertices;
		//mesh.RecalculateBounds();
	}

	public void MoveLeftTwist()
	{
		bodyPivotVertical.transform.DOKill();
		DOTween.To(() => twisterPower, x => twisterPower = x, 1.5f, 0.2f).SetEase(Ease.Linear);
		bodyPivotVertical.transform.DOScaleY(0.9f,0.2f).SetEase(Ease.Linear);
		DOTween.To(() => twisterPower, x => twisterPower = x, 0, 1f).SetEase(Ease.OutElastic).SetDelay(0.2f);
		bodyPivotVertical.transform.DOScaleY(1f, 1f).SetEase(Ease.OutElastic).SetDelay(0.2f);
	}
	
	public void MoveRightTwist()
	{
		bodyPivotVertical.transform.DOKill();
		DOTween.To(() => twisterPower, x => twisterPower = x, -1.5f, 0.2f).SetEase(Ease.Linear);
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
		MoveHorizontalTwistBack();
		MoveVerticalTwistBack();
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
		bodyPivotVertical.transform.DOScaleY(1.2f,0.2f).SetEase(Ease.Linear);
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

	public void FromGroundToAir()
	{
		inairMeshChangeTweener.Kill();
		inairMeshChangeTweener = DOTween.To(() => inairPercentage, x => inairPercentage = x, 1, 1f).SetEase(Ease.OutElastic);
	}

	public void FromAirToGround()
	{
		inairMeshChangeTweener.Kill();
		inairMeshChangeTweener = DOTween.To(() => inairPercentage, x => inairPercentage = x, 0, 0.2f);
	}
	
}
