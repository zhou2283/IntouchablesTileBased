using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class ButtonBase : MonoBehaviour {

    protected RewindControl rewindControlScript;
    public Transform connectedLight;
    private int count = 0;
    private int _countLastFrame = 0;

    public bool isRewinding = false;


	private Transform buttonCore;
	// Use this for initialization
	void Start () {
	    rewindControlScript = GameObject.Find("RewindControl").GetComponent<RewindControl>();
		buttonCore = transform.Find("ButtonCore");
	}
	
	// Update is called once per frame
	void Update () {
		//print(count);
		//print(rewindControlScript.isRewinding);
		//if is rewinding
	    if (rewindControlScript.isRewinding)
	    {
	        _countLastFrame = count;
	        return;
	    }
		if (_countLastFrame == 0 && count > 0)
		{
			connectedLight.GetComponent<Light2DBaseControl>().LightSwitch();
		}
		else if (_countLastFrame > 0 && count == 0)
		{
			connectedLight.GetComponent<Light2DBaseControl>().LightSwitch();
		}
        _countLastFrame = count;
    }

    private void OnTriggerEnter2D(Collider2D col)
    {
        count++;
	    
    }

	private void OnTriggerStay2D(Collider2D col)
	{
		buttonCore.DOLocalMoveY(-0.1f, 0.15f);
	}

	private void OnTriggerExit2D(Collider2D col)
    {
        count--;
	    buttonCore.DOKill();
	    buttonCore.DOLocalMoveY(0f, 0.15f);
    }
}
