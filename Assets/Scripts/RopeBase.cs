using UnityEngine;
using Vectrosity;
using System.Collections.Generic;

public class RopeBase : MonoBehaviour {
	
	public int segments = 100;
	public Color lineColor = new Color(0.5f, 0.5f, 0.5f, 1);
	public Material ropeMaterial;
	List<Vector3> linePoints = new List<Vector3>();
	private VectorLine line;

	private float lineWidth = 10f;
	
	void Start () {
		//initialize
		foreach (Transform child in transform)
		{
			linePoints.Add(child.transform.position);
		}
		line = VectorLine.SetLine(lineColor, linePoints.ToArray());
		line.SetWidth(Screen.width/1920f*lineWidth);
		line.material = ropeMaterial;
		line.joins = Joins.Fill;
		line.layer = 18;
		line.name = "SetLine" + transform.name;
		//update line 
		UpdateDrawLine();
		//set render layer and order
		
		var lineObj = GameObject.Find(line.name);
		lineObj.GetComponent<MeshRenderer>().allowOcclusionWhenDynamic = false;
		lineObj.AddComponent<MeshSortingOrder>();
		lineObj.GetComponent<MeshSortingOrder>().layerName = "Wire";
		lineObj.GetComponent<MeshSortingOrder>().order = 10;
		lineObj.GetComponent<MeshSortingOrder>().enabled = true;
		
	}

	private void Update()
	{
		UpdateDrawLine();
	}

	void UpdateDrawLine()
	{
		linePoints.Clear();
		foreach (Transform child in transform)
		{
			
			linePoints.Add(child.transform.position);
		}
		
		//line = VectorLine.SetLine(lineColor, linePoints.ToArray());
		//line.SetWidth(0.5f);
		//line.joins = Joins.Weld;
		line.points3 = linePoints;
		line.SetWidth(Screen.width/1920f*lineWidth);
		line.Draw3D();
	}
}