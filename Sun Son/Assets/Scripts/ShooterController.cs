﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

// code based off of this video: https://www.youtube.com/watch?v=UjkSFoLxesw
public class ShooterController : MonoBehaviour
{
    [SerializeField] NavMeshAgent agent;
    [SerializeField] Transform player;
    [SerializeField] int health;

    private PlayerMelee _sword;

    //Attacking
    [SerializeField] float timeBetweenAttacks;
    bool alreadyAttacked;
    public GameObject projectile;

    //States
    public float sightRange, attackRange;
    public bool playerInSightRange, playerInAttackRange;

    //Animations
    private Animator _anim;
    private int _isChasing;
    private int _isAttacking;

    public SkinnedMeshRenderer rhead;
    public SkinnedMeshRenderer rbody;
    public SkinnedMeshRenderer rlegs;
    public Material red;
    private Material originalMaterial;
    public GameObject ps;
    public GameObject ps2;

    public AudioClip shoot;
    public AudioClip scream;
    public AudioClip die;

    private void Awake()
    {
        player = FindObjectOfType<PlayerV2>().gameObject.transform;
        agent.GetComponent<NavMeshAgent>();

        _sword = player.GetComponent<PlayerMelee>();
    }

    private void Start()
    {
        transform.position = new Vector3(transform.position.x, transform.position.y, player.transform.position.z);
agent.SetDestination(transform.position);

        _anim = GetComponent<Animator>();

        _isChasing = Animator.StringToHash("Chasing");
        _isAttacking = Animator.StringToHash("RangedAttack");

        originalMaterial = rhead.material;

    }

    private void Update()
    {
        if (health <= 0)
        {
            Invoke(nameof(DestroyEnemy), 0.5f);
        }

        if (Vector3.Distance(transform.position, player.position) < sightRange)
        {
            if (!playerInSightRange)
            {
                GetComponent<AudioSource>().clip = scream;
                GetComponent<AudioSource>().Play();
            }
            playerInSightRange = true;
        }
        else
        {
            playerInSightRange = false;
        }

        if (Vector3.Distance(transform.position, player.position) < attackRange)
        {
            playerInAttackRange = true;
        }
        else
        {
            playerInAttackRange = false;
        }

        if (!playerInSightRange && !playerInAttackRange)
        {
            _anim.SetBool(_isChasing, false);
            _anim.SetBool(_isAttacking, false);
            agent.SetDestination(transform.position);
        }

        if (playerInSightRange && !playerInAttackRange)
        {
            _anim.SetBool(_isChasing, true);
            _anim.SetBool(_isAttacking, false);
            ChasePlayer();
        }

        if (playerInSightRange && playerInAttackRange)
        {
            if (!_anim.GetBool(_isAttacking))
            {
                _anim.SetBool(_isAttacking, true);
                _anim.SetBool(_isChasing, false);
            }
            AttackPlayer();
        }
    }

    private void ChasePlayer()
    {
        agent.SetDestination(player.position);
        agent.speed = 3;
    }

    private void AttackPlayer()
    {
        //make sure enemy doesn't move
        agent.SetDestination(transform.position);
    }

    public void Shoot()
    {
        Vector3 targetPostition = new Vector3(player.position.x,
                                        this.transform.position.y,
                                        player.position.z);
        transform.LookAt(targetPostition);

        Vector3 shootPoint = new Vector3(transform.position.x,
                               transform.position.y + 1, transform.position.z);
        //attack code here
        Rigidbody rb = Instantiate(projectile, shootPoint, Quaternion.identity).GetComponent<Rigidbody>();
        rb.AddForce(transform.forward * 16f, ForceMode.Impulse);
        rb.AddForce(transform.up * 1f, ForceMode.Impulse);

        GetComponent<AudioSource>().clip = shoot;
        GetComponent<AudioSource>().Play();
    }

    public void TakeDamage(int damage)
    {
        health -= damage;

        rhead.materials = new Material[] { red, red, red };
        rbody.materials = new Material[] { red, red };
        rlegs.materials = new Material[] { red, red };

        Invoke("ResetColor", 0.1f);
    }

    void ResetColor()
    {
        rhead.materials = new Material[] { originalMaterial, originalMaterial, originalMaterial };
        rbody.materials = new Material[] { originalMaterial, originalMaterial };
        rlegs.materials = new Material[] { originalMaterial, originalMaterial };
    }

    private void DestroyEnemy()
    {
        Instantiate(ps, transform.position, Quaternion.identity);
        Destroy(gameObject);
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.gameObject.CompareTag("Weapon"))
        {
            if (other.GetComponent<SunBulletController>())
            {
                //bullet damage
                Vector3 targetPos = new Vector3(transform.position.x, other.transform.position.y,
                                transform.position.z - 1);
                Instantiate(ps2, targetPos, Quaternion.LookRotation(transform.forward * 1, Vector3.up));
                TakeDamage(5);
            }
            else
            {
                if (_sword._hit)
                {
                    Vector3 targetPos = new Vector3(transform.position.x, other.transform.position.y,
                                    transform.position.z - 1);
                    Instantiate(ps2, targetPos, Quaternion.LookRotation(transform.forward * 1, Vector3.up));
                    TakeDamage(10);
                    _sword._hit = false;
                }
            }
        }
    }

}
