using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Random = UnityEngine.Random;

public class EnemyAI : MonoBehaviour
{
    public NavMeshAgent agent;

    public Transform player;

    public LayerMask whatIsGround, whatIsPlayer, whatIsObstacle;

    public float health;

    public Animator anim;

    public float waterLevel;

    //Patrulhar
    public Vector3 walkPoint;
    public bool walkPointSet;
    public float walkPointRange;

    //Atacar
    public float timeBetweenAttacks;
    bool alreadyAttacked;
    public GameObject projectile;

    //Estados
    public float sightRange, attackRange;
    public bool playerInSightRange, playerInAttackRange, isIdle, isRunning, isPatroling;

    private void Awake()
    {
        player = GameObject.Find("Player(Clone)").transform;
        agent = GetComponent<NavMeshAgent>();
    }

    private void Update()
    {
        //Verificar o alcance de visão e ataque
        playerInSightRange = Physics.CheckSphere(transform.position, sightRange, whatIsPlayer);
        playerInAttackRange = Physics.CheckSphere(transform.position, attackRange, whatIsPlayer);

        if (!playerInSightRange && !playerInAttackRange && !isIdle)
        {
            anim.SetTrigger("walk");
            isPatroling = true;
            isRunning = false;
        }
        
        if (playerInSightRange)
        {
            anim.SetTrigger("run");
            isRunning = true;
            agent.speed = 7f;
            agent.angularSpeed = 200f;

            if (playerInAttackRange)
            {
                agent.speed = 12f;
                agent.angularSpeed = 200f;
                agent.acceleration = 12f;
            }
        }

        if (isRunning)
            Run();
        else if (isPatroling)
            Patroling();
        else
            StartCoroutine(Idling());



        //if (playerInSightRange && playerInAttackRange) AttackPlayer();

    }

    private void Patroling()
    {
        anim.SetBool("isIdle", false);
        anim.SetBool("isRunning", false);
        anim.SetBool("isWalking", true);

        agent.speed = 3.5f;

        agent.angularSpeed = 120f;

        if (!walkPointSet) SearchWalkPoint();

        if (walkPointSet)
            agent.SetDestination(walkPoint);

        Vector3 distanceToWalkPoint = transform.position - walkPoint;

        //Chegou ao ponto
        if (distanceToWalkPoint.magnitude < 5f)
        {
            isIdle = true;
            isPatroling = false;
        }
    }

    IEnumerator Idling()
    {
        anim.SetBool("isIdle", true);
        anim.SetBool("isRunning", false);
        anim.SetBool("isWalking", false);

        if (isRunning)
        {
            Run();
            yield return null;
        }

        yield return new WaitForSeconds(10f);

        isIdle = false;
        isPatroling = true;
        walkPointSet = false;
    }

    private void SearchWalkPoint()
    {
        NavMeshPath navMeshPath = new NavMeshPath();

        //Calcular um ponto aleatório dentro do alcance
        float randomZ = Random.Range(-walkPointRange, walkPointRange);
        float randomX = Random.Range(-walkPointRange, walkPointRange);

        walkPoint = new Vector3(transform.position.x + randomX, transform.position.y, transform.position.z + randomZ);

        if (agent.CalculatePath(walkPoint, navMeshPath) && navMeshPath.status == NavMeshPathStatus.PathComplete && walkPoint.y >= waterLevel)
            walkPointSet = true;
    }

    private void ChasePlayer()
    {
        agent.SetDestination(player.position);
    }

    private void Run()
    {
        isPatroling = false;
        isIdle = false;

        anim.SetBool("isIdle", false);
        anim.SetBool("isRunning", true);
        anim.SetBool("isWalking", false);

        Vector3 dirToPlayer = transform.position - player.transform.position;

        Vector3 newPos = transform.position + dirToPlayer;

        agent.SetDestination(newPos);
    }

    private void AttackPlayer()
    {
        //Certificar que o inimigo não se move
        agent.SetDestination(transform.position);

        transform.LookAt(player);

        if (!alreadyAttacked)
        {
            //Código de ataque aqui
            Rigidbody rb = Instantiate(projectile, transform.position, Quaternion.identity).GetComponent<Rigidbody>();
            rb.AddForce(transform.forward * 32f, ForceMode.Impulse);
            rb.AddForce(transform.up * 8f, ForceMode.Impulse);
            //Fim do código de ataque

            alreadyAttacked = true;
            Invoke(nameof(ResetAttack), timeBetweenAttacks);
        }
    }

    private void ResetAttack()
    {
        alreadyAttacked = false;
    }

    public void TakeDamage(int damage)
    {
        health -= damage;

        if (health <= 0) Invoke(nameof(DestroyEnemy), 0.5f);
    }

    private void DestroyEnemy()
    {
        Destroy(gameObject);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, sightRange);
    }
}
