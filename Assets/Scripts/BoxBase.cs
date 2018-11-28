using System.Collections;
using System.Collections.Generic;
using System.Text;
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
    BoxGroup boxGroupScript;
    // Use this for initialization
    void Start()
    {
        //layer mask part
        sideDetectableLayer = solidBlockLayer | glassBlockLayer | solidBoxLayer | glassBoxLayer | outlineLayer;
        sideDetectableLayerIncludePlayer = sideDetectableLayer | playerTriggerLayer;

        //find part
        boxGroupScript = transform.parent.GetComponent<BoxGroup>();
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
        boxGroupScript.InitializeBoxGroupAndPlayer();
        bool result = CheckBox(isRight, true);//check box include player
        if (result == false)
        {
            //initialize recursive states
            boxGroupScript.InitializeBoxGroupAndPlayer();
            result = CheckBox(isRight, false);//check box exclude player(smash player)
        }
        if (result)
        {
            boxGroupScript.MoveAllBoxAndPlayer();//move all marked obj
        }
        return result;
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
            DisableNeedMoveOnNext(includePlayer);
            needMove = false;
            visited = true;
        }
        else if (downSideHitIsPushable && !upSideHitIsPushable)
        {
            DisableNeedMoveOnNext(includePlayer);
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
    
    
    public bool CheckAndMoveBoxUp()//this will only be called when it is the first attached box
    {
        //initialize recursive states
        boxGroupScript.InitializeBoxGroupAndPlayer();
        bool result = CheckBoxUp(true);//check box include player
        if (result == false)
        {
            //initialize recursive states
            boxGroupScript.InitializeBoxGroupAndPlayer();
            result = CheckBoxUp(false);//check box exclude player(smash player)
        }
        if (result)
        {
            boxGroupScript.MoveAllBoxAndPlayer();//move all marked obj
        }
        return result;
    }

    public bool CheckBoxUp(bool includePlayer = true)//used for movable block moving up
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
        direction = Vector2.up;
        
        RaycastHit2D topleftUpHit = Physics2D.Raycast((Vector2)transform.position + new Vector2(-gridSize / 2f, gridSize / 2f), Vector2.up, gridSize, _checkLayer);
        RaycastHit2D toprightUpHit = Physics2D.Raycast((Vector2)transform.position + new Vector2(gridSize / 2f, gridSize / 2f), Vector2.up, gridSize, _checkLayer);
        bool topleftUpHitIsPushable = false;
        bool toprightUpHitIsPushable = false;
        if (topleftUpHit)
        {
            if(topleftUpHit.transform.gameObject.layer == 11 || topleftUpHit.transform.gameObject.layer == 12)//if it is a box
            {
                topleftUpHitIsPushable = topleftUpHit.transform.GetComponent<BoxBase>().CheckBoxUp(includePlayer);
            }
            else if(topleftUpHit.transform.gameObject.layer == 14)//if it is a player
            {
                topleftUpHitIsPushable = topleftUpHit.transform.GetComponent<PlayerBase>().CheckPlayerUp();
            }
            else//it is block
            {
                topleftUpHitIsPushable = false;
            }
        }
        else//nothing
        {
            topleftUpHitIsPushable = true;
        }
        
        if (toprightUpHit)
        {
            if(toprightUpHit.transform.gameObject.layer == 11 || toprightUpHit.transform.gameObject.layer == 12)//if it is a box
            {
                toprightUpHitIsPushable = toprightUpHit.transform.GetComponent<BoxBase>().CheckBoxUp(includePlayer);
            }
            else if(toprightUpHit.transform.gameObject.layer == 14)//if it is a player
            {
                toprightUpHitIsPushable = toprightUpHit.transform.GetComponent<PlayerBase>().CheckPlayerUp();
            }
            else//it is block
            {
                toprightUpHitIsPushable = false;
            }
        }
        else//nothing
        {
            toprightUpHitIsPushable = true;
        }
        
        if (topleftUpHitIsPushable && !toprightUpHitIsPushable)
        {
            DisableNeedMoveOnNext(includePlayer);
            needMove = false;
            visited = true;
        }
        else if (toprightUpHitIsPushable && !topleftUpHitIsPushable)
        {
            DisableNeedMoveOnNext(includePlayer);
            needMove = false;
            visited = true;
        }
        else if (topleftUpHitIsPushable && toprightUpHitIsPushable)//there is nothing on the way, need to move
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
    
    
    public bool CheckBoxDown()//used for movable block moving down
    {
        LayerMask _checkLayer;
        _checkLayer = sideDetectableLayerIncludePlayer;
        //if it is visited, return
        if (visited)
        {
            return needMove;
        }
        //renew direction
        direction = Vector2.down;
        
        RaycastHit2D topleftUpHit = Physics2D.Raycast((Vector2)transform.position + new Vector2(-gridSize / 2f, gridSize / 2f), Vector2.up, gridSize, _checkLayer);
        RaycastHit2D toprightUpHit = Physics2D.Raycast((Vector2)transform.position + new Vector2(gridSize / 2f, gridSize / 2f), Vector2.up, gridSize, _checkLayer);
        RaycastHit2D downleftDownHit = Physics2D.Raycast((Vector2)transform.position + new Vector2(-gridSize / 2f, -gridSize / 2f), Vector2.down, gridSize, _checkLayer);
        RaycastHit2D downrightDownHit = Physics2D.Raycast((Vector2)transform.position + new Vector2(gridSize / 2f, -gridSize / 2f), Vector2.down, gridSize, _checkLayer);
        bool topleftUpHitIsPushable = false;
        bool toprightUpHitIsPushable = false;
        bool downleftDownHitIsMoving = false;
        bool downrightDownHitIsMoving = false;

        //check if itself can move first
        if (downleftDownHit)
        {
            if(downleftDownHit.transform.gameObject.layer == 11 || downleftDownHit.transform.gameObject.layer == 12)//if it is a box
            {
                downleftDownHitIsMoving = downleftDownHit.transform.GetComponent<BoxBase>().needMove;
            }
            else if(downleftDownHit.transform.gameObject.CompareTag("MovableBlock"))//if it is a movable block
            {
                downleftDownHitIsMoving = downleftDownHit.transform.GetComponent<MovableBlockBase>().needMoveDown;
            }
            else//it is block
            {
                downleftDownHitIsMoving = false;
            }
        }
        else//nothing
        {
            downleftDownHitIsMoving = true;
        }
        
        if (downrightDownHit)
        {
            if(downrightDownHit.transform.gameObject.layer == 11 || downrightDownHit.transform.gameObject.layer == 12)//if it is a box
            {
                downrightDownHitIsMoving = downrightDownHit.transform.GetComponent<BoxBase>().needMove;
            }
            else if(downrightDownHit.transform.gameObject.CompareTag("MovableBlock"))//if it is a movable block
            {
                downrightDownHitIsMoving = downrightDownHit.transform.GetComponent<MovableBlockBase>().needMoveDown;
            }
            else//it is block
            {
                downrightDownHitIsMoving = false;
            }
        }
        else//nothing
        {
            downrightDownHitIsMoving = true;
        }

        if (downleftDownHitIsMoving && downrightDownHitIsMoving)
        {
            needMove = true;
            visited = true;
        }
        
        //drag all boxes above
        if (topleftUpHit)
        {
            if(topleftUpHit.transform.gameObject.layer == 11 || topleftUpHit.transform.gameObject.layer == 12)//if it is a box
            {
                topleftUpHit.transform.GetComponent<BoxBase>().CheckBoxDown();
            }
            else if(topleftUpHit.transform.gameObject.layer == 14)//if it is a player
            {
                topleftUpHit.transform.GetComponent<PlayerBase>().CheckPlayerDown();
            }
        }
        if (toprightUpHit)
        {
            if(toprightUpHit.transform.gameObject.layer == 11 || toprightUpHit.transform.gameObject.layer == 12)//if it is a box
            {
                toprightUpHit.transform.GetComponent<BoxBase>().CheckBoxDown();
            }
            else if(toprightUpHit.transform.gameObject.layer == 14)//if it is a player
            {
                toprightUpHit.transform.GetComponent<PlayerBase>().CheckPlayerDown();
            }
        }   

        return needMove;
    }
    

    public void DisableNeedMoveOnNext(bool includePlayer = true)
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
        needMove = false;
        RaycastHit2D topleftUpHit = Physics2D.Raycast((Vector2)transform.position + new Vector2(-gridSize / 2f, gridSize / 2f), Vector2.up, gridSize, _checkLayer);
        RaycastHit2D toprightUpHit = Physics2D.Raycast((Vector2)transform.position + new Vector2(gridSize / 2f, gridSize / 2f), Vector2.up, gridSize, _checkLayer);
        RaycastHit2D upSideHit = Physics2D.Raycast((Vector2)transform.position + new Vector2(direction.x * gridSize / 2f, gridSize / 2f), direction, gridSize, _checkLayer);
        RaycastHit2D downSideHit = Physics2D.Raycast((Vector2)transform.position + new Vector2(direction.x * gridSize / 2f, -gridSize / 2f), direction, gridSize, _checkLayer);

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
            if (child)
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
    }

    public void MoveBox()
    {
        isTweening = true;
        transform.DOMoveX(transform.position.x + direction.x * gridSize, unitMoveTime).SetEase(Ease.Linear).OnComplete(CheckStatus);
        transform.DOMoveY(transform.position.y + direction.y * gridSize, unitMoveTime).SetEase(Ease.Linear).OnComplete(CheckStatus);
    }

    void DisactiveTweening()
    {
        isTweening = false;
    }

    void CheckStatus()
    {
        RaycastHit2D downleftDownHit = Physics2D.Raycast((Vector2)transform.position + new Vector2(-gridSize / 2f, -gridSize / 2f), Vector2.down, gridSize/2f+0.05f, sideDetectableLayer);
        RaycastHit2D downrightDownHit = Physics2D.Raycast((Vector2)transform.position + new Vector2(gridSize / 2f, -gridSize / 2f), Vector2.down, gridSize/2f+0.05f, sideDetectableLayer);

        
        if (CanFallCheck(downleftDownHit,downrightDownHit))
        {
            //falling
            isTweening = true;
            isFalling = true;
            transform.DOMoveY(transform.position.y - gridSize, unitMoveTime).SetEase(Ease.Linear).OnComplete(CheckStatusUpdate);
        }
        else
        {
            //it is on ground
            isTweening = false;
            isFalling = false;
        }
        
        /*
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
        */
    }

    void CheckStatusUpdate()
    {
        RaycastHit2D downleftDownHit = Physics2D.Raycast((Vector2)transform.position + new Vector2(-gridSize / 1.1f, -gridSize / 2f), Vector2.down, gridSize / 2f + 0.05f, sideDetectableLayer);
        RaycastHit2D downrightDownHit = Physics2D.Raycast((Vector2)transform.position + new Vector2(gridSize / 1.1f, -gridSize / 2f), Vector2.down, gridSize / 2f + 0.05f, sideDetectableLayer);
        Debug.DrawRay((Vector2)transform.position + new Vector2(-gridSize / 1.1f, -gridSize / 2f), Vector2.down, Color.green);
        Debug.DrawRay((Vector2)transform.position + new Vector2(gridSize / 1.1f, -gridSize / 2f), Vector2.down, Color.green);

        
        if (CanFallCheck(downleftDownHit,downrightDownHit))
        {
            //falling
            isTweening = true;
            isFalling = true;
            transform.DOMoveY(transform.position.y - gridSize, unitMoveTime).SetEase(Ease.Linear).OnComplete(CheckStatusUpdate);
        }
        else
        {
            //it is on ground
            isTweening = false;
            isFalling = false;
        }
        
        /*
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
        */
    }
    
    bool CanFallCheck(RaycastHit2D downleftDownHit, RaycastHit2D downrightDownHit)
    {
        bool downLeftCanFall = false;
        bool downRightCanFall = false;

        if (!downleftDownHit)
        {
            downLeftCanFall = true;
        }
        else
        {
            if (downleftDownHit.transform.CompareTag("Box"))
            {
                if (downleftDownHit.transform.GetComponent<BoxBase>().isFalling)
                {
                    downLeftCanFall = true;
                }
                else
                {
                    downLeftCanFall = false;
                }
            }
            /*
            else if (downleftDownHit.transform.CompareTag("MovableBlock"))
            {
                if (downleftDownHit.transform.GetComponent<MovableBlockBase>().isFalling)
                {
                    downLeftCanFall = true;
                }
                else
                {
                    downLeftCanFall = false;
                }
            }
            */
            else
            {
                downLeftCanFall = false;
            }
        }
        if (!downrightDownHit)
        {
            downRightCanFall = true;
        }
        else
        {
            if (downrightDownHit.transform.CompareTag("Box"))
            {
                if (downrightDownHit.transform.GetComponent<BoxBase>().isFalling)
                {
                    downRightCanFall = true;
                }
                else
                {
                    downRightCanFall = false;
                }
            }
            /*
            else if (downrightDownHit.transform.CompareTag("MovableBlock"))
            {
                if (downrightDownHit.transform.GetComponent<MovableBlockBase>().isFalling)
                {
                    downRightCanFall = true;
                }
                else
                {
                    downRightCanFall = false;
                }
            }
            */
            else
            {
                downRightCanFall = false;
            }
        }

        return (downLeftCanFall && downRightCanFall);
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
