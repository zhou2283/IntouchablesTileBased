using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using DG.Tweening;

public class PlayerControl : MonoBehaviour {

    //find part
    public Transform playerLight;
    public Transform playerDark;
    Transform playerIndicator;
    

    Transform boxGroup;
    Transform lightGroup;
    //rewind part
    private RewindControl rewindControlScript;
    private GoalGroup goalGroupScript;

    public bool isWaiting = false; // this is used for waiting for tweening and animation
    bool _isWaitingLastFrame = false;
    public bool isLightPlayerActive = true;

    public bool isDead = false;

	// Use this for initialization
	void Start () {
        playerLight = transform.Find("PlayerLight");
        playerDark = transform.Find("PlayerDark");
        playerIndicator = transform.Find("PlayerIndicator");
        playerLight.GetComponent<PlayerBase>().activeSelf = isLightPlayerActive;
        playerDark.GetComponent<PlayerBase>().activeSelf = !isLightPlayerActive;
        if (isLightPlayerActive)
        {
            playerIndicator.parent = playerLight;
            playerIndicator.DOLocalMove(new Vector3(0, 0.4f, 0), 0f);
            
        }
        else
        {
            playerIndicator.parent = playerDark;
            playerIndicator.DOLocalMove(new Vector3(0, 0.4f, 0), 0f);
        }

        //find part
        boxGroup = GameObject.Find("BoxGroup").transform;
        lightGroup = GameObject.Find("LightGroup").transform;
        rewindControlScript = GameObject.Find("RewindControl").GetComponent<RewindControl>();
	    goalGroupScript = GameObject.Find("GoalGroup").GetComponent<GoalGroup>();
        //rewind part
        //rewindControlScript.Record();
    }
	
	// Update is called once per frame
	void Update () {

	    if (playerLight.GetComponent<PlayerBase>().isDead || playerDark.GetComponent<PlayerBase>().isDead)
	    {
	        isDead = true;
	    }
	    else
	    {
	        isDead = false;
	    }
	    
	    if (Input.GetKeyDown(KeyCode.R))
	    {
	        RestartLevel();
	    }
	    
	    if (goalGroupScript.levelIsCleared)
	    {
	        return;
	    }
	    
        //update part
        UpdateIsWaiting();

        if (Input.GetKey(KeyCode.Space) && !isWaiting)
        {
            if (!isDead)
            {
                rewindControlScript.Rewind();
            }
            else
            {
                rewindControlScript.RewindFromDead();
            }
            
        }

        if (Input.GetKeyDown(KeyCode.K))
        {
            if (!isDead)
            {
                SwitchPlayers();
            }
            
        }
	}

    void SwitchPlayers()
    {
        if (isLightPlayerActive)
        {
            isLightPlayerActive = false;
            playerDark.GetComponent<PlayerBase>().DoActiveTwistAnimation();
            playerLight.GetComponent<PlayerBase>().DoDisactiveTwistAnimation();
            playerIndicator.parent = playerDark;
            playerIndicator.DOLocalMove(new Vector3(0, 0.4f, 0), 0.1f);
        }
        else
        {
            isLightPlayerActive = true;
            playerLight.GetComponent<PlayerBase>().DoActiveTwistAnimation();
            playerDark.GetComponent<PlayerBase>().DoDisactiveTwistAnimation();
            playerIndicator.parent = playerLight;
            playerIndicator.DOLocalMove(new Vector3(0, 0.4f, 0), 0.1f);
        }

        playerLight.GetComponent<PlayerBase>().activeSelf = isLightPlayerActive;
        playerDark.GetComponent<PlayerBase>().activeSelf = !isLightPlayerActive;
    }

    void UpdateIsWaiting()
    {
        //default is not waiting;
        bool result = false;
        //box part
        BoxBase[] boxBaseArray = boxGroup.GetComponentsInChildren<BoxBase>();
        foreach(BoxBase child in boxBaseArray)
        {
            if (child.isTweening)
            {
                result = true;
            }
        }
        //light part
        Light2DBaseControl[] lightBaseArray = lightGroup.GetComponentsInChildren<Light2DBaseControl>();
        foreach (Light2DBaseControl child in lightBaseArray)
        {
            if (child.isTweening)
            {
                result = true;
            }
        }
        //slider part
        SliderBase[] sliderBaseArray = lightGroup.GetComponentsInChildren<SliderBase>();
        foreach (SliderBase child in sliderBaseArray)
        {
            if (child.isTweening)
            {
                result = true;
            }
        }
        //player part
        if (playerLight.GetComponent<PlayerBase>().isTweening)
        {
            result = true;
        }
        if (playerDark.GetComponent<PlayerBase>().isTweening)
        {
            result = true;
        }
        //renew isWaiting
        isWaiting = result;

        if(isWaiting == true && _isWaitingLastFrame == false)
        {
            rewindControlScript.Record();
        }

        _isWaitingLastFrame = isWaiting;
    }

    void RestartLevel()
    {
        GameObject.Find("RenderGroup").GetComponent<RenderGroupControl>().MoveBack();
        StartCoroutine(DelayToLoadScene(1.5f));
    }
    
    public IEnumerator DelayToLoadScene(float delaySeconds)
    {
        yield return new WaitForSeconds(delaySeconds);
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}
