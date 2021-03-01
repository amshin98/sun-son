﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

//majority of code from:
//https://unity.grogansoft.com/navigation-with-the-nav-mesh-part-4-patrolling-and-chasing/

public class CrawlerController : MonoBehaviour
{
    [SerializeField] int _damageCost = 2;
    [SerializeField] float _targetRange = 5;
    [SerializeField] float _attackRange = 0.5f;
    [SerializeField] float _decisionDelay = 3f;

    [SerializeField] int _health = 5;


    [SerializeField] Transform[] targets;
    [SerializeField] EnemyStates currentState;

    private GameObject _player;
    private PlayerShield _shield;
    private PlayerMelee _sword;
    public bool _swordAttacked;
    private int currentTarget = 0;

    
    NavMeshAgent agent;

    private Animator _anim;
    private int _isWalkingHash;
    private int _isAttackingHash;

    public SkinnedMeshRenderer rbody;
    public Material red;
    private Material originalMaterial;

    public GameObject ps;



    enum EnemyStates
    {
        Patrolling,
        Chasing
    }

    // Start is called before the first frame update
    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        InvokeRepeating("SetDestination", 1.5f, _decisionDelay);
        //agent.SetDestination(targets.position);
        _player = FindObjectOfType<PlayerV2>().gameObject;
        _shield = _player.GetComponent<PlayerShield>();
        _sword = _player.GetComponent<PlayerMelee>();

        if (currentState == EnemyStates.Patrolling)
        {
            agent.SetDestination(targets[currentTarget].position);
        }

        _anim = GetComponent<Animator>();

        _isWalkingHash = Animator.StringToHash("IsWalking");
        _isAttackingHash = Animator.StringToHash("IsAttacking");
    }

    // Update is called once per frame
    void Update()
    {
        _swordAttacked = _sword._isAttacking;

        if (_health <= 0)
        {
            DestroyEnemy();
        }

        if (Vector3.Distance(transform.position, _player.transform.position) > _targetRange)
        {
            currentState = EnemyStates.Patrolling;
            _anim.SetBool(_isWalkingHash, true);
            _anim.SetBool(_isAttackingHash, false);
        }
        else if (Vector3.Distance(transform.position, _player.transform.position) < _attackRange)
        {
            agent.SetDestination(transform.position);
            _anim.SetBool(_isWalkingHash, false);
            _anim.SetBool(_isAttackingHash, true);
        }
        else
        {
            currentState = EnemyStates.Chasing;
            _anim.SetBool(_isWalkingHash, true);
            _anim.SetBool(_isAttackingHash, false);
        }

        if (currentState == EnemyStates.Patrolling)
        {
            if (Vector3.Distance(transform.position, targets[currentTarget].position) < 0.2f)
            {
                currentTarget++;
                if (currentTarget == targets.Length)
                {
                    currentTarget = 0;
                }
            }
            agent.SetDestination(targets[currentTarget].position);
        }
    }

    void SetDestination()
    {
        if (currentState == EnemyStates.Chasing)
        {
            agent.SetDestination(_player.transform.position);
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            _anim.SetBool(_isWalkingHash, false);
            _anim.SetBool(_isAttackingHash, true);

        }
    }

    public void Attack(GameObject obj)
    {
        Vector3 targetPosition = new Vector3(transform.position.x,
                                        transform.position.y + 0.2f,
                                        transform.position.z);
        if (!_shield._shieldPressed)
        {
            _player.GetComponent<PlayerResources>().TakeDamage(_damageCost);

            Instantiate(obj, targetPosition, Quaternion.LookRotation(transform.forward * -1, Vector3.up));
        }
        GetComponent<AudioSource>().Play();
    }

    public void TakeDamage(int damage)
    {
        _health -= damage;

        rbody.materials = new Material[] { red };

        Invoke("ResetColor", 0.1f);

        if (_health <= 0)
        {
            Invoke(nameof(DestroyEnemy), 0.5f);
        }
    }

    void ResetColor()
    {
        rbody.materials = new Material[] { originalMaterial };
    }

    private void DestroyEnemy()
    {
        Instantiate(ps, transform.position, Quaternion.identity);
        Destroy(gameObject);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Weapon"))
        {
            if (other.GetComponent<SunBulletController>())
            {
                //bullet damage
                TakeDamage(5);
            }
            else
            {
                if (_sword._isAttacking && _swordAttacked)
                {
                    TakeDamage(5);
                    _sword._isAttacking = false;
                }
            }
        }
    }

}
