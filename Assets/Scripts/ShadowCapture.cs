using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShadowCapture : MonoBehaviour
{
	public RenderTexture shadowRenderTex;

	private Texture2D shadowTex2D;

	private int width;
	private int height;
	private Rect rec;
	
	// Use this for initialization
	void Start () {
		width = shadowRenderTex.width;
		height = shadowRenderTex.height;
		rec = new Rect(0, 0, width, height);
		shadowTex2D = new Texture2D(width, height, TextureFormat.ARGB32, false);
	}
	
	// Update is called once per frame
	void Update ()
	{
		UpdateShadow();
	}

	void UpdateShadow()
	{		
		RenderTexture.active = shadowRenderTex;
		shadowTex2D.ReadPixels(new Rect(0, 0, width, height), 0, 0);
		shadowTex2D.Apply();
		GetComponent<SpriteRenderer>().sprite = Sprite.Create(shadowTex2D,rec,new Vector2(0,0),1);
	}
}
