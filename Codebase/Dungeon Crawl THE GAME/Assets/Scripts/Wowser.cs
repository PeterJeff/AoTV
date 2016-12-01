﻿using UnityEngine;
using System.Collections;


public enum BossStates
{
    Idle,
    Moving,
    Stomp,
    FireBreath,
    Stunned

}

public class Wowser : MonoBehaviour
{

    float tail;
    bool IsIdle = true;
    bool IsStunned = false;
    bool lowHP = false;
    public BossStates CurrentState = BossStates.Idle;
    float timeElapsed = 0.0f;
    Vector3 Position;
    public GameObject Mario;
    public GameObject wowser;
    public GameObject arena;
    void Start()
    {

    }

    void Update()
    {
        Position = transform.position;
            switch (CurrentState)
        {
            case BossStates.Idle:
                IdleState();
                break;
            case BossStates.Moving:
                MovingState();
                break;
            case BossStates.Stomp:
                StompState();
                break;
            case BossStates.FireBreath:
                FirebreathState();
                break;
            case BossStates.Stunned:
                StunndedState();
                break;
            default:
                break;
        }
    }
    void IdleState()
    {
        //controls duration of IdleState // change hard coded 1 eventually
        if(timeElapsed>1)
        {
            CurrentState = BossStates.Moving;
            timeElapsed = 0;
        }
       
        while(timeElapsed<1)
        {
            timeElapsed += Time.deltaTime;
            continue;
        }
       

    }
    void MovingState()
    {
     
    }
    void StompState()
    {
        int r = 1;
        while (timeElapsed < 1.5)
        {
            timeElapsed += Time.deltaTime;
        }
       if (timeElapsed >=1.5)
        {

            if (true)
            { }
            //stomp circle code here
        }



    }
    void FirebreathState()
    {
        

    }
    void StunndedState()
    {
 
        int stunDuration = 3;
        if (timeElapsed > stunDuration )
        {
            CurrentState = BossStates.Moving;
            timeElapsed = 0;
        }

        while (timeElapsed < 1)
        {
            timeElapsed += Time.deltaTime;
            continue;
        }

    }

    void Follow()
    {
        arena.GetComponent<NavMeshAgent>();

        while (CurrentState == BossStates.Idle)
        {
           


        }
    }
    public void SetCurrentState(BossStates NewState)
    {
        CurrentState = NewState;
    }
}