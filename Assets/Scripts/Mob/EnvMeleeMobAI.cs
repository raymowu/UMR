using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.AI;

public class EnvMeleeMobAI : NetworkBehaviour
{
    public NavMeshAgent agent;
    public float rotateSpeedMovement = 0.05f;
    private float rotateVelocity;

    public Animator anim;
    float motionSmoothTime = 0.1f;

    [Header("Enemy Targeting")]
    public GameObject targetEnemy;
    public float stoppingDistance;
    public Vector3 spawnPoint;
    public float detectionRange = 6f;

    void Start()
    {
        agent = gameObject.GetComponent<NavMeshAgent>();
    }

    void Update()
    {
        if (!IsOwner) { return; }
        Animation();
        Move();
    }

    private GameObject nearestPlayer()
    {
        GameObject tMin = null;
        float minDist = Mathf.Infinity;
        Vector3 currentPos = transform.position;
        foreach (GameObject t in GameManager.Instance.playerPrefabs)
        {
            float dist = Vector3.Distance(t.transform.position, currentPos);
            if (dist < minDist)
            {
                tMin = t;
                minDist = dist;
            }
        }
        return tMin;
    }

    public void Animation()
    {
        float speed = agent.velocity.magnitude / agent.speed;
        anim.SetFloat("Speed", speed, motionSmoothTime, Time.deltaTime);
    }

    public void Move()
    {
        GameObject targetEnemy = nearestPlayer();
        if (targetEnemy != null && Vector3.Distance(spawnPoint, targetEnemy.transform.position) <= detectionRange)
        {
            MoveTowardsEnemy(targetEnemy);
        }
        else
        {
            MoveBackToSpawn(spawnPoint);
        }
    }

    public void MoveToPosition(Vector3 position)
    {
        agent.SetDestination(position);
        agent.stoppingDistance = 0;
        Rotate(position);
        if (targetEnemy != null)
        {
            targetEnemy = null;
        }
    }

    public void MoveBackToSpawn(Vector3 spawnPoint)
    {
        agent.SetDestination(spawnPoint);
        Rotate(spawnPoint);
    }

    public void MoveTowardsEnemy(GameObject enemy)
    {
        targetEnemy = enemy;
        agent.SetDestination(targetEnemy.transform.position);
        agent.stoppingDistance = stoppingDistance;

        Rotate(targetEnemy.transform.position);
    }
    public void StopMovement()
    {
        agent.SetDestination(agent.transform.position);
        agent.stoppingDistance = 0;
    }

    public void Rotate(Vector3 lookAtPosition)
    {
        Quaternion rotationToLookAt = Quaternion.LookRotation(lookAtPosition - transform.position);
        float rotationY = Mathf.SmoothDampAngle(transform.eulerAngles.y, rotationToLookAt.eulerAngles.y,
            ref rotateVelocity, rotateSpeedMovement * (Time.deltaTime * 5));

        transform.eulerAngles = new Vector3(0, rotationY, 0);
    }
}
