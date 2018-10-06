using UnityEngine;
using Vectrosity;
using System.Collections.Generic;

public class RopeBase : MonoBehaviour {
	
	public int segments = 100;
	public Color lineColor = new Color(0.5f, 0.5f, 0.5f, 1);
	List<Vector3> linePoints = new List<Vector3>();
	private VectorLine line;
	
	void Start () {
		foreach (Transform child in transform)
		{
			linePoints.Add(child.transform.position);
		}

		line = VectorLine.SetLine(lineColor, linePoints.ToArray());
		line.SetWidth(8f);
		line.joins = Joins.Fill;
		line.layer = 18;
	}

	private void Update()
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
		line.Draw3D();
	}
}