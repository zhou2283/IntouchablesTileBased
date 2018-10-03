using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;


[CustomEditor(typeof(SliderBase))]
public class SliderBaseEditor : Editor {
	public override void OnInspectorGUI()
	{
		DrawDefaultInspector();
		GUILayout.Label ("This is a Label in a Custom Editor");
		SliderBase sliderBase = (SliderBase)target;
		if(GUILayout.Button("Update Slider"))
		{
			sliderBase.UpdateInEditor();
		}
	}
}
