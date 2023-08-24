using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.AI;

public class PlayerMovement : NetworkBehaviour
{
    public NavMeshAgent agent;
    public float rotateSpeedMovement = 0.05f;
    private float rotateVelocity;

    public Animator anim;
    float motionSmoothTime = 0.1f;

    [Header("Enemy Targeting")]
    public GameObject targetEnemy;
    public float stoppingDistance;
    private HighlightManager hmScript;

    void Start()
    {
        agent = gameObject.GetComponent<NavMeshAgent>();
        hmScript = GetComponent<HighlightManager>();
    }

    void Update()
    {

        if (!IsOwner) { return; }

        if (GetComponent<PlayerPrefab>().IsDead)
        {
            anim.SetBool("isDead", true);
            hmScript.DeselectHighlight();
            targetEnemy = null;
            return;
        }
        else
        {
            anim.SetBool("isDead", false);
        }

        if (GetComponent<PlayerPrefab>().CurrentMovementSpeed <= 0.1f) {
            anim.SetFloat("Speed", 0);
            return;  
        }


        Animation();
        Move();
    }

    public void Animation()
    {
        float speed = agent.velocity.magnitude / agent.speed;
        anim.SetFloat("Speed", speed, motionSmoothTime, Time.deltaTime);
    }

    public void Move()
    {
        if (Input.GetMouseButtonDown(1))
        {
            RaycastHit hit;

            //TODO: change Mathf.Infinity to whatever fixed var it should be
            if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit, Mathf.Infinity))
            {
                // TODO: OR compare tag "Enemy" for monsters
                if (hit.collider.CompareTag("Player")) { //&& hit.collider.gameObject != gameObject
                    MoveTowardsEnemy(hit.collider.gameObject);
                }
                else
                {
                    MoveToPosition(hit.point);
                }
            }
        }
        // Stop when at the max attack range
        if (targetEnemy != null)
        {
            if (Vector3.Distance(transform.position, targetEnemy.transform.position) > stoppingDistance)
            {
                agent.SetDestination(targetEnemy.transform.position);
            }
        }
    }

    public void MoveToPosition(Vector3 position)
    {
        agent.SetDestination(position);
        agent.stoppingDistance = 0;
        Rotate(position);
        if (targetEnemy != null)
        {
            hmScript.DeselectHighlight();
            targetEnemy = null;
        }
    }

    public void MoveTowardsEnemy(GameObject enemy)
    {
        targetEnemy = enemy;
        agent.SetDestination(targetEnemy.transform.position);
        agent.stoppingDistance = stoppingDistance;

        Rotate(targetEnemy.transform.position);

        hmScript.SelectedHighlight(); 
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
