﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MissileForEnemyTwo : MonoBehaviour
{

    public GameObject player;
    public GameObject missileExplosion;
    //private Vector2 playerPos;

    [Header("- Animation")]
    public Animator animator;
    public int animatorNumber = 1;


    private Vector2 parentPos;
    public OPCurves opCurves;
    public TrailRenderer enemyTrail;

    public float ChaseTimerMin = 1.5f;
    public float ChaseTimerMax = 2.5f;


    public bool startChase = false;
    private float ChaseTimer = 2.5f;
    private float ChaseTimerCheck = 0.0f;
    public float missileMaxSpeed = 10.0f;
    public float missileCurrentSpeed = 0.0f;
    public float missileReflectSpeed = 2.0f;

    //private float distance;
    //private Vector2 heading; 
    private Vector2 direction;
    private Vector2 bezierStart, bezierCenter, bezierEnd;
    public float bezierSpeed = 0.0f;

    private SpriteRenderer missileColor;

    private void OnDestroyAllObject()
    {
        Destroy(this.gameObject);
    }
    private void OnEnable()
    {
        GameManager.onDestroyAllObject += OnDestroyAllObject;
        GameManager.onPlayerDie += OnPlayerDie;
        GameManager.onDestroyAllEnemy += OnPlayerDie;

        animator.SetInteger("MissileAnimTrigger", animatorNumber);
    }
    // Use this for initialization
    void Start()
    {

        enemyTrail.enabled = false;

        parentPos = this.transform.position;

        //opCurves = this.gameObject.GetComponent<OPCurves>();

        bezierStart = bezierCenter = bezierEnd = this.gameObject.transform.position;
        bezierEnd.x += Random.Range(-1.0f, 1.0f);
        bezierEnd.y += Random.Range(0.1f, -0.3f);
        bezierCenter.x = (bezierEnd.x + bezierStart.x) * 0.5f;
        bezierCenter.y += Random.Range(0.2f, 0.5f);

        ChaseTimer = Random.Range(ChaseTimerMin, ChaseTimerMax);

        this.gameObject.SetActive(true);

        //heading = player.transform.position - this.transform.position;
        //distance = heading.magnitude;
        //direction = heading / distance;
    }

    // Update is called once per frame
    void Update()
    {
        if (startChase == false)
        {
            bezierSpeed += Time.deltaTime;
            opCurves.Bezier2DLerp(this.gameObject, bezierStart, bezierCenter, bezierEnd, bezierSpeed);
            if (bezierSpeed >= 1.0f)
            {
                //direction = opCurves.SeekDirectionToPlayer(this.gameObject.transform.position, GameManager.Instance.GetPlayerPosition());
                direction = opCurves.SeekDirection(this.gameObject.transform.position, PublicValueStorage.Instance.GetPlayerPos());
                startChase = true;
            }
        }
        else // startChase == true
        {
            if (ChaseTimerCheck <= ChaseTimer)
            {
                ChaseTimerCheck += Time.deltaTime;
                if (ChaseTimerCheck <= ChaseTimer - 1.0f)
                {
                    this.transform.GetChild(0).transform.Rotate(0, 0, 80f);
                }
            }
            // Chase Start !
            else
            {
                this.transform.Translate(direction * Time.deltaTime * missileCurrentSpeed);
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.tag == "WasteBasket")
        {
            Destroy(this.gameObject);
        }
        if (this.tag == "Missile")
        {
            switch (collision.tag)
            {
                case "Player":
                    //case "WasteBasket":
                    Instantiate(missileExplosion, this.transform.position, Quaternion.identity);
                    PublicValueStorage.Instance.GetPlayerComponent().ActivatePlayerDie();
                    Destroy(this.gameObject);
                    break;

                case "PlayerShield":
                    this.tag = "PlayerMissile";
                    this.name = "PlayerMissile";
                    enemyTrail.enabled = true;
                    animator.SetInteger("MissileAnimTrigger", 2);
                    //Debug.Log(missileColor);
                    //GameManager.Instance.ScoreAdd("Missile");
                    PublicValueStorage.Instance.AddMissileScore();
                    //direction = opCurves.SeekDirectionToPlayer(this.gameObject.transform.position, GameManager.Instance.GetRandomEnemyPos(parentPos));
                    direction = opCurves.SeekDirection(this.gameObject.transform.position, PublicValueStorage.Instance.GetRandomEnemyPos());
                    //missileCurrentSpeed *= missileReflectSpeed;
                    break;
            }
        }
        else if (this.tag == "PlayerMissile")
        {
            switch (collision.tag)
            {
                case "Enemy":
                    Instantiate(missileExplosion, this.transform.position, Quaternion.identity);
                    Destroy(this.gameObject);
                    break;
            }
        }
    }

    public void OnPlayerDie()
    {
        Destroy(this.gameObject);
    }

    private void OnDisable()
    {
        GameManager.onDestroyAllObject -= OnDestroyAllObject;
        GameManager.onPlayerDie -= OnPlayerDie;
        GameManager.onDestroyAllEnemy -= OnPlayerDie;
    }
}
