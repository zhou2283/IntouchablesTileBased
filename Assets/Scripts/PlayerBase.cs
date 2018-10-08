﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class PlayerBase : MonoBehaviour {

    //find part
    protected PlayerControl playerControlScript;
    protected RewindControl rewindControlScript;
    protected GoalGroup goalGroupScript;
    public GameObject deadEffect;
    public GameObject reviveEffect;

    public bool activeSelf = true;

    //bool isFalling = false;
    bool canMoveUp = false;
    bool canMoveDown = false;

    float gridSize = 0.4f;
    float speed = 2f;
    float unitMoveTime = 0.15f;
    bool leftIsDown = false;
    bool rightIsDown = false;
    bool upIsDown = false;
    bool downIsDown = false;

    public bool isTweening = false;
    public bool isFalling = false;// it is a special case of isTweening
    public bool isTeleporting = false;// to record if player is teleporting

    LayerMask solidBlockLayer = 1 << 9;
    LayerMask glassBlockLayer = 1 << 10;
    LayerMask solidBoxLayer = 1 << 11;
    LayerMask glassBoxLayer = 1 << 12;
    LayerMask ladderLayer = 1 << 13;

    LayerMask downDetectableLayer;
    LayerMask sideDetectableLayer;
    LayerMask sideDetectablePushableLayer;

    //interact part
    bool isInSwitch = false;
    bool isInRocker = false;
    bool isInPortal = false;
    bool isInteractWithRocker = false;
    bool isInButton = false;
    Transform currentSwitch;
    Transform currentRocker;
    Transform currentButton;
    Transform currentPortal;

    //used for push recursive
    public bool visited = false;
    public bool needMove = false;
    Transform boxGroup;
    Vector2 direction;

    //prewarm time
    float prewarmTime = 0.1f;
    float prewarmTimeCount = 0f;

    //rewind part
    public bool isRewinding = false;

    // Use this for initialization
    void Start () {
        //find part
        playerControlScript = GameObject.Find("PlayerControl").GetComponent<PlayerControl>();
        rewindControlScript = GameObject.Find("RewindControl").GetComponent<RewindControl>();
        goalGroupScript = GameObject.Find("GoalGroup").GetComponent<GoalGroup>();
        //layer mask part
        downDetectableLayer = solidBlockLayer | glassBlockLayer | solidBoxLayer | glassBoxLayer | ladderLayer;
        sideDetectableLayer = solidBlockLayer | glassBlockLayer | solidBoxLayer | glassBoxLayer;
        sideDetectablePushableLayer = solidBoxLayer | glassBoxLayer;

        //precheck function
        CheckLadder();
    }
	
	// Update is called once per frame
	void Update () {

	    if (goalGroupScript.levelIsCleared)
	    {
	        return;
	    }
	    
        if (rewindControlScript.isRewinding)//if it is in rewinding, ignore all
        {
            return;
        }
	    
	    if (playerControlScript.isDead)//if it is in rewinding, ignore all
	    {
	        isTweening = false;
	        return;
	    }
	    
	    //update interact part
	    UpdateInteractPart();
        
	    //detect update
        if (prewarmTimeCount < prewarmTime)
        {
            prewarmTimeCount += Time.deltaTime;
        }
        else
        {
            if (!playerControlScript.isDead && !rewindControlScript.isRewinding)
            {
                UpdateDeathDetect();
            }
        }

        if (activeSelf && !playerControlScript.isWaiting && !playerControlScript.isDead &&!isInteractWithRocker)
        {
            #region =====Update Keydown States=====
            //update keydown states
            if (Input.GetKey(KeyCode.D))
            {
                rightIsDown = true;
            }
            else
            {
                rightIsDown = false;
            }
            if (Input.GetKey(KeyCode.A))
            {
                leftIsDown = true;
            }
            else
            {
                leftIsDown = false;
            }
            if (Input.GetKey(KeyCode.W))
            {
                upIsDown = true;
            }
            else
            {
                upIsDown = false;
            }
            if (Input.GetKey(KeyCode.S))
            {
                downIsDown = true;
            }
            else
            {
                downIsDown = false;
            }
            #endregion

            #region =====Move Character=====
            //move character
            if (rightIsDown && !leftIsDown && !upIsDown && !downIsDown)
            {
                //raycast to right grid
                RaycastHit2D hit = Physics2D.Raycast(transform.position, Vector2.right, gridSize, sideDetectableLayer);
                //if hit something
                if (hit)
                {
                    if (hit.transform.gameObject.layer == 11 || hit.transform.gameObject.layer == 12)//if it is a box
                    {
                        if (hit.transform.GetComponent<BoxBase>().CheckAndMoveBox(true))//if it can be moved to right(true)
                        {
                            isTweening = true;
                            //!!!!!IMPORTANT!!!!!the box base will call the CheckStatus function, to make sure tweening animations are all down.
                            transform.DOMoveX(transform.position.x + gridSize, unitMoveTime).SetEase(Ease.Linear).OnComplete(CheckFalling);
                        }
                        else
                        {
                            //cannot push
                            Debug.Log("Cannot Push");
                        }
                    }
                    else
                    {
                        //cannot move
                        Debug.Log("Cannot Move");
                    }
                }
                else
                {
                    isTweening = true;
                    transform.DOMoveX(transform.position.x + gridSize, unitMoveTime).SetEase(Ease.Linear).OnComplete(CheckFalling);
                }
            }

            if (leftIsDown && !rightIsDown && !upIsDown && !downIsDown)
            {
                //raycast to left grid
                RaycastHit2D hit = Physics2D.Raycast(transform.position, Vector2.left, gridSize, sideDetectableLayer);
                //if hit something
                if (hit)
                {
                    if (hit.transform.gameObject.layer == 11 || hit.transform.gameObject.layer == 12)//if it is a box
                    {
                        if (hit.transform.GetComponent<BoxBase>().CheckAndMoveBox(false))//if it can be moved to left(false)
                        {
                            isTweening = true;
                            //!!!!!IMPORTANT!!!!!the box base will call the CheckStatus function, to make sure tweening animations are all down.
                            transform.DOMoveX(transform.position.x - gridSize, unitMoveTime).SetEase(Ease.Linear).OnComplete(CheckFalling);
                        }
                        else
                        {
                            //cannot push
                            Debug.Log("Cannot Push");
                        }
                    }
                    else
                    {
                        //cannot move
                        Debug.Log("Cannot Move");
                    }
                }
                else
                {
                    isTweening = true;
                    transform.DOMoveX(transform.position.x - gridSize, unitMoveTime).SetEase(Ease.Linear).OnComplete(CheckFalling);
                }
            }

            if (upIsDown && !downIsDown && !leftIsDown && !rightIsDown)
            {
                CheckLadder();//check ladder first
                if (canMoveUp)
                {
                    RaycastHit2D hit = Physics2D.Raycast(transform.position, Vector2.up, gridSize, sideDetectableLayer);
                    if (hit)
                    {
                        //collide with wall or box
                        Debug.Log("Cannot Move");
                    }
                    else
                    {
                        isTweening = true;
                        transform.DOMoveY(transform.position.y + gridSize, unitMoveTime).SetEase(Ease.Linear).OnComplete(CheckFalling);
                    }
                }
            }

            if (downIsDown && !upIsDown && !leftIsDown && !rightIsDown)
            {
                CheckLadder();//check ladder first
                if (canMoveDown)
                {
                    RaycastHit2D hit = Physics2D.Raycast(transform.position, Vector2.down, gridSize, sideDetectableLayer);
                    if (hit)
                    {
                        //collide with wall or box
                        Debug.Log("Cannot Move");
                    }
                    else
                    {
                        isTweening = true;
                        transform.DOMoveY(transform.position.y - gridSize, unitMoveTime).SetEase(Ease.Linear).OnComplete(CheckFalling);
                    }
                }
            }
            #endregion


        }
        else if (!activeSelf && !isTweening)//if it is not active, treat is as a box
        {
            CheckFallingInUpdate();
        }
        
    }



    float CeilGridSizeValue(float value)
    {
        return Mathf.Ceil((value - gridSize / 2f) * (1f / gridSize)) * gridSize + gridSize/2f;
    }

    float FloorGridSizeValue(float value)
    {
        return Mathf.Floor((value + gridSize / 2f) * (1f / gridSize)) * gridSize - gridSize / 2f;
    }

    void CheckLadder()
    {
        RaycastHit2D hitThisGrid = Physics2D.Raycast((Vector2)transform.position + Vector2.up * gridSize, Vector2.down, gridSize, ladderLayer);
        RaycastHit2D hitDown = Physics2D.Raycast((Vector2)transform.position, Vector2.down, gridSize, ladderLayer);
        RaycastHit2D hitUp = Physics2D.Raycast((Vector2)transform.position, Vector2.up, gridSize, ladderLayer);
        if (hitDown || hitThisGrid)
        {
            canMoveDown = true;
        }
        else
        {
            canMoveDown = false;
        }
        if (hitThisGrid)
        {
            canMoveUp = true;
        }
        else
        {
            canMoveUp = false;
        }
    }

    public void CheckFalling()
    {
        isTeleporting = false;
        RaycastHit2D hit = Physics2D.Raycast((Vector2)transform.position, Vector2.down, gridSize, downDetectableLayer);//check hit
        RaycastHit2D hitLadder = Physics2D.Raycast((Vector2)transform.position + new Vector2(0,-gridSize), Vector2.up, gridSize, ladderLayer);//check hit
        if (hit || hitLadder)
        {

            isFalling = false;
            isTweening = false;
        }
        else
        {
            isFalling = true;
            isTweening = true;
            transform.DOMoveY(transform.position.y - gridSize, unitMoveTime).SetEase(Ease.Linear).OnComplete(CheckFalling);
        }
    }

    public bool CheckPlayer(bool isRight)
    {
        if (visited)
        {
            return needMove;
        }
        direction = Vector2.right;
        bool isPushable = false;
        //renew direction
        if (isRight)
        {
            direction = Vector2.right;
        }
        else
        {
            direction = Vector2.left;
        }
        RaycastHit2D sideHit = Physics2D.Raycast((Vector2)transform.position, direction, gridSize, sideDetectableLayer);
        if (sideHit)
        {
            if (sideHit.transform.gameObject.layer == 11 || sideHit.transform.gameObject.layer == 12)//if it is a box
            {
                isPushable = sideHit.transform.GetComponent<BoxBase>().CheckBox(isRight);
            }
            else
            {
                isPushable = false;
            }
        }
        else//nothing
        {
            isPushable = true;
        }

        if (isPushable)
        {
            needMove = true;
            visited = true;
        }
        else
        {
            needMove = false;
            visited = true;
        }
        return needMove;
    }

    public void DisableNeedMoveOnNext()
    {
        needMove = false;
        RaycastHit2D sideHit = Physics2D.Raycast((Vector2)transform.position, direction, gridSize, sideDetectableLayer);
        if (sideHit)
        {
            if (sideHit.transform.gameObject.layer == 11 || sideHit.transform.gameObject.layer == 12)//if it is a box
            {
                sideHit.transform.GetComponent<BoxBase>().DisableNeedMoveOnNext();
            }
            else
            {
                //nothing happened
            }
        }
    }

    public void MovePlayer()
    {
        isTweening = true;
        transform.DOMoveX(transform.position.x + direction.x * gridSize, unitMoveTime).SetEase(Ease.Linear).OnComplete(CheckFalling);
    }

    void CheckFallingInUpdate()
    {
        if (playerControlScript.isDead)
        {
            return;
        }
        //use two rays to avoid small gap
        RaycastHit2D downleftDownHit = Physics2D.Raycast((Vector2)transform.position + new Vector2(-gridSize / 2.1f, 0), Vector2.down, gridSize/2f + 0.01f, downDetectableLayer);
        //RaycastHit2D downcenterDownHit = Physics2D.Raycast((Vector2)transform.position + new Vector2(0, 0), Vector2.down, gridSize / 2f + 0.01f, downDetectableLayer);
        RaycastHit2D downrightDownHit = Physics2D.Raycast((Vector2)transform.position + new Vector2(gridSize / 2.1f, 0), Vector2.down, gridSize/2f + 0.01f, downDetectableLayer);
        Debug.DrawRay((Vector2)transform.position + new Vector2(-gridSize / 2.1f, 0), Vector2.down, Color.green);
        Debug.DrawRay((Vector2)transform.position + new Vector2(gridSize / 2.1f, 0), Vector2.down, Color.green);
        if (downleftDownHit || downrightDownHit)
        {
            //it is on ground
            isFalling = false;
            isTweening = false;

        }
        else
        {
            isFalling = true;
            isTweening = true;
            transform.DOMoveY(transform.position.y - gridSize, unitMoveTime).SetEase(Ease.Linear).OnComplete(CheckFallingInUpdate);
            //print(transform.position.y - gridSize);
        }
    }


    //Interact Part
    void UpdateInteractPart()
    {
        isInteractWithRocker = false;
        if (isInSwitch)
        {
            if (Input.GetKeyDown(KeyCode.J) && activeSelf)
            {
                currentSwitch.GetComponent<SwitchBase>().Interact();
            }
        }
        else if (isInRocker)
        {
            if (Input.GetKey(KeyCode.J) && activeSelf)
            {
                isInteractWithRocker = true;
                if (currentRocker.GetComponent<RockerBase>().connectedSlider.GetComponent<SliderBase>().isXDirection)
                {
                    if (Input.GetKey(KeyCode.A))
                    {
                        currentRocker.GetComponent<RockerBase>().Interact(false);//left(false)
                    }
                    else if (Input.GetKey(KeyCode.D))
                    {
                        currentRocker.GetComponent<RockerBase>().Interact(true);//right(true)
                    }
                }
                else
                {
                    if (Input.GetKey(KeyCode.S))
                    {
                        currentRocker.GetComponent<RockerBase>().Interact(false);//left(false)
                    }
                    else if (Input.GetKey(KeyCode.W))
                    {
                        currentRocker.GetComponent<RockerBase>().Interact(true);//right(true)
                    }
                }
            }
        }
        else if (isInPortal)
        {
            
            if (Input.GetKeyDown(KeyCode.J) && activeSelf && !isTweening)
            {
                var _connectedPortal = currentPortal.GetComponent<PortalBase>().connectedPortal;
                if (_connectedPortal.GetComponent<PortalBase>().isEmpty)
                {
                    isTeleporting = true;
                    isTweening = true;
                    var teleportSequence = DOTween.Sequence();
                    teleportSequence
                        .Append(transform.DOScale(0.1f, 0.2f))//if the player size is 0, OnTriggerExit will not be called
                        .Append(transform.DOMove(_connectedPortal.transform.position,
                            0f))
                        .Append(transform.DOScale(1, 0.2f))
                        .AppendCallback(CheckFalling);
                }
                else
                {
                    print("Portal is not empty");
                }

            }

        }
    }

    private void OnTriggerEnter2D(Collider2D col)
    {
        //print(col.tag);
        if (col.CompareTag("Switch"))
        {
            isInSwitch = true;
            currentSwitch = col.transform;
        }
        else if (col.CompareTag("Rocker"))
        {
            isInRocker = true;
            currentRocker = col.transform;
        }
        else if (col.CompareTag("Portal"))
        {
            isInPortal = true;
            currentPortal = col.transform;
        }
        else if(col.CompareTag("Box"))
        {
            if (!rewindControlScript.isRewinding)
            {
                PlayerDead();
            }            
        }
    }

    private void OnTriggerExit2D(Collider2D col)
    {
        if (col.CompareTag("Switch"))
        {
            isInSwitch = false;
            currentSwitch = null;
        }
        else if (col.CompareTag("Rocker"))
        {
            isInRocker = false;
            currentRocker = null;
        }
        else if (col.CompareTag("Portal"))
        {
            isInPortal = false;
            currentPortal = null;
        }
        else if (col.CompareTag("Box"))
        {
            
        }
    }

    public virtual void PlayerDead()
    {
        KillTweening();
        playerControlScript.isDead = true;
        transform.GetComponent<SpriteRenderer>().enabled = false;
        Instantiate(deadEffect, transform.position, Quaternion.identity);
    }

    public virtual void PlayerRevive()
    {
        Instantiate(reviveEffect, transform.position, Quaternion.identity);
        StartCoroutine(DelayToActiveSprite(0.5f));
    }


    public virtual void UpdateDeathDetect()
    {

    }

    public IEnumerator DelayToActiveSprite(float delaySeconds)
    {
        yield return new WaitForSeconds(delaySeconds);
        transform.GetComponent<SpriteRenderer>().enabled = true;
    }

    //rewind part
    public void KillTweening()
    {
        isTweening = false;
        transform.DOKill();
    }
    /*
    public void RewindingDisable(float delaySeconds)
    {
        isRewinding = true;
        StartCoroutine(DelayToDisactiveIsRewinding(delaySeconds));
    }
    
    public IEnumerator DelayToDisactiveIsRewinding(float delaySeconds)
    {
        yield return new WaitForSeconds(delaySeconds);
        isRewinding = false;
    }
    */
}
