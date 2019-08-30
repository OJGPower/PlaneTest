﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class ControllerPlayer : MonoBehaviour {

    // Debug
    public bool invincible = false;

    // System
    public delegate void PlayerCallback(int name = -1);
    protected PlayerCallback op_CallbackPlayerDie = null;
    protected PlayerCallback op_CallbackGetItem = null;
    protected PlayerCallback op_CallbackGameStart = null;


    // Player values
    public Pilot pilot;
    public SpriteRenderer spriteRenderer;
    public CircleCollider2D op_playerPilot;
    public CircleCollider2D op_playerShield;
    private bool spawnAnimation = false;
    private bool activePlayer = false;
    private Vector2 spawnPostion = Vector2.zero;
    private float spawnAnimSpeed = 1.0f;

    private Vector2 constPosition;


    // Shield values
    //private bool op_playerShieldBool = false;
    private bool op_activeShield = true;
    private bool op_isShieldActive = false;
    public float op_playerShieldDuration = 1f;

    private bool op_playerShieldOverheat = false;
    public float op_playerShieldOverheatStart = 0.0f;
    private float op_playerShieldOverheatTimer = 0;
    public float op_playerShieldOverheatDelay = 3.0f;
    
    public Slider op_playerShieldGauge;
    private ColorBlock op_playerShieldColor = ColorBlock.defaultColorBlock;

    public GameObject op_playerSprite;
    public float op_speed = 5.0f;

    [HideInInspector]
    public Vector3 op_shieldOriginalSize;

    // Debug Options
    public bool op_IsDebug = false;

    // Signal from GameManager Values
    private bool GMshieldFullCharge = false;


    // Move Values
    enum MoveState { Normal = 0, LineFall, RopeLadderIlluminator, StateCount };
    MoveState moveState;
    enum LineFallPlayerLocation { Left = 0, Middle, Right, StateCount };
    LineFallPlayerLocation lineFallPlayerLocation = LineFallPlayerLocation.Middle;

    [Header("- LineFall Values")]
    public float moveSpeedProcess = 0;
    public float moveSpeedInMoveMode = 5.0f;
    public float moveSpeedToNormalPos = 2.0f;
    Vector2 moveModeBasePosition;
    Vector2 nextPosInLineFall;
    Vector2 lastPosInLineFall;

    // Move Switches
    [SerializeField]
    private bool playerMovedInLineFall = false;
    private bool playerAnimationLineFallDeactive = false;
    private bool playerMovedBySlide = false;



    // RLI MoveMode
    private bool playerMovedInRLI = false;

    private List<Vector3> moveModeRLIPosList = new List<Vector3>();
    private int moveModePlayerLocationIndex;

    [SerializeField]
    private bool moveModeNowMoving = false;
    private bool moveModeLeftMoving = false;
    private bool moveModeRLI = false;
    Vector2 moveModeForScreenSize;


    // Use this for initialization
    void Start () {
        if (invincible == true)
            this.transform.GetChild(1).GetComponent<CircleCollider2D>().enabled = false;

        //op_playerPilot = this.transform.GetChild(1).GetComponent<CircleCollider2D>();

        constPosition = PublicValueStorage.Instance.GetPlayerConstPosition();
        //moveState = MoveState.LineFall;


        //List<Vector3> temp = new List<Vector3>();
        //for (int i = 0, j = -1; i < 3; i++, j++)
        //{
        //    Vector3 p = this.transform.position;
        //    p.x = j;
        //    temp.Add(p);
        //}

        //InitRLI(true, temp);
    }

    // Update is called once per frame
    void Update () {

        OP_PlaneMove();
        if (activePlayer == false)
        {
            SpawnAnimation();
        }
        if (activePlayer == true)
        {
            OP_PlayerRunning();
        }
	}


    /////////////////////////////////////////////////////////
    /////////////////////////////////////////////////////////
    ////////////////////// IN SYSTEM ////////////////////////
    /////////////////////////////////////////////////////////
    /////////////////////////////////////////////////////////

    /// <summary>
    /// Set Player's Callback
    /// </summary>
    /// <param name="select">
    /// <para>0. PlayerDie : If player died, destroy all object</para>
    /// <para>1. GetItem : If Player get item, signal to GamaManager.cs</para>
    /// </param>
    /// <param name="callback">
    /// <para>0. PlayerDie</para>
    /// <para>1. GetItem</para>
    /// </param>
    public void SetCallback(int select, PlayerCallback callback)
    {
        switch(select)
        {
            case 0:
                op_CallbackPlayerDie = callback;
                break;
            case 1:
                op_CallbackGetItem = callback;
                break;
        }
    }

    public void SetCallbacks(PlayerCallback playerDie, PlayerCallback getItem, PlayerCallback spawnEnd)
    {
        op_CallbackPlayerDie = playerDie;
        op_CallbackGetItem = getItem;
        op_CallbackGameStart = spawnEnd;
    }


    /////////////////////////////////////////////////////////
    /////////////////////////////////////////////////////////
    ////////////////////// IN GAME AREA /////////////////////
    /////////////////////////////////////////////////////////
    /////////////////////////////////////////////////////////



    // 190502 LifeBalance
    // Method about player control is separated from Update() for legibility
    private void OP_PlayerRunning()
    {
        //if (op_IsDebug == true)
        //{
        //    OP_PlaneMove();
        //}


        // When Player state in Overheat
        if (op_playerShieldOverheat == true)
        {
            // When Player state return normal
            if (op_playerShieldGauge.value >= op_playerShieldOverheatDelay)
            {
                op_playerShieldOverheatTimer = 0;
                op_playerShieldOverheat = false;
                UICanvas.Instance.OP_PrintOverheat(false);
            }
            OP_ShieldUse(false);
            op_playerShieldOverheatTimer += Time.deltaTime;
        }
        else
        {
            if (op_activeShield == true)
            {
                // Switching shield
                if (Input.GetMouseButtonDown(0))
                {
                    
                }
                if (Input.GetMouseButtonUp(0))
                {
                }

                if ((Input.touchCount >= 1 || Input.GetMouseButton(0)) &&
                    (EventSystem.current.IsPointerOverGameObject() == false &&
                    EventSystem.current.IsPointerOverGameObject(0) == false))
                {
                    OP_ShieldActive(true);
                }
                else
                {
                    OP_ShieldActive(false);
                    // Overheat
                }
            }
        }
    }

    /// <summary>
    /// Debug option area
    /// </summary>
    private void OP_PlaneMove()
    {
        //float xMove = 0;
        //float yMove = 0;

        //if (Input.GetKey(KeyCode.RightArrow))
        //    xMove = op_speed * Time.deltaTime;
        //else if (Input.GetKey(KeyCode.LeftArrow))
        //    xMove = -op_speed * Time.deltaTime;
        //if (Input.GetKey(KeyCode.UpArrow))
        //    yMove = op_speed * Time.deltaTime;
        //else if (Input.GetKey(KeyCode.DownArrow))
        //    yMove = -op_speed * Time.deltaTime;
        //this.transform.Translate(new Vector3(xMove, yMove, 0));
        //Debug.Log(moveState);

        switch (moveState)
        {
            case MoveState.Normal:
                //Debug.Log("normal");
                break;

            case MoveState.LineFall:
                MoveModeLineFall();
                break;

            case MoveState.RopeLadderIlluminator:
                MoveModeRLI();
                break;

            default:
                Debug.LogError(this.name + " : OP_PlaneMove has a error.");
                break;
        }


    }

    //
    //
    //
    //
    //          M O V E  M O D E S
    //
    //
    //
    //
    //
    //
    public void InitRLI(bool active, List<Vector3> ladderList = null)
    {
        playerMovedInRLI = active;
        op_isShieldActive = active;
        op_activeShield = !active;

        if (active == true)
        {
            moveModeRLIPosList.Clear();

            if (ladderList == null)
            {
                moveModeRLIPosList = new List<Vector3>();
                moveModeRLIPosList.Add(Vector3.zero);
                Debug.Log("null");
            }
            else
            {
                moveModeRLIPosList = new List<Vector3>(ladderList);
                Debug.Log("not null");
            }

            Vector3 p = this.transform.position;
            for (int i = 0; i < moveModeRLIPosList.Count; i++)
            {
                Vector3 q = moveModeRLIPosList[i];
                moveModeRLIPosList[i] = new Vector3(q.x, p.y, q.z);
            }

            moveModePlayerLocationIndex = (ladderList.Count - 1) / 2;
            moveState = MoveState.RopeLadderIlluminator;
            moveModeRLI = true;
            moveModeRLIFirstInit = true;
            moveModeFirstMyPosition = this.transform.position;

            moveModeForScreenSize = PublicValueStorage.Instance.GetScreenSize();
            Debug.Log("Init MOVEMODE RLI");
            Debug.Log("player idx : " + moveModePlayerLocationIndex);
            Debug.Log("count : " + moveModeRLIPosList.Count);

        }
        else
        {
            moveModeRLI = active;
        }
    }

    private bool moveModeRLIFirstInit = false;
    private Vector3 moveModeFirstMyPosition;
    public void MoveModeRLI()
    {
        // After First Move Animation
        if (moveModeRLIFirstInit == false)
        {
            if (moveModeRLI == true)
            {
                if (moveModeNowMoving == false)
                {
                    if (Input.GetMouseButtonDown(0))
                    {
                        //Debug.Log("Pos : " + currentPosInLineFall);
                        moveModeBasePosition = Input.mousePosition;
                    }
                    if (Input.GetMouseButton(0))
                    {
                        Debug.Log("IDX : " + moveModePlayerLocationIndex);
                        //Debug.Log("mPos : " + moveModeForScreenSize.x);

                        // RIGHT MOVE
                        if (Input.mousePosition.x > moveModeBasePosition.x + (moveModeForScreenSize.x * 10))
                        {
                            // When Player Location index is smaller than Last index
                            if (moveModePlayerLocationIndex < (moveModeRLIPosList.Count - 1))
                            {
                                moveModeNowMoving = true;
                                moveModeLeftMoving = false;
                                Debug.Log("Right!");
                            }
                            //Debug.Log(lineFallPlayerLocation);
                        }

                        // LEFT MOVE
                        if (Input.mousePosition.x < moveModeBasePosition.x - (moveModeForScreenSize.x * 10))
                        {
                            if (moveModePlayerLocationIndex > 0)
                            {
                                moveModeNowMoving = true;
                                moveModeLeftMoving = true;
                                Debug.Log("Left!");
                            }
                            //Debug.Log(lineFallPlayerLocation);
                        }
                    }
                }
                if (moveModeNowMoving == true)
                {
                    moveSpeedProcess += Time.deltaTime * moveSpeedInMoveMode;
                    // Move Right
                    if (moveModeLeftMoving == false)
                    {
                        this.transform.position = Vector3.Lerp
                            (moveModeRLIPosList[moveModePlayerLocationIndex],
                             moveModeRLIPosList[moveModePlayerLocationIndex + 1],
                             moveSpeedProcess);
                    }

                    // Move Left
                    if (moveModeLeftMoving == true)
                    {
                        this.transform.position = Vector3.Lerp
                            (moveModeRLIPosList[moveModePlayerLocationIndex],
                            moveModeRLIPosList[moveModePlayerLocationIndex - 1],
                            moveSpeedProcess);
                    }

                    if (moveSpeedProcess >= 1.0f)
                    {
                        moveModeNowMoving = false;
                        moveSpeedProcess = 0;
                        moveModePlayerLocationIndex =
                            (moveModeLeftMoving == false) ?
                            moveModePlayerLocationIndex + 1 :
                            moveModePlayerLocationIndex - 1;

                        moveModeBasePosition = Input.mousePosition;
                    }
                }
            }
            else
            {
                if (moveSpeedProcess >= 1.0f)
                {
                    moveSpeedProcess = 0;
                    moveState = MoveState.Normal;

                    moveModeRLI = false;
                    moveModeRLIFirstInit = true;
                    Debug.Log("Escape");
                    return;
                }
                moveSpeedProcess += Time.deltaTime * moveSpeedToNormalPos;
                this.gameObject.transform.position = Vector3.Lerp(moveModeRLIPosList[moveModePlayerLocationIndex], constPosition, moveSpeedProcess);

            }
        }

        // First Move
        else
        {
            
            moveSpeedProcess += Time.deltaTime * moveSpeedToNormalPos * 2.0f;
            this.gameObject.transform.position = 
                Vector3.Lerp(moveModeFirstMyPosition,
                moveModeRLIPosList[moveModePlayerLocationIndex], 
                moveSpeedProcess);

            if (moveSpeedProcess >= 1.0f)
            {
                moveSpeedProcess = 0;
                moveModeRLIFirstInit = false;
            }            
        }
    }


    public void InitLineFall(bool active)
    {
        playerMovedInLineFall = active;
        op_isShieldActive = active;
        op_activeShield = !active;
        if (active == true)
        {
            moveState = MoveState.LineFall;
            Debug.Log("init");
        }
        else
        {
            //currentSpeedInLineFall = 0;
        }
    }


    public void MoveModeLineFall()
    {
        if (playerMovedInLineFall == true)
        {
            Vector2 screen = PublicValueStorage.Instance.GetScreenSize();
            if (Input.GetMouseButtonDown(0))
            {
                //Debug.Log("Pos : " + currentPosInLineFall);
                moveModeBasePosition = Input.mousePosition;
            }
            if (Input.GetMouseButton(0) && playerMovedBySlide == false)
            {
                //Debug.Log("mPos : " + screen.x);

                if (Input.mousePosition.x > moveModeBasePosition.x + (screen.x * 10)
                    && lineFallPlayerLocation != LineFallPlayerLocation.Right)
                {
                    //Debug.Log("Right!");
                    nextPosInLineFall = this.gameObject.transform.position;
                    nextPosInLineFall.x += 1.35f;


                    lineFallPlayerLocation += 1;
                    //Debug.Log(lineFallPlayerLocation);

                    playerMovedBySlide = true;
                }
                if (Input.mousePosition.x < moveModeBasePosition.x - (screen.x * 10)
                    && lineFallPlayerLocation != LineFallPlayerLocation.Left)
                {
                    //Debug.Log("Left!");
                    nextPosInLineFall = this.gameObject.transform.position;
                    nextPosInLineFall.x -= 1.35f;


                    lineFallPlayerLocation -= 1;
                    //Debug.Log(lineFallPlayerLocation);


                    playerMovedBySlide = true;
                }
            }
            if (playerMovedBySlide == true)
            {
                if (moveSpeedProcess >= 1.0f && !Input.GetMouseButton(0) && !Input.GetMouseButtonDown(0))
                {
                    moveSpeedProcess = 0;
                    playerMovedBySlide = false;
                    moveModeBasePosition = Input.mousePosition;

                    lastPosInLineFall = this.gameObject.transform.position;
                    return;
                }
                moveSpeedProcess += Time.deltaTime * moveSpeedInMoveMode;
                this.gameObject.transform.position = Vector2.Lerp(this.gameObject.transform.position, nextPosInLineFall, moveSpeedProcess);
            }
        }

        if (playerMovedInLineFall == false)
        {
            //Debug.Log("In");                    
            if (playerAnimationLineFallDeactive == false)
            {
                switch (lineFallPlayerLocation)
                {
                    case LineFallPlayerLocation.Left:
                        playerAnimationLineFallDeactive = true;

                        lineFallPlayerLocation = LineFallPlayerLocation.Middle;
                        break;


                    case LineFallPlayerLocation.Middle:
                        moveSpeedProcess = 0;
                        playerAnimationLineFallDeactive = true;

                        moveSpeedProcess = 1.0f;

                        lineFallPlayerLocation = LineFallPlayerLocation.Middle;
                        break;


                    case LineFallPlayerLocation.Right:
                        playerAnimationLineFallDeactive = true;

                        lineFallPlayerLocation = LineFallPlayerLocation.Middle;
                        break;
                    default:
                        break;
                }
            }
            else
            {
                if (moveSpeedProcess >= 1.0f)
                {
                    moveSpeedProcess = 0;
                    playerAnimationLineFallDeactive = false;
                    moveState = MoveState.Normal;
                    Debug.Log("Escape");
                    return;
                }
                moveSpeedProcess += Time.deltaTime * moveSpeedToNormalPos;
                this.gameObject.transform.position = Vector2.Lerp(lastPosInLineFall, constPosition, moveSpeedProcess);
            }
        }
        //if (Input.touchCount > 0)
        //{
        //    Touch[] touches = Input.touches;
        //    List<Vector2> position = new List<Vector2>();
        //    Vector2[] pos = new Vector2[Input.touchCount];
        //    Debug.Log("move");
        //    for (int i = 0; i < Input.touchCount; i++)
        //    {
        //        switch (touches[i].phase)
        //        {
        //            case TouchPhase.Began:
        //                pos[i] = touches[i].position;
        //                break;
        //            case TouchPhase.Moved:
        //                Debug.Log("pos[0] : " + pos[i]);
        //                Debug.Log("pos2[0] : " + touches[i].position);
        //                break;
        //            case TouchPhase.Ended:
        //            case TouchPhase.Canceled:
        //            case TouchPhase.Stationary:
        //                pos[i] = Vector2.zero;
        //                playerMovedInLineFall = false;
        //                break;
        //            default:
        //                Debug.Log("Error on " + this.name + " : OP_PlaneMove");
        //                break;
        //        }
        //    }
        //}
    }


    // 190502 LifeBalance
    // When gameplay started, player move playerSlot to playerPosition
    public void SetSpawnAnimation(Vector2 playerPosition, float animationSpeed)
    {
        spawnPostion = playerPosition;
        spawnAnimSpeed = animationSpeed;
        spawnAnimSpeed *= -1;
        spawnAnimation = true;
    }

    public void SpawnAnimation()
    {
        if (spawnAnimation == true)
        {
            if (this.gameObject.transform.position.y <= spawnPostion.y)
            {
                this.gameObject.transform.position = spawnPostion;
                spawnAnimation = false;
                activePlayer = true;


                op_shieldOriginalSize = op_playerShield.transform.localScale;
                op_playerShieldGauge = GameObject.FindGameObjectWithTag("PlayerShieldGauge").GetComponent<Slider>();
                op_playerShieldGauge.value = op_playerShieldGauge.maxValue;

                op_CallbackGameStart();
               
                return;
            }
            this.gameObject.transform.Translate(0, spawnAnimSpeed * Time.deltaTime, 0);
        }
    }



    /////////////////////////////////////////////////////////
    /////////////////////////////////////////////////////////
    ///////////////////// Player Shield /////////////////////
    /////////////////////////////////////////////////////////
    /////////////////////////////////////////////////////////

    /// <summary>
    /// Activate Shield
    /// </summary>
    private void OP_ShieldActive(bool active)
    {
        // Shield is working
        if (active == true)
        {
            OP_ShieldUse(active);

            if (op_playerShieldGauge.value <= op_playerShieldOverheatStart)
            {
                op_playerShieldOverheat = true;
                OP_ShieldActive(false);
                UICanvas.Instance.OP_PrintOverheat(true);
                return;
            }
        }
        else // active == false
        {
            OP_ShieldUse(active);
            UICanvas.Instance.OP_PrintOverheat(false);
        }
        op_playerShield.gameObject.SetActive(active);
    }

    /// <summary>
    /// Shield fill or use?
    /// </summary>
    /// <param name="active"></param>
    private void OP_ShieldUse(bool active)
    {
        op_isShieldActive = active;
        if (active == true) // Use
        {
            if(GMshieldFullCharge == true)
            {
                op_playerShieldGauge.value = op_playerShieldGauge.maxValue;
            }
            OP_SetShieldColor(true);
            op_playerShieldGauge.value =
                Mathf.MoveTowards(op_playerShieldGauge.value, op_playerShieldGauge.minValue, Time.deltaTime * op_playerShieldDuration);
        }
        else // Unuse
        {
            OP_SetShieldColor(false);
            op_playerShieldGauge.value =
                Mathf.MoveTowards(op_playerShieldGauge.value, op_playerShieldGauge.maxValue, Time.deltaTime * op_playerShieldDuration);
        }
    }

    /// <summary>
    /// 1/11 여기까지 함
    /// 다음에 할 것 : Enemy 인스턴스로 만들기 (완료)
    /// </summary>
    /// <param name="active"></param>
    private void OP_SetShieldColor(bool active)
    {
            op_playerShieldColor.disabledColor = Color.Lerp(Color.red, Color.green, op_playerShieldGauge.value / op_playerShieldGauge.maxValue);
            op_playerShieldGauge.colors = op_playerShieldColor;
    }

    // 190227 LifeBalance
    // Use Item Effects
    public void OP_BigShield(float multiply)
    {
        op_playerShield.gameObject.transform.localScale *= multiply;
    }
    public void OP_BigShield(Vector3 multiply)
    {
        op_playerShield.gameObject.transform.localScale = multiply;
    }
    // 190227 LifeBalance
    // Use Item Effects
    public void OP_ShieldRecovery(float value)
    {
        op_playerShieldGauge.value += value;
    }

    // 190227 LifeBalance
    // Use Item Effects
    public void OP_ShieldFullCharge(bool active)
    {
        if (active == true) op_playerShieldGauge.value = op_playerShieldGauge.maxValue;
        GMshieldFullCharge = active;
    }

    // 190605 LifeBalance
    // Mode = 1 is Dead by LineFallSquare Attack
    public void ActivatePlayerDie(int mode = 0)
    {
        if (op_isShieldActive == false || mode == 1)
        {
            op_CallbackPlayerDie();
            Destroy(this.gameObject);
        }
    }

    public void ActivateGetItem(int number)
    {
        op_CallbackGetItem(number);
    }


    //
    //
    //
    // Change Player Options 
    //
    //
    //

    public void SetPlayerSprite(Sprite sprite)
    {
        spriteRenderer.sprite = sprite;
    }
}
