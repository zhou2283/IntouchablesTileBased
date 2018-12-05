using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using FMODUnity;

public class PlayerBase : MonoBehaviour {

    //test var
    public bool invincible = false;
    
    //find part
    protected PlayerControl playerControlScript;
    protected RewindControl rewindControlScript;
    protected GoalGroup goalGroupScript;
    public GameObject deadEffect;
    public GameObject reviveEffect;
    public GameObject juiceEffect;

    public bool activeSelf = true;


    bool canMoveUp = false;
    bool canMoveDown = false;

    float speed = 2f;
    float unitMoveTime = 0.15f;
    bool leftIsDown = false;
    bool rightIsDown = false;
    bool upIsDown = false;
    bool downIsDown = false;
    private bool _leftIsDownLastFrame = false;
    private bool _rightIsDownLastFrame = false;
    private bool _upIsDownLastFrame = false;
    private bool _downIsDownLastFrame = false;

    public bool isTweening = false;
    private bool _isTweeningLastFrame = false;
    public bool isFalling = false;// it is a special case of isTweening
    private bool _isFallingLastFrame = false;
    public bool isTeleporting = false;// to record if player is teleporting
    public bool isInair = false;
    private bool _isInairLastFrame = false;
    public bool isDead = false;

    public bool isMovingLeft = false;
    public bool isMovingRight = false;

    LayerMask solidBlockLayer = 1 << 9;
    LayerMask glassBlockLayer = 1 << 10;
    LayerMask solidBoxLayer = 1 << 11;
    LayerMask glassBoxLayer = 1 << 12;
    LayerMask ladderLayer = 1 << 13;
    LayerMask outlineLayer = 1 << 21;

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
    float prewarmTime = 1.5f;
    float prewarmTimeCount = 0f;

    //rewind part
    public bool isRewinding = false;
    
    //animation part
    private MeshTwister meshTwisterScript;
    private PlayerEyeControl playerEyeControlScript;
    private PlayerIndicator playerIndicatorScript;
    private RenderGroupControl renderGroupControlScript;
    
    //FMOD part
    private StudioEventEmitter eventEmitter;
    public string moveSound = "event:/PlayerMove/MoveSound";
    public string splashSound = "event:/PlayerMove/SplashSound";
    public string moveOnNetSound = "event:/PlayerMove/MoveOnNetSound";

    // Use this for initialization
    void Start () {
        //find part
        playerControlScript = GameObject.Find("PlayerControl").GetComponent<PlayerControl>();
        rewindControlScript = GameObject.Find("RewindControl").GetComponent<RewindControl>();
        goalGroupScript = GameObject.Find("GoalGroup").GetComponent<GoalGroup>();
        meshTwisterScript = transform.Find("BodyPivotVertical").GetComponent<MeshTwister>();
        playerEyeControlScript = transform.Find("EyeGroup").GetComponent<PlayerEyeControl>();
        playerIndicatorScript = GameObject.Find("PlayerIndicator").GetComponent<PlayerIndicator>();
        renderGroupControlScript = GameObject.Find("RenderGroup").GetComponent<RenderGroupControl>();
        //layer mask part
        downDetectableLayer = solidBlockLayer | glassBlockLayer | solidBoxLayer | glassBoxLayer | ladderLayer | outlineLayer;
        sideDetectableLayer = solidBlockLayer | glassBlockLayer | solidBoxLayer | glassBoxLayer | outlineLayer;
        sideDetectablePushableLayer = solidBoxLayer | glassBoxLayer;
        //precheck function
        CheckLadder();
        
        //FMOD part
        eventEmitter = GetComponent<StudioEventEmitter>();
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
	    
	    if (isDead)//if it is in rewinding, ignore all
	    {
	        isTweening = false;
	        return;
	    }
	    
	    //update interact part
	    UpdateInteractPart();
	    
	    //check if the player is in air
	    CheckInairUpdate();
	    if (isInair && !_isInairLastFrame)
	    {
	        
	        meshTwisterScript.FromGroundToAir();
	        /*
	        if (isMovingLeft)
	        {
	            Instantiate(juiceEffect, transform.position + new Vector3(0,-0.2f,0), Quaternion.Euler(0,0,0));
	        }
	        else if (isMovingRight)
	        {
	            Instantiate(juiceEffect, transform.position + new Vector3(0,-0.2f,0), Quaternion.Euler(0,0,135));
	        }
	        else
	        {
	            Instantiate(juiceEffect, transform.position + new Vector3(0,-0.2f,0), Quaternion.Euler(0,0,0));
	            Instantiate(juiceEffect, transform.position + new Vector3(0,-0.2f,0), Quaternion.Euler(0,0,135));
	        }
	        */
	    }
	    else if (!isInair && _isInairLastFrame)
	    {
	        meshTwisterScript.FromAirToGround();
	        if (isMovingLeft)
	        {
	            Instantiate(juiceEffect, transform.position + new Vector3(0,-0.2f,0), Quaternion.Euler(0,0,0));
	            GameControlSingleton.Instance.PlayOneShotSound(splashSound);
	        }
	        else if (isMovingRight)
	        {
	            Instantiate(juiceEffect, transform.position + new Vector3(0,-0.2f,0), Quaternion.Euler(0,0,135));
	            GameControlSingleton.Instance.PlayOneShotSound(splashSound);
	        }
	        else
	        {
	            Instantiate(juiceEffect, transform.position + new Vector3(0,-0.2f,0), Quaternion.Euler(0,0,0));
	            Instantiate(juiceEffect, transform.position + new Vector3(0,-0.2f,0), Quaternion.Euler(0,0,135));
	            GameControlSingleton.Instance.PlayOneShotSound(splashSound);
	        }
	    }
        _isInairLastFrame = isInair;
	    //detect update
        if (prewarmTimeCount < prewarmTime)
        {
            prewarmTimeCount += Time.deltaTime;
            return;
        }
        else
        {
            if (!isDead && !rewindControlScript.isRewinding)
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
                //meshTwisterScript.FaceRight();
                playerEyeControlScript.LookRight();
                playerEyeControlScript.MoveRight();
                meshTwisterScript.MoveRightTwist();
                //raycast to right grid
                RaycastHit2D hit = Physics2D.Raycast(transform.position, Vector2.right, GameConst.GRID_SIZE, sideDetectableLayer);
                //if hit something
                if (hit)
                {
                    if (hit.transform.gameObject.layer == 11 || hit.transform.gameObject.layer == 12)//if it is a box
                    {
                        if (hit.transform.GetComponent<BoxBase>().CheckAndMoveBox(true))//if it can be moved to right(true)
                        {
                            isTweening = true;
                            isMovingRight = true;
                            //!!!!!IMPORTANT!!!!!the box base will call the CheckStatus function, to make sure tweening animations are all down.
                            transform.DOMoveX(transform.position.x + GameConst.GRID_SIZE, unitMoveTime).SetEase(Ease.Linear).OnComplete(CheckFalling);
                            
                            //FMOD
                            PlayPlayerMoveSound();

                        }
                        else
                        {
                            //cannot push
                            Debug.Log("Cannot Push");
                            //renderGroupControlScript.shakeDir = RenderGroupControl.SHAKE_DIR.RIGHT;
                        }
                    }
                    else
                    {
                        //cannot move
                        Debug.Log("Cannot Move");
                        //renderGroupControlScript.shakeDir = RenderGroupControl.SHAKE_DIR.RIGHT;
                    }
                }
                else
                {
                    isTweening = true;
                    isMovingRight = true;
                    transform.DOMoveX(transform.position.x + GameConst.GRID_SIZE, unitMoveTime).SetEase(Ease.Linear).OnComplete(CheckFalling);
                    
                    //FMOD
                    PlayPlayerMoveSound();
                }
            }

            else if (leftIsDown && !rightIsDown && !upIsDown && !downIsDown)
            {
                //meshTwisterScript.FaceLeft();
                playerEyeControlScript.LookLeft();
                playerEyeControlScript.MoveLeft();
                meshTwisterScript.MoveLeftTwist();
                //raycast to left grid
                RaycastHit2D hit = Physics2D.Raycast(transform.position, Vector2.left, GameConst.GRID_SIZE, sideDetectableLayer);
                //if hit something
                if (hit)
                {
                    if (hit.transform.gameObject.layer == 11 || hit.transform.gameObject.layer == 12)//if it is a box
                    {
                        if (hit.transform.GetComponent<BoxBase>().CheckAndMoveBox(false))//if it can be moved to left(false)
                        {
                            isTweening = true;
                            isMovingLeft = true;
                            //!!!!!IMPORTANT!!!!!the box base will call the CheckStatus function, to make sure tweening animations are all down.
                            transform.DOMoveX(transform.position.x - GameConst.GRID_SIZE, unitMoveTime).SetEase(Ease.Linear).OnComplete(CheckFalling);
                            
                            //FMOD
                            PlayPlayerMoveSound();
                        }
                        else
                        {
                            //cannot push
                            Debug.Log("Cannot Push");
                            //renderGroupControlScript.shakeDir = RenderGroupControl.SHAKE_DIR.LEFT;
                        }
                    }
                    else
                    {
                        //cannot move
                        Debug.Log("Cannot Move");
                        //renderGroupControlScript.shakeDir = RenderGroupControl.SHAKE_DIR.LEFT;
                    }
                }
                else
                {
                    isTweening = true;
                    isMovingLeft = true;
                    transform.DOMoveX(transform.position.x - GameConst.GRID_SIZE, unitMoveTime).SetEase(Ease.Linear).OnComplete(CheckFalling);
                    
                    //FMOD
                    PlayPlayerMoveSound();
                }
            }

            else if (upIsDown && !downIsDown && !leftIsDown && !rightIsDown)
            {
                playerEyeControlScript.MoveUp();
                playerEyeControlScript.LookUp();
                meshTwisterScript.MoveUpTwist();
                CheckLadder();//check ladder first
                if (canMoveUp)
                {
                    RaycastHit2D hit = Physics2D.Raycast(transform.position, Vector2.up, GameConst.GRID_SIZE, sideDetectableLayer);
                    if (hit)
                    {
                        //collide with wall or box
                        Debug.Log("Cannot Move");
                        //renderGroupControlScript.shakeDir = RenderGroupControl.SHAKE_DIR.UP;
                    }
                    else
                    {
                        isTweening = true;
                        transform.DOMoveY(transform.position.y + GameConst.GRID_SIZE, unitMoveTime).SetEase(Ease.Linear).OnComplete(CheckFalling);
                        
                        //FMOD
                        PlayPlayerMoveSound();
                    }
                }
                else
                {
                    //collide with wall or box
                    Debug.Log("Cannot Move");
                    //renderGroupControlScript.shakeDir = RenderGroupControl.SHAKE_DIR.UP;
                }
            }

            else if (downIsDown && !upIsDown && !leftIsDown && !rightIsDown)
            {
                playerEyeControlScript.MoveDown();
                playerEyeControlScript.LookDown();
                meshTwisterScript.MoveDownTwist();
                CheckLadder();//check ladder first
                if (canMoveDown)
                {
                    RaycastHit2D hit = Physics2D.Raycast(transform.position, Vector2.down, GameConst.GRID_SIZE, sideDetectableLayer);
                    if (hit)
                    {
                        //collide with wall or box
                        Debug.Log("Cannot Move");
                        //renderGroupControlScript.shakeDir = RenderGroupControl.SHAKE_DIR.DOWN;
                    }
                    else
                    {
                        isTweening = true;
                        transform.DOMoveY(transform.position.y - GameConst.GRID_SIZE, unitMoveTime).SetEase(Ease.Linear).OnComplete(CheckFalling);
                        
                        //FMOD
                        PlayPlayerMoveSound();
                    }
                }
                else
                {
                    //collide with wall or box
                    Debug.Log("Cannot Move");
                    //renderGroupControlScript.shakeDir = RenderGroupControl.SHAKE_DIR.DOWN;
                }
            }
            
            else
            {
                //renderGroupControlScript.shakeDir = RenderGroupControl.SHAKE_DIR.CENTER;
            }

            if (!leftIsDown && _leftIsDownLastFrame ||
                !rightIsDown && _rightIsDownLastFrame||
                !downIsDown && _downIsDownLastFrame||
                !upIsDown && _upIsDownLastFrame)
            {
                playerEyeControlScript.MoveBackToCenter();
                playerEyeControlScript.LookCenter();
                meshTwisterScript.MoveHorizontalTwistBack();
                meshTwisterScript.MoveVerticalTwistBack();
            }


            
            _leftIsDownLastFrame = leftIsDown;
            _rightIsDownLastFrame = rightIsDown;
            _upIsDownLastFrame = upIsDown;
            _downIsDownLastFrame = downIsDown;
            
            
            
            #endregion


        }
        else if (!activeSelf && !isTweening && !playerControlScript.isDead)//if it is not active, treat is as a box
        {
            CheckFallingInUpdate();
        }
        
	    if (isFalling && !playerControlScript.isDead)
	    {
	        meshTwisterScript.DropTwist();
	        playerEyeControlScript.MoveUp();
	        playerEyeControlScript.LookDown();
	    }

	    if (_isFallingLastFrame && !isFalling)
	    {
	        meshTwisterScript.MoveVerticalTwistBack();
	    }

	    if (isInSwitch && activeSelf)
	    {
	        playerEyeControlScript.LookAtTarget(currentSwitch.GetComponent<SwitchBase>().connectedLight.transform.position);
	    }
	    else if (isInRocker && activeSelf)
	    {
	        playerEyeControlScript.LookAtTarget(currentRocker.GetComponent<RockerBase>().connectedSlider.GetComponent<SliderBase>().connectedItem.transform.position);
	    }
	    else if (isInPortal && activeSelf)
	    {
	        playerEyeControlScript.LookAtTarget(currentPortal.GetComponent<PortalBase>().connectedPortal.transform.position);
	    }

	    _isTweeningLastFrame = isTweening;
	    _isFallingLastFrame = isFalling;
	}



    float CeilGridSizeValue(float value)
    {
        return Mathf.Ceil((value - GameConst.GRID_SIZE / 2f) * (1f / GameConst.GRID_SIZE)) * GameConst.GRID_SIZE + GameConst.GRID_SIZE/2f;
    }

    float FloorGridSizeValue(float value)
    {
        return Mathf.Floor((value + GameConst.GRID_SIZE / 2f) * (1f / GameConst.GRID_SIZE)) * GameConst.GRID_SIZE - GameConst.GRID_SIZE / 2f;
    }

    void CheckLadder()
    {
        RaycastHit2D hitThisGrid = Physics2D.Raycast((Vector2)transform.position + Vector2.up * GameConst.GRID_SIZE, Vector2.down, GameConst.GRID_SIZE, ladderLayer);
        RaycastHit2D hitDown = Physics2D.Raycast((Vector2)transform.position, Vector2.down, GameConst.GRID_SIZE, ladderLayer);
        RaycastHit2D hitUp = Physics2D.Raycast((Vector2)transform.position, Vector2.up, GameConst.GRID_SIZE, ladderLayer);
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
            if (hitThisGrid.transform.CompareTag("Ladder"))//it is ladder
            {
                canMoveUp = true;
            }
            else//it is netting
            {
                if (!hitUp)
                {
                    canMoveUp = false;
                }
                else
                {
                    canMoveUp = true;
                }
            }
        }
        else
        {
            canMoveUp = false;
        }
    }

    public void CheckFalling()
    {
        
        isTeleporting = false;
        isMovingLeft = false;
        isMovingRight = false;
        RaycastHit2D hit = Physics2D.Raycast((Vector2)transform.position, Vector2.down, GameConst.GRID_SIZE, downDetectableLayer);//check hit
        RaycastHit2D hitLadder = Physics2D.Raycast((Vector2)transform.position + new Vector2(0,-GameConst.GRID_SIZE), Vector2.up, GameConst.GRID_SIZE, ladderLayer);//check hit
  
        if (hit && !hit.transform.CompareTag("Netting") || hitLadder)
        {
            isFalling = false;
            isTweening = false;
        }
        else
        {
            isFalling = true;
            isTweening = true;
            transform.DOMoveY(transform.position.y - GameConst.GRID_SIZE, unitMoveTime).SetEase(Ease.Linear).OnComplete(CheckFalling);
        }
    }
    
    public void CheckInairUpdate()
    {
        //isTeleporting = false;
        RaycastHit2D hit = Physics2D.Raycast((Vector2)transform.position, Vector2.down, GameConst.GRID_SIZE/2.0f + 0.02f, sideDetectableLayer);//check hit
        //RaycastHit2D hitLadder = Physics2D.Raycast((Vector2)transform.position + new Vector2(0,-GameConst.GRID_SIZE), Vector2.up, GameConst.GRID_SIZE, ladderLayer);//check hit
  
        if (hit)
        {
            isInair = false;
        }
        else
        {
            isInair = true;
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
        RaycastHit2D sideHit = Physics2D.Raycast((Vector2)transform.position, direction, GameConst.GRID_SIZE, sideDetectableLayer);
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


    public bool CheckPlayerUp()
    {
        if (visited)
        {
            return needMove;
        }
        

        
        //renew direction
        direction = Vector2.up;
        needMove = true;
        visited = true;
        return needMove;
    }
    
    public bool CheckPlayerDown()//used for movable block moving down
    {
        if (visited && needMove)
        {
            return needMove;
        }

        //renew direction
        direction = Vector2.down;
        
        RaycastHit2D downHit = Physics2D.Raycast((Vector2)transform.position, direction, GameConst.GRID_SIZE, sideDetectableLayer);
        if (downHit)
        {
            if(downHit.transform.gameObject.layer == 11 || downHit.transform.gameObject.layer == 12)//if it is a box
            {
                needMove = downHit.transform.GetComponent<BoxBase>().needMove;
            }
            else if(downHit.transform.gameObject.CompareTag("MovableBlock"))//if it is a movable block
            {
                needMove = downHit.transform.GetComponent<MovableBlockBase>().needMoveDown;
            }
            else//it is block
            {
                needMove = false;
            }
        }
        else
        {
            needMove = false;
        }
        
        visited = true;
        return needMove;
    }
    
    

    public void DisableNeedMoveOnNext()
    {
        needMove = false;
        RaycastHit2D sideHit = Physics2D.Raycast((Vector2)transform.position, direction, GameConst.GRID_SIZE, sideDetectableLayer);
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
        if (direction.x == 1f)
        {
            isMovingRight = true;
        }
        else if (direction.x == -1f)
        {
            isMovingLeft = true;
        }
        transform.DOMoveX(transform.position.x + direction.x * GameConst.GRID_SIZE, unitMoveTime).SetEase(Ease.Linear).OnComplete(CheckFalling);
        transform.DOMoveY(transform.position.y + direction.y * GameConst.GRID_SIZE, unitMoveTime).SetEase(Ease.Linear).OnComplete(CheckFalling);
    }

    void CheckFallingInUpdate()
    {
        /*
        if (playerControlScript.isDead)
        {
            return;
        }
        */
        //use two rays to avoid small gap
        RaycastHit2D currentGridHit = Physics2D.Raycast((Vector2)transform.position + new Vector2(0, GameConst.GRID_SIZE), Vector2.down, GameConst.GRID_SIZE, downDetectableLayer);
        RaycastHit2D downleftDownHit = Physics2D.Raycast((Vector2)transform.position + new Vector2(-GameConst.GRID_SIZE / 2.1f, 0), Vector2.down, GameConst.GRID_SIZE/2f + 0.05f, downDetectableLayer);
        RaycastHit2D hitLadder = Physics2D.Raycast((Vector2)transform.position + new Vector2(0,-GameConst.GRID_SIZE), Vector2.up, GameConst.GRID_SIZE, ladderLayer);//check hit
        
        //RaycastHit2D downcenterDownHit = Physics2D.Raycast((Vector2)transform.position + new Vector2(0, 0), Vector2.down, gridSize / 2f + 0.01f, downDetectableLayer);
        RaycastHit2D downrightDownHit = Physics2D.Raycast((Vector2)transform.position + new Vector2(GameConst.GRID_SIZE / 2.1f, 0), Vector2.down, GameConst.GRID_SIZE/2f + 0.05f, downDetectableLayer);
        Debug.DrawRay((Vector2)transform.position + new Vector2(0, GameConst.GRID_SIZE), Vector2.down, Color.green);
        Debug.DrawRay((Vector2)transform.position + new Vector2(-GameConst.GRID_SIZE / 2.1f, 0), Vector2.down, Color.green);
        Debug.DrawRay((Vector2)transform.position + new Vector2(GameConst.GRID_SIZE / 2.1f, 0), Vector2.down, Color.green);
 
        
        
        
        if (downleftDownHit || downrightDownHit)
        {
            if (downleftDownHit.transform.CompareTag("Netting") && downrightDownHit.transform.CompareTag("Netting"))
            {
                if (hitLadder)
                {
                    //it is on ground
                    isFalling = false;
                    isTweening = false;
                }
                else
                {
                    isFalling = true;
                    isTweening = true;
                    transform.DOMoveY(transform.position.y - GameConst.GRID_SIZE, unitMoveTime).SetEase(Ease.Linear).OnComplete(CheckFallingInUpdate);
                }
            }
            else
            {
                //it is on ground
                isFalling = false;
                isTweening = false;
            }
        }
        else if (currentGridHit && currentGridHit.transform.gameObject.layer == 13)//if current is on the ladder
        {
            //it is on ground
            isFalling = false;
            isTweening = false;
        }
        else
        {
            isFalling = true;
            isTweening = true;
            transform.DOMoveY(transform.position.y - GameConst.GRID_SIZE, unitMoveTime).SetEase(Ease.Linear).OnComplete(CheckFallingInUpdate);
            //print(transform.position.y - gridSize);
        }
        
    }


    //Interact Part
    void UpdateInteractPart()
    {
        //change indicator
        if (activeSelf)
        {
            if (isInSwitch)
            {
                playerIndicatorScript.ChangeToGear();
            }
            else if (isInRocker)
            {
                playerIndicatorScript.ChangeToGear();
            }
            else if (isInPortal)
            {
                playerIndicatorScript.ChangeToPortal();
            }
            else
            {
                playerIndicatorScript.ChangeToBasic();
            }
        }
        
        
        isInteractWithRocker = false;
        if (isInSwitch)
        {
            if (Input.GetKeyDown(KeyCode.J) && activeSelf)
            {
                playerIndicatorScript.GearBlink();
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
                    playerIndicatorScript.ChangeToXSlider();
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
                    playerIndicatorScript.ChangeToYSlider();
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
            else if(activeSelf)
            {
                playerIndicatorScript.ChangeToGear();
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
                        .Append(transform.DOScale(0.9f, 0.2f))
                        .AppendCallback(CheckFalling);
                }
                else
                {
                    playerIndicatorScript.PortalBlink();
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
            //change indicator
/*
            
            if (activeSelf)
            {
                playerIndicatorScript.ChangeToGear();
            }*/
        }
        else if (col.CompareTag("Rocker"))
        {
            isInRocker = true;
            currentRocker = col.transform;
            //change indicator
/*            if (activeSelf)
            {
                playerIndicatorScript.ChangeToGear();
            }*/
        }
        else if (col.CompareTag("Portal"))
        {
            isInPortal = true;
            currentPortal = col.transform;
        }
        else if(col.CompareTag("Box") || col.CompareTag("MovableBlock"))
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
            //change indicator
/*            if (activeSelf)
            {
                playerIndicatorScript.ChangeToBasic();
            }*/
        }
        else if (col.CompareTag("Rocker"))
        {
            isInRocker = false;
            currentRocker = null;
            //change indicator
            if (activeSelf)
            {
                playerIndicatorScript.ChangeToBasic();
            }
        }
        else if (col.CompareTag("Portal"))
        {
            isInPortal = false;
            currentPortal = null;
            //change indicator
/*            if (activeSelf)
            {
                playerIndicatorScript.ChangeToBasic();
            }*/
        }
        else if (col.CompareTag("Box"))
        {
            
        }
    }

    public virtual void PlayerDead()
    {
        KillTweening();
        
        
        isFalling = false;
        isDead = true;
        meshTwisterScript.DeadTwist();
        playerEyeControlScript.DeadEyes();
        Instantiate(deadEffect, transform.position, Quaternion.identity);
    }

    public virtual void PlayerRevive()
    {
        Instantiate(reviveEffect, transform.position, Quaternion.identity);
        meshTwisterScript.ReviveTwist(0.4f);
        playerEyeControlScript.ReviveEyes(0.4f);
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
    
    
    //animation part
    public void DoActiveAnimation()
    {
        meshTwisterScript.ActiveTwist();
        playerEyeControlScript.MoveUp();
        playerEyeControlScript.LookCenter();
        playerEyeControlScript.OpenEyes();
    }
    public void DoDisactiveAnimation()
    {
        meshTwisterScript.DisactiveTwist();
        playerEyeControlScript.MoveBackToCenter();
        playerEyeControlScript.LookCenter();
        playerEyeControlScript.CloseEyes();
    }
    
    
    //FMOD
    void PlayPlayerMoveSound()
    {
        GameControlSingleton.Instance.PlayOneShotSound(moveSound);
        //check terrain
        RaycastHit2D hitLadder = Physics2D.Raycast((Vector2)transform.position + new Vector2(0,-GameConst.GRID_SIZE), Vector2.up, GameConst.GRID_SIZE, ladderLayer);//check hit
        if (hitLadder)
        {
            if (hitLadder.transform.CompareTag("Netting"))
            {
                GameControlSingleton.Instance.PlayOneShotSound(moveOnNetSound);
            }
            else if (hitLadder.transform.CompareTag("Ladder"))
            {
                //GameControlSingleton.Instance.PlayOneShotSound(moveOnNetSound);
            }
        }
        
    }
    
}
