using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class RewindControl : MonoBehaviour {

    Transform boxGroup;
    Transform lightGroup;
    Transform playerLight;
    Transform playerDark;

    Light2DBaseControl[] light2DBaseControlScriptArray;
    ButtonBase[] buttonBaseScriptArray;
    SwitchBase[] switchBaseScriptArray;

    PlayerControl playerControlScript;

    public bool isRewinding = false;
    float rewindTime = 0.1f;
    float rewindFromDeadTime = 0.1f;

    private Transform mainCamera;

    // Use this for initialization
    void Start () {
        //find part
        boxGroup = GameObject.Find("BoxGroup").transform;
        lightGroup = GameObject.Find("LightGroup").transform;
        playerLight = GameObject.Find("PlayerLight").transform;
        playerDark = GameObject.Find("PlayerDark").transform;
        mainCamera = GameObject.Find("Main Camera").transform;
        light2DBaseControlScriptArray = lightGroup.GetComponentsInChildren<Light2DBaseControl>(true);
        buttonBaseScriptArray = lightGroup.GetComponentsInChildren<ButtonBase>(true);
        switchBaseScriptArray = lightGroup.GetComponentsInChildren<SwitchBase>(true);
        playerControlScript = GameObject.Find("PlayerControl").GetComponent<PlayerControl>();
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
        if (!playerControlScript.isDead)
        {
            print("record");
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
    }

    public void Rewind()
    {
        print("rewind");
        if (!isRewinding)
        {
            isRewinding = true;
            RewindPublicPart();
            StartCoroutine(DelayToDisactiveIsRewinding(rewindTime + 0.02f));
        }
    }

    public void RewindFromDead()
    {
        print("rewind from dead");
        if (!isRewinding)
        {
            isRewinding = true;
            //revive effect, takes 0.5f
            if (playerLight.GetComponent<SpriteRenderer>().enabled == false)
            {
                playerLight.GetComponent<PlayerBase>().PlayerRevive();//0.5s
            }
            if (playerDark.GetComponent<SpriteRenderer>().enabled == false)
            {
                playerDark.GetComponent<PlayerBase>().PlayerRevive();//0.5s
            }
            StartCoroutine(DelayToDoRewindPublicPart(0.5f));
            StartCoroutine(DelayToRevive(rewindTime +0.5f));
        }

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
        //disable button
        foreach(ButtonBase child in buttonBaseScriptArray)
        {
            child.RewindingDisable(rewindTime + 0.03f);
        }


    }

    public IEnumerator DelayToDisactiveIsRewinding(float delaySeconds)
    {
        yield return new WaitForSeconds(delaySeconds);
        isRewinding = false;
    }

    public IEnumerator DelayToRevive(float delaySeconds)
    {
        yield return new WaitForSeconds(delaySeconds);
        GameObject.Find("PlayerControl").GetComponent<PlayerControl>().isDead = false;
        isRewinding = false;
    }

    public IEnumerator DelayToDoRewindPublicPart(float delaySeconds)
    {
        yield return new WaitForSeconds(delaySeconds);
        RewindPublicPart();
    }


}

