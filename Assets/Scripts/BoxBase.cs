using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class BoxBase : MonoBehaviour
{
    private bool test;
    float gridSize = 0.4f;

    float unitMoveTime = 0.15f;
    public bool isTweening = false;
    public bool isFalling = false;

    public bool isRewinding = false;

    Vector2 direction = Vector2.right;

    LayerMask solidBlockLayer = 1 << 9;
    LayerMask glassBlockLayer = 1 << 10;
    LayerMask solidBoxLayer = 1 << 11;
    LayerMask glassBoxLayer = 1 << 12;
    LayerMask playerTriggerLayer = 1 << 14;
    LayerMask outlineLayer = 1 << 21;

    LayerMask sideDetectableLayerIncludePlayer;
    LayerMask sideDetectableLayer;

    //used for push recursive
    public bool visited = false;
    public bool needMove = false;
    Transform boxGroup;
    Transform playerLight;
    Transform playerDark;
    // Use this for initialization
    void Start()
    {
        //layer mask part
        sideDetectableLayer = solidBlockLayer | glassBlockLayer | solidBoxLayer | glassBoxLayer | outlineLayer;
        sideDetectableLayerIncludePlayer = sideDetectableLayer | playerTriggerLayer;

        //find part
        boxGroup = transform.parent;
        playerLight = GameObject.Find("PlayerLight").transform;
        playerDark = GameObject.Find("PlayerDark").transform;
    }

    // Update is called once per frame
    void Update () {
        if (isRewinding)//if it is in rewinding, ignore all
        {
            return;
        }
        if (!isTweening)
        {
            CheckStatusUpdate();
        }
    }

    public bool CheckAndMoveBox(bool isRight)//this will only be called when it is the first attached box
    {
        //initialize recursive states
        InitializeBoxGroupAndPlayer();

        //check all boxes first
        bool result = CheckBox(isRight, true);//check include player
        if (result == false)
        {
            //initialize recursive states
            InitializeBoxGroupAndPlayer();
            result = CheckBox(isRight, false);//check exclude player(smash player)
        }
        if (result)
        {
            foreach (Transform child in boxGroup)
            {
                if (child.GetComponent<BoxBase>().needMove)
                {
                    child.GetComponent<BoxBase>().MoveBox();
                }
            }
            if (playerDark.GetComponent<PlayerBase>().needMove)
            {
                playerDark.GetComponent<PlayerBase>().MovePlayer();
            }
            if (playerLight.GetComponent<PlayerBase>().needMove)
            {
                playerLight.GetComponent<PlayerBase>().MovePlayer();
            }
        }
        return result;
    }

    void InitializeBoxGroupAndPlayer()
    {
        foreach (Transform child in boxGroup)
        {
            child.GetComponent<BoxBase>().visited = false;
            child.GetComponent<BoxBase>().needMove = false;
        }
        playerDark.GetComponent<PlayerBase>().visited = false;
        playerDark.GetComponent<PlayerBase>().needMove = false;
        playerLight.GetComponent<PlayerBase>().visited = false;
        playerLight.GetComponent<PlayerBase>().needMove = false;
    }

    public bool CheckBox(bool isRight, bool includePlayer = true)
    {
        LayerMask _checkLayer;
        if (includePlayer)
        {
            _checkLayer = sideDetectableLayerIncludePlayer;
        }
        else
        {
            _checkLayer = sideDetectableLayer;
        }
        //if it is visited, return
        if (visited)
        {
            return needMove;
        }
        //renew direction
        if (isRight)
        {
            direction = Vector2.right;
        }
        else
        {
            direction = Vector2.left;
        } 
        RaycastHit2D topleftUpHit = Physics2D.Raycast((Vector2)transform.position + new Vector2(-gridSize / 2f, gridSize / 2f), Vector2.up, gridSize, _checkLayer);
        RaycastHit2D toprightUpHit = Physics2D.Raycast((Vector2)transform.position + new Vector2(gridSize / 2f, gridSize / 2f), Vector2.up, gridSize, _checkLayer);
        RaycastHit2D upSideHit = Physics2D.Raycast((Vector2)transform.position + new Vector2(direction.x * gridSize / 2f, gridSize / 2f), direction, gridSize, _checkLayer);
        RaycastHit2D downSideHit = Physics2D.Raycast((Vector2)transform.position + new Vector2(direction.x * gridSize / 2f, -gridSize / 2f), direction, gridSize, _checkLayer);

        bool upSideHitIsPushable = false;
        bool downSideHitIsPushable = false;
        
        //check side first, then check up
        if (upSideHit)
        {
            if(upSideHit.transform.gameObject.layer == 11 || upSideHit.transform.gameObject.layer == 12)//if it is a box
            {
                upSideHitIsPushable = upSideHit.transform.GetComponent<BoxBase>().CheckBox(isRight, includePlayer);
            }
            else if(upSideHit.transform.gameObject.layer == 14)//if it is a player
            {
                upSideHitIsPushable = upSideHit.transform.GetComponent<PlayerBase>().CheckPlayer(isRight);
            }
            else//it is block
            {
                upSideHitIsPushable = false;
            }
        }
        else//nothing
        {
            upSideHitIsPushable = true;
        }


        if (downSideHit)
        {
            if (downSideHit.transform.gameObject.layer == 11 || downSideHit.transform.gameObject.layer == 12)//if it is a box
            {
                //print(downSideHit.transform.gameObject.name);
                downSideHitIsPushable = downSideHit.transform.GetComponent<BoxBase>().CheckBox(isRight, includePlayer);
            }
            else if (downSideHit.transform.gameObject.layer == 14)//if it is a player
            {
                downSideHitIsPushable = downSideHit.transform.GetComponent<PlayerBase>().CheckPlayer(isRight);
            }
            else//it is block
            {
                downSideHitIsPushable = false;
            }
        }
        else//nothing
        {
            downSideHitIsPushable = true;
        }


        //check boxes above
        if (topleftUpHit)
        {
            if (topleftUpHit.transform.gameObject.layer == 11 || topleftUpHit.transform.gameObject.layer == 12)
            {
                topleftUpHit.transform.GetComponent<BoxBase>().CheckBox(isRight, includePlayer);
            }
            else if (topleftUpHit.transform.gameObject.layer == 14)
            {
                topleftUpHit.transform.GetComponent<PlayerBase>().CheckPlayer(isRight);
            }
        }
        if (toprightUpHit)
        {
            if (toprightUpHit.transform.gameObject.layer == 11 || toprightUpHit.transform.gameObject.layer == 12)
            {
                toprightUpHit.transform.GetComponent<BoxBase>().CheckBox(isRight, includePlayer);
            }
            else if (toprightUpHit.transform.gameObject.layer == 14)
            {
                toprightUpHit.transform.GetComponent<PlayerBase>().CheckPlayer(isRight);
            }
        }


        if (upSideHitIsPushable && !downSideHitIsPushable)
        {
            DisableNeedMoveOnNext();
            needMove = false;
            visited = true;
        }
        else if (downSideHitIsPushable && !upSideHitIsPushable)
        {
            DisableNeedMoveOnNext();
            needMove = false;
            visited = true;
        }
        else if (upSideHitIsPushable && downSideHitIsPushable)//there is nothing on the way, need to move
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
        RaycastHit2D topleftUpHit = Physics2D.Raycast((Vector2)transform.position + new Vector2(-gridSize / 2f, gridSize / 2f), Vector2.up, gridSize, sideDetectableLayer);
        RaycastHit2D toprightUpHit = Physics2D.Raycast((Vector2)transform.position + new Vector2(gridSize / 2f, gridSize / 2f), Vector2.up, gridSize, sideDetectableLayer);
        RaycastHit2D upSideHit = Physics2D.Raycast((Vector2)transform.position + new Vector2(direction.x * gridSize / 2f, gridSize / 2f), direction, gridSize, sideDetectableLayer);
        RaycastHit2D downSideHit = Physics2D.Raycast((Vector2)transform.position + new Vector2(direction.x * gridSize / 2f, -gridSize / 2f), direction, gridSize, sideDetectableLayer);

        List<RaycastHit2D> hitList = new List<RaycastHit2D>();
        if(upSideHit)
            hitList.Add(upSideHit);
        if(downSideHit)
            hitList.Add(downSideHit);
        if(topleftUpHit)
            hitList.Add(topleftUpHit);
        if(toprightUpHit)
            hitList.Add(toprightUpHit);

        foreach(RaycastHit2D child in hitList)
        {
            if (child.transform.gameObject.layer == 11 || child.transform.gameObject.layer == 12)//if it is a box
            {
                child.transform.GetComponent<BoxBase>().DisableNeedMoveOnNext();
            }
            else if(child.transform.gameObject.layer == 14)
            {
                child.transform.GetComponent<PlayerBase>().DisableNeedMoveOnNext();
            }
            else
            {
                //nothing happend
            }
        }
    }

    public void MoveBox()
    {
        isTweening = true;
        transform.DOMoveX(transform.position.x + direction.x * gridSize, unitMoveTime).SetEase(Ease.Linear).OnComplete(CheckStatus);
    }

    void DisactiveTweening()
    {
        isTweening = false;
    }

    void CheckStatus()
    {
        RaycastHit2D downleftDownHit = Physics2D.Raycast((Vector2)transform.position + new Vector2(-gridSize / 2f, -gridSize / 2f), Vector2.down, gridSize/2f+0.01f, sideDetectableLayer);
        RaycastHit2D downrightDownHit = Physics2D.Raycast((Vector2)transform.position + new Vector2(gridSize / 2f, -gridSize / 2f), Vector2.down, gridSize/2f+0.01f, sideDetectableLayer);
        if (downleftDownHit || downrightDownHit)
        {
            //it is on ground
            isTweening = false;
            isFalling = false;
        }
        else
        {
            //falling
            isTweening = true;
            isFalling = true;
            transform.DOMoveY(transform.position.y - gridSize, unitMoveTime).SetEase(Ease.Linear).OnComplete(CheckStatus);
        }
    }

    void CheckStatusUpdate()
    {
        RaycastHit2D downleftDownHit = Physics2D.Raycast((Vector2)transform.position + new Vector2(-gridSize / 2f, -gridSize / 2f), Vector2.down, gridSize / 2f + 0.01f, sideDetectableLayer);
        RaycastHit2D downrightDownHit = Physics2D.Raycast((Vector2)transform.position + new Vector2(gridSize / 2f, -gridSize / 2f), Vector2.down, gridSize / 2f + 0.01f, sideDetectableLayer);
        Debug.DrawRay((Vector2)transform.position + new Vector2(-gridSize / 2f, -gridSize / 2f), Vector2.down, Color.green);
        Debug.DrawRay((Vector2)transform.position + new Vector2(gridSize / 2f, -gridSize / 2f), Vector2.down, Color.green);
        if (downleftDownHit || downrightDownHit)
        {
            //it is on ground
            isTweening = false;
            isFalling = false;
        }
        else
        {
            //falling
            isTweening = true;
            isFalling = true;
            transform.DOMoveY(transform.position.y - gridSize, unitMoveTime).SetEase(Ease.Linear).OnComplete(CheckStatusUpdate);
        }
    }

    //rewind part
    public void KillTweening()
    {
        isTweening = false;
        transform.DOKill();
    }
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

}
