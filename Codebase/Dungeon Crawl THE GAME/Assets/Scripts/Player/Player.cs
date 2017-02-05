﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class Player : MonoBehaviour
{
    public AudioClip deathSFX;

    //Basic Settings - Edit in Unity
    public int maxJump = 1;
    public float strafeModfier = .75f;
    public int health = 30;
    public int lives = 3;

    //This is not allowed.
    //public HUD Hud;

    //Variables
    bool invulnerable = false;
    bool burning = false;
    int maxJumpStored;

    //Component References
    Animator anim;
    CharacterController controller;
    GameObject weapon;
    IWeaponBehavior weaponScript;
    GameObject teleportMarker;
    GameObject tpMarker;
    Vector3 tpDestination;
    GameObject rangedScythe;
    GameObject rangedScytheAttack;

    //Physics Settings
    float gravity = 9.8F;
    public float speed = 3.0F;
    public float sprintSpeed = 6.0f;
    public float jumpSpeed = 10.0F;
    public float mass = 20.0F;
    public float rotationSpeed = 5.0f;

    //Physics Internals
    Vector3 moveDirection = Vector3.zero;
    Vector3 Impact = Vector3.zero;
    float verticalVel = 0;
    bool dead = false;

    //Control Settings
    Vector3 mousePosition;
    float mouseDistance;

    //Checkpoints
    Vector3 CurrentCheckpoint;

    //Spin Attack CD
    float spinTime = 0;
    float maxSpinTime = 3;
    float spinCDMax = 8;
    float spinCD = 999;

    //Teleport CD
    float teleportCDMax = 8;
    float teleportCD = 999;

    //Ranged attack
    float rangedAttackTime = 0;

    void OnEnable()
    {
        EventSystem.onMousePositionUpdate += UpdateMousePosition;
    }
    //unsubscribe from player movement
    void OnDisable()
    {
        EventSystem.onMousePositionUpdate -= UpdateMousePosition;
    }

    // Use this for initialization
    void Start()
    {
        anim = GetComponent<Animator>();
        controller = GetComponent<CharacterController>();
        teleportMarker = (GameObject)Resources.Load("Prefabs/Particles/TeleportTarget");
        rangedScythe = (GameObject)Resources.Load("Prefabs/Player/ScytheRangedAttack");
        maxJumpStored = maxJump;
        currentState = States.Idle;
        weapon = FindWeapon(transform);
        weaponScript = weapon.GetComponent<IWeaponBehavior>();
    }

    //States
    enum States
    {
        Idle, MoveForward, Attack, SpinAttack, Die, TakeDamage, Teleport, LowPriorityIdle, ThrowScythe
    }
    States _cs;
    States currentState
    {
        get { return _cs; }
        set
        {

            if (_cs == States.SpinAttack)
            {
                anim.SetBool("Spin Attack", false);
                weaponScript.AttackEnd();
            }
            switch (value)
            {
                case States.Idle:
                    //You can prevent a state assignment with a check here
                    anim.SetBool("Move Forward", false);
                    _cs = value;
                    break;
                case States.MoveForward:
                    if (_cs == States.Idle)
                    {
                        anim.SetBool("Move Forward", true);
                        _cs = value;
                    }
                    break;
                case States.Attack:
                    anim.SetBool("Slash Attack 01", true);
                    weaponScript.AttackStart();
                    _cs = value;
                    break;
                case States.SpinAttack:
                    anim.SetBool("Spin Attack", true);
                    weaponScript.AttackStart();
                    spinTime = 0;
                    spinCD = 0;
                    _cs = value;
                    break;
                case States.Die:
                    if (!dead && lives < 1)
                    {
                        GetComponent<AudioSource>().PlayOneShot(deathSFX);
                        anim.SetBool("Die", true);
                        dead = true;
                        EventSystem.PlayerDeath();
                        _cs = value;
                    }
                    else
                    {
                        lives--;
                        ReturnToCheckpoint();
                        currentState = States.Idle;
                        health = 3;
                    }
                    break;
                case States.TakeDamage:
                    GetComponent<AudioSource>().Play();
                    weaponScript.AttackEnd();
                    anim.SetBool("Take Damage", true);
                    _cs = value;
                    break;
                case States.Teleport:
                    tpDestination = tpMarker.transform.position;
                    Destroy(tpMarker);
                    anim.SetTrigger("Teleport");
                    teleportCD = 0;
                    //_cs = value;
                    break;
                case States.LowPriorityIdle:
                    if (_cs == States.MoveForward)
                    {
                        currentState = States.Idle;
                    }
                    break;
                default:
                    _cs = value;
                    break;
            }
        }
    }


    bool _tT = false;
    bool teleportToggle
    {
        get
        {
            return _tT;
        }
        set
        {
            if (_tT != value)
            {
                _tT = value;
                if (_tT)
                {
                    tpMarker = Instantiate(teleportMarker);
                    //Debug.Log(tpMarker);
                    tpMarker.transform.parent = transform;
                    tpMarker.transform.position = transform.position;
                }
                else
                {
                    currentState = States.Teleport;
                }
            }
        }
    }
    bool _tS = false;
    bool throwScythe
    {
        get
        {
            return _tS;
        }
        set
        {
            if (_tS != value)
            {
                _tS = value;
                if (_tS)
                {
                    rangedAttackTime = 0;
                    weapon.SetActive(false);
                    rangedScytheAttack = Instantiate(rangedScythe);
                    rangedScytheAttack.transform.position = transform.position + transform.up * 2;
                    rangedScytheAttack.transform.rotation = transform.rotation;
                }
                else
                {
                    Destroy(rangedScytheAttack);
                    weapon.SetActive(true);
                }
            }
        }
    }
    // Update is called once per frame
    void Update()
    {
        if (!dead)
        {
            spinCD += Time.deltaTime;
            teleportCD += Time.deltaTime;


            //Re-used a lot of Harrison's movement code
            moveDirection = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));
            mouseDistance = Vector3.Distance(mousePosition, transform.position);
            //Alternate Control Scheme - bad imo
            //moveDirection = transform.TransformDirection(moveDirection);
            //moveDirection *= sprintSpeed;
            moveDirection *= speed;



            if (!throwScythe && (currentState == States.Idle || currentState == States.MoveForward))
            {
                if (Input.GetMouseButton(0))
                {
                    currentState = States.Attack;

                }
                else if (Input.GetMouseButton(1) && spinCD > spinCDMax)
                {
                    currentState = States.SpinAttack;
                }
                else if ( !throwScythe && Input.GetKey(KeyCode.LeftControl) )
                {
                    throwScythe = true;
                }
            }



            if (currentState == States.SpinAttack)
            {
                if (!Input.GetMouseButton(1) || spinTime > maxSpinTime)
                {
                    currentState = States.Idle;
                    weaponScript.AttackEnd();
                    spinCD = 0;
                }
                spinTime += Time.deltaTime;
            }
            else
            {
                if (throwScythe)
                {
                    if (rangedAttackTime > 1.5)
                    {
                        if (Vector3.Distance(transform.position, rangedScytheAttack.transform.position) < 5)
                        {
                            throwScythe = false;
                        }
                    }
                    rangedAttackTime += Time.deltaTime;
                }

                //Strafe and reverse modified with multiplier
                var localVel = transform.InverseTransformDirection(moveDirection);
                if (localVel.z > 0 && localVel.z > localVel.x)
                {
                    currentState = States.MoveForward;
                }
                else
                {
                    currentState = States.LowPriorityIdle;
                    if (localVel.z < 0)
                    {
                        localVel.z = localVel.z * strafeModfier;
                    }
                }
                localVel.x = localVel.x * strafeModfier;
                moveDirection = transform.TransformDirection(localVel);


                if (Impact.magnitude > 0.2)
                {
                    moveDirection += Impact;
                    Impact = Vector3.Lerp(Impact, Vector3.zero, 5 * Time.deltaTime);
                }


                //The character still twitches a bit in very specific positions due to his vertical bobbing
                if (mouseDistance > 1.9f)
                {
                    //Turn player to face cursor on terrain
                    RotateToFaceTarget(mousePosition, rotationSpeed);
                }

            }


            // ^^^ Probably could be done better than this.
            // Agreed

            //Landed / Grounded
            if (controller.isGrounded)
            {
                //anim.SetBool("Jump", false);
                verticalVel = 0;
                maxJump = maxJumpStored;
            }

            //Jump
            if (Input.GetKeyDown(KeyCode.Space) && maxJump > 0)
            {
                maxJump--;
                verticalVel -= jumpSpeed;
            }

            if (teleportCD > teleportCDMax && !teleportToggle && Input.GetKey(KeyCode.Q))
            {
                teleportToggle = true;
            }
            else if (teleportToggle && !(Input.GetKey(KeyCode.Q)))
            {
                teleportToggle = false;
            }

            if (teleportToggle)
            {
                //tpMarker.transform.rotation = transform.rotation;
                float md = (mouseDistance < 40) ? mouseDistance / 2 : 20;
                tpMarker.transform.localPosition = new Vector3(0, mousePosition.y + .2f, md);
            }




            //Gravity
            verticalVel += gravity * Time.deltaTime;
            moveDirection.y -= verticalVel;



            //Move
            controller.Move(moveDirection * Time.deltaTime);
            EventSystem.PlayerPositionUpdate(transform.position);

        }

    }


    public void ResetToIdle()
    {
        currentState = States.Idle;
    }

    void UpdateMousePosition(Vector3 MousePos)
    {
        mousePosition = MousePos;
    }


    public void TakeDamage(int dmg = 1)
    {
        if (!invulnerable)
        {
            StartCoroutine(Invulnerable());
            EventSystem.PlayerHealthUpdate(-dmg);
            health--;

            if (health < 1)
            {

                currentState = States.Die;
            }
            else
            {

                currentState = States.TakeDamage;
            }
        }
    }
    //Use this to send the character flying with a force from a given direction
    public void AddImpact(Vector3 direction, float force)
    {
        direction.Normalize();
        if (direction.y < 0)
            direction.y = -direction.y;
        Impact = direction.normalized * force / mass;
    }

    public void TakeFireDamage()
    {

        if (!burning)
        {
            TakeDamage();
            StartCoroutine("Burning");
        }
    }


    IEnumerator Burning()
    {

        burning = true;
        //Generate Burn Flames
        //GameObject Flames = (GameObject)Instantiate(BurnEffect, transform.position, new Quaternion(0, 45, 45, 0));
        //Attach to player
        //Flames.transform.parent = transform;

        yield return new WaitForSeconds(2);
        //Destroy(Flames);
        burning = false;

    }

    IEnumerator Invulnerable(int seconds = 1)
    {
        invulnerable = true;

        yield return new WaitForSeconds(seconds);

        invulnerable = false;
    }




    public void AttackFinished(int attack)
    {
        if (currentState == States.Attack)
        {

            if (attack == 1)
                anim.SetBool("Slash Attack 01", false);
            else
                anim.SetBool("Slash Attack 02", false);

            currentState = States.Idle;
            weaponScript.AttackEnd();
        }
    }

    public void ResetAttackStack()
    {
        weaponScript.ResetAttack();
    }

    void OnTriggerStay(Collider col)
    {
        if (col.tag == "Trapdoor")
            if (Input.GetKeyDown(KeyCode.E))
                SceneManager.LoadScene("Graveyard");

        if (col.tag == "LastDoor")
            SceneManager.LoadScene("Graveyard");       

    }
    void OnTriggerEnter(Collider col)
    {
        if (col.tag == "ScorePowerUp")
        {

            Destroy(col.gameObject);
            //waiting on fixxed score system 
        }
        else if (col.tag == "HealthPowerUp")
        {
            Destroy(col.gameObject);
            health = 10;
            //Hud.UpdateHealth(health);
        }
        else if (col.tag == "Trapdoor")
            col.gameObject.GetComponent<Animator>().SetBool("Close", false);

        else if (col.tag == "Invulneraball")
        {
            StartCoroutine(Invulnerable(10));
            Destroy(col.gameObject);
        }
        else if (col.tag == "Health Collectible")
        {
            Destroy(col.gameObject);
            health = health + 3;
            //Hud.UpdateHealth(health);
        }
        else if (col.tag == "Checkpoint")
        {
            if (CurrentCheckpoint != col.transform.position)
            {
                CurrentCheckpoint.x = col.transform.position.x;
                CurrentCheckpoint.y = col.transform.position.y + 3;
                CurrentCheckpoint.z = col.transform.position.z;


               // GameObject temp = GameObject.Find("Ground Glow");
                //temp.SetActive(true);

                //GameObject[] terribleStuff;
                //terribleStuff = col.GetComponentsInChildren<GameObject>();
                //foreach (GameObject GO in terribleStuff)
                //    if (GO.name == "Ground Glow")
                //        GO.SetActive(true);
                
                    

               // GameObject.Find("Ground Glow").SetActive(true);




               // col.GetComponentInChildren<ParticleSystem>().Play();
            }
        }
        else if (col.tag == "OutOfBounds")
        {
            ReturnToCheckpoint();
            TakeDamage();
        }
    }

    void ReturnToCheckpoint()
    {
        anim.SetBool("Teleport Appear", true);
        transform.position = CurrentCheckpoint;
        throwScythe = false;
    }
    void OnTriggerExit(Collider col)
    {
        if (col.tag == "Trapdoor")
            col.gameObject.GetComponent<Animator>().SetBool("Close", true);
    }

    void TeleportMove()
    {
        transform.position = tpDestination;
    }

    GameObject FindWeapon(Transform obj)
    {
        foreach (Transform tr in obj)
        {
            if (tr.tag == "Weapon")
            {
                return tr.gameObject;
            }
            if (tr.childCount > 0)
            {
                GameObject temp = FindWeapon(tr);
                if (temp)
                {
                    return temp;
                }
            }
        }
        return null;
    }

    void RotateToFaceTarget(Vector3 _TargetPosition, float _LerpSpeed = 4f, float _AngleAdjustment = -90f)
    {
        Vector3 lookPos = (transform.position - _TargetPosition);
        lookPos.y = 0;
        float angle = Mathf.LerpAngle(transform.rotation.eulerAngles.y, -(Mathf.Atan2(lookPos.z, lookPos.x) * Mathf.Rad2Deg) + _AngleAdjustment, Time.deltaTime * _LerpSpeed);
        //float angle = -(Mathf.Atan2(lookPos.z, lookPos.x) * Mathf.Rad2Deg) - 90;
        transform.rotation = Quaternion.AngleAxis(angle, new Vector3(0, 1, 0));
    }
}