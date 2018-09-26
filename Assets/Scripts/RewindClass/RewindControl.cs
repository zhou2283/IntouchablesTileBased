using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class RewindControl : MonoBehaviour {

    //find part
    //transform
    private Transform boxGroup;
    private Transform lightGroup;
    private Transform playerLight;
    private Transform playerDark;
    private Transform mainCamera;
    //array
    private Light2DBaseControl[] light2DBaseControlScriptArray;
    private ButtonBase[] buttonBaseScriptArray;
    private SwitchBase[] switchBaseScriptArray;
    //script
    private PlayerControl playerControlScript;
        
    //rewind state indicator
    public bool isRewinding = false;//this is used to check if we can do rewind when key is hold
    //rewind time
    private float rewindTime = 0.1f;
    private float reviveExtraTime = 0.5f;//this is used when it is rewind from dead player. There is an extra animation time
    private float rewindBufferTime = 0.01f;//this is to make sure all RewindObjectBase are done
    private float disableButtonBufferTime = 0.03f;//this is to make sure the button is not trigger when rewinding. Need to be a little bit big since it is base on FixedUpdate interval

    // Use this for initialization
    void Start () {
        //find part
        //transform
        boxGroup = GameObject.Find("BoxGroup").transform;
        lightGroup = GameObject.Find("LightGroup").transform;
        playerLight = GameObject.Find("PlayerLight").transform;
        playerDark = GameObject.Find("PlayerDark").transform;
        mainCamera = GameObject.Find("Main Camera").transform;
        //array
        light2DBaseControlScriptArray = lightGroup.GetComponentsInChildren<Light2DBaseControl>(true);
        buttonBaseScriptArray = lightGroup.GetComponentsInChildren<ButtonBase>(true);
        switchBaseScriptArray = lightGroup.GetComponentsInChildren<SwitchBase>(true);
        //script
        playerControlScript = GameObject.Find("PlayerControl").GetComponent<PlayerControl>();
        //record the start state
        Record();
    }
	
	// Update is called once per frame
	void Update () {
	    if (isRewinding)
	    {
	        mainCamera.transform.DOMoveZ(-12f, 0.5f);
	    }
	    else
	    {
	        mainCamera.transform.DOMoveZ(-10f, 0.5f);
	    }
	}

    public void Record()
    {
        //if player is dead
        if (playerControlScript.isDead)
        {
            //do not record
            return;
        }
        //record part
        //print("All Record);
        //box part
        foreach (Transform child in boxGroup)
        {
            child.GetComponent<RewindObjectBase>().Record();
        }
        //player part
        playerLight.GetComponent<RewindObjectBase>().Record();
        playerDark.GetComponent<RewindObjectBase>().Record();
        //light part
        foreach (Light2DBaseControl child in light2DBaseControlScriptArray)
        {
            child.transform.GetComponent<RewindLight>().Record();
        }
    }

    public void Rewind()
    {
        //if it is rewinding
        if (isRewinding)
        {
            //ignore all
            return;
        }
        //rewind part
        //print("All Rewind");
        //enable isRewinding
        EnableIsRewinding();
        RewindPublicPart();
        StartCoroutine(DelayToDisableIsRewinding(rewindTime + rewindBufferTime));
    }

    public void RewindFromDead()
    {
        //if it is rewinding
        if (isRewinding)
        {
            //ignore all
            return;
        }
        //rewind part
        //print("All Rewind From Dead");
        //enable isRewinding
        EnableIsRewinding();
        //revive effect, takes 0.5f
        if (playerLight.GetComponent<SpriteRenderer>().enabled == false)
        {
            playerLight.GetComponent<PlayerBase>().PlayerRevive();//0.5s
        }
        if (playerDark.GetComponent<SpriteRenderer>().enabled == false)
        {
            playerDark.GetComponent<PlayerBase>().PlayerRevive();//0.5s
        }
        StartCoroutine(DelayToDoRewindPublicPart(reviveExtraTime));
        StartCoroutine(DelayToRevive(reviveExtraTime + rewindTime));
        StartCoroutine(DelayToDisableIsRewinding(reviveExtraTime + rewindTime));
    }

    void RewindPublicPart()
    {
        //for box
        foreach (Transform child in boxGroup)
        {
            child.GetComponent<RewindObjectBase>().Rewind(rewindTime);
        }
        //for player
        playerLight.GetComponent<RewindObjectBase>().Rewind(rewindTime);
        playerDark.GetComponent<RewindObjectBase>().Rewind(rewindTime);
        //for light
        foreach (Light2DBaseControl child in light2DBaseControlScriptArray)
        {
            child.transform.GetComponent<RewindLight>().Rewind(rewindTime);
        }
        //for switch


    }
    
    
    //set isRewinding to true
    protected void EnableIsRewinding()
    {
        isRewinding = true;
    }
    //set isRewinding to false
    protected void DisableIsRewinding()
    {
        isRewinding = false;
    }
    
    
    
    //All Coroutines
    
    private IEnumerator DelayToDisableIsRewinding(float delaySeconds)
    {
        yield return new WaitForSeconds(delaySeconds);
        isRewinding = false;
    }

    private IEnumerator DelayToRevive(float delaySeconds)
    {
        yield return new WaitForSeconds(delaySeconds);
        playerControlScript.isDead = false;
    }

    private IEnumerator DelayToDoRewindPublicPart(float delaySeconds)
    {
        yield return new WaitForSeconds(delaySeconds);
        RewindPublicPart();
    }


}

