﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class QueenWormController : MonoBehaviour
{

    enum QueenState
    {
        Idle,
        Move,
        ClawAttack,
        BiteAttack,
        CastSpell,
        BreathAttackStart,
        BreathAttackLoop,
        BreathAttackEnd,
        Summon,
        Defend,
        TakeDamage,
        Death
    }

    QueenState qs;
    Animator anim;
    float PlayerDist;
    Vector3 PlayerPos;
    bool defendTime;
    float idleTime;
    NavMeshAgent navigate;

    QueenState currentState
    {
        get { return qs; }
        set
        {
            switch (value)
            {
                case QueenState.Idle:
                    navigate.speed = 0;
                    navigate.Stop();
                    idleTime = 0;
                    qs = value;
                    break;
                case QueenState.Move:
                    anim.SetBool("Move", true);
                    navigate.Resume();
                    navigate.speed = 8f;
                    qs = value;
                    break;
                case QueenState.ClawAttack:
                    anim.SetBool("Claw Attack", true);
                    navigate.speed = 0;
                    navigate.Stop();
                    idleTime = 0;
                    qs = value;
                    break;
                case QueenState.BiteAttack:
                    anim.SetBool("Bite Attack", true);
                    navigate.speed = 0;
                    navigate.Stop();
                    idleTime = 0;
                    qs = value;
                    break;
                case QueenState.CastSpell:
                    anim.SetBool("Cast Spell", true);
                    navigate.speed = 0;
                    navigate.Stop();
                    idleTime = 0;
                    qs = value;
                    break;
                case QueenState.BreathAttackStart:
                    anim.SetBool("BreathAttack", true);
                    navigate.speed = 0;
                    navigate.Stop();
                    idleTime = 0;
                    qs = value;
                    break;
                case QueenState.BreathAttackLoop:
                    qs = value;
                    break;
                case QueenState.BreathAttackEnd:
                    anim.SetBool("Bite Attack", false);
                    qs = value;
                    break;
                case QueenState.Summon:
                    anim.SetBool("Summon", true);
                    navigate.speed = 0;
                    navigate.Stop();
                    idleTime = 0;
                    qs = value;
                    break;
                case QueenState.Defend:
                    anim.SetBool("Defend", true);
                    navigate.speed = 0;
                    navigate.Stop();
                    idleTime = 0;
                    qs = value;
                    break;
                case QueenState.TakeDamage:
                    anim.SetBool("Take Damage", true);
                    qs = value;
                    break;
                case QueenState.Death:
                    navigate.speed = 0f;
                    navigate.enabled = false;
                    anim.SetBool("Die", true);
                    qs = value;
                    break;
            }
        }
    }

    // Use this for initialization
    void Start()
    {
        navigate = GetComponent<NavMeshAgent>();
        currentState = QueenState.Idle;

    }

    // Update is called once per frame
    void Update()
    {
        PlayerDist = Vector3.Distance(PlayerPos, transform.position);

        switch (currentState)
        {
            case QueenState.Idle:
                if (idleTime > 1f)
                {
                    if (PlayerDist <= 4f)
                    {
                        if (defendTime)
                        {
                            currentState = QueenState.Defend;
                            defendTime = false;
                        }
                        else
                        {
                            currentState = QueenState.BiteAttack;
                            defendTime = true;
                        }
                    }
                    else if (PlayerDist <= 10f)
                        currentState = QueenState.ClawAttack;
                }
                if (idleTime > 3f)
                    idleTime = 0;
                idleTime += Time.deltaTime;
                break;

            case QueenState.Move:
                navigate.SetDestination(PlayerPos);
                if (PlayerDist <= 4f)
                {
                    currentState = QueenState.BiteAttack;
                    anim.SetBool("Walk", false);
                }
                else if (PlayerDist <= 10f)
                {
                    currentState = QueenState.ClawAttack;
                    anim.SetBool("Walk", false);
                }
                break;

            case QueenState.ClawAttack:
                if (idleTime > 1f)
                {
                    currentState = QueenState.Idle;
                    anim.SetTrigger("Claw Attack");
                }
                else
                    idleTime += Time.deltaTime;
                break;
            case QueenState.BiteAttack:
                if (idleTime > 1f)
                {
                    currentState = QueenState.Idle;
                    anim.SetBool("Bite Attack", false);
                }
                else
                    idleTime += Time.deltaTime;
                break;
            case QueenState.CastSpell:
                if (idleTime > 1f)
                {
                    currentState = QueenState.Idle;
                    anim.SetBool("Cast Spell", false);
                }
                else
                    idleTime += Time.deltaTime;
                break;
            case QueenState.BreathAttackStart:
                break;
            case QueenState.BreathAttackLoop:
                break;
            case QueenState.BreathAttackEnd:
                break;
            case QueenState.Summon:
                break;
            case QueenState.Defend:
                break;
            case QueenState.TakeDamage:
                break;
            case QueenState.Death:
                break;
        }
    }

    void OnEnable()
    {
        EventSystem.onPlayerPositionUpdate += UpdateTargetPosistion;
    }

    void OnDisable()
    {
        EventSystem.onPlayerPositionUpdate -= UpdateTargetPosistion;
    }

    void UpdateTargetPosistion(Vector3 pos)
    {
        PlayerPos = pos;
    }
}
