using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class IA : MonoBehaviour
{
    // Start is called before the first frame update
    public enum EnemyState
    {
        Patrolling,
        Chasing,
        Searching
    }

    public EnemyState currentState;

    private NavMeshAgent agent;

    private Transform playertransform;

    [SerializeField] Transform[] patrolpoints;

    [SerializeField] Vector2 patrolAreaSize = new Vector2(5, 5);
    [SerializeField] Transform patrolAreaCenter;

    [SerializeField] float searchRange = 20f;
    [SerializeField] float visionAngle = 90f;

    private Vector3 playerLastPosition;

    // Update is called once per frame

    float searchTimer:
    float searchDuration = 15;
    float searchRadius = 10;


    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        playertransform = GameObject.FindWithTag("Player").transform;
    }

    void Start()    
    {
        currentState = EnemyState.Patrolling;
        SetRandomPatrolPoint();
    }

    void Update()
    {
        switch (currentState)
        {
            case EnemyState.Patrolling:
                Patrol();
                break;
            case EnemyState.Chasing:
                Chase();
                break;
            case EnemyState.Searching:
                Searching();
                break;
        }

    }

        void Patrol()
        {
            if (OnRange())
            {
                currentState = EnemyState.Chasing;
            }
            if(agent.remainingDistance < 0.2f)
            {
                SetRandomPatrolPoint();
            }
        }
        void Chase()
        {
            if (!OnRange())
            {
                currentState = EnemyState.Patrolling;
            }
            agent.destination = playertransform.position;

        }
        void Search()
        {
            if (OnRange())
            {
                currentState = EnemyState.Chasing;
            }

            searchTimer += Time.deltaTime;
            if (searchTimer < searchDuration)
            {
                if(agent.remainingDistance < 0.5f)
                {
                    Vector3 randomPoint;
                    if (RandomSearchPoints(patrolAreaCenter.position, searchRadius, out randomPoint))
                    {
                        agent.destination = randomPoint;
                    }
                }
            }
            else
            {
                currentState = EnemyState.Patrolling;
            }
        }

        bool RandomSearchPoints(Vector3 center, float radius, out Vector3 point)
        {
            Vector3 randomPoint = center + Random.insideUnitCircle * radius;
            NavMeshHit hit;
            if (NavMesh.SamplePosition(randomPoint, out hit, 0.1f, NavMesh.AllAreas))
            {
                point = hit.position;
                return true;
            }
            point = Vector3.zero;
            return false;
        }
        bool OnRange()
        {
            Vector3 directionToPlayer = (playertransform.position - transform.position).normalized;
            float angleToPlayer = Vector3.Angle(transform.forward, directionToPlayer);
            float distanceToPlayer = Vector3.Distance(transform.position, playertransform.position);

            if(playerLastPosition == playertransform.position)
            {
                return true;
            }
            if (distanceToPlayer > searchRange)
            {
                return false;
            }
            if (angleToPlayer > visionAngle * 0.5f)
            {
                return false;
            }

            RaycastHit hit;
            if (Physics.Raycast(transform.position, directionToPlayer, out hit, distanceToPlayer))
            {
                if (hit.transform.tag == "Player")
                {
                   playerLastPosition = playertransform.position;

                   return true;
                }
                else
                {
                    return false;
                }
            }
            return true;
        }

        void SetRandomPatrolPoint()
        {
            //agent.destination = patrolpoints[Random.Range(0, patrolpoints.Length)].position;
            float RandomX = Random.Range(-patrolAreaSize.x / 2, patrolAreaSize.x / 2);
            float RandomZ = Random.Range(-patrolAreaSize.y / 2, patrolAreaSize.y / 2);
            
            Vector3 randomPoint = new Vector3(RandomX, 0, RandomZ) + patrolAreaCenter.position;
        }

        void OnDrawGizmos()
        {
        
        foreach (Transform point in patrolpoints)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(point.position, 1f);
        }

        Gizmos.color = Color.blue;
        Gizmos.DrawWireCube(patrolAreaCenter.position, new Vector3(patrolAreaSize.x, 0, patrolAreaSize.y));

        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, searchRange);

        Gizmos.color = Color.yellow;

        Vector3 fovLine1 = Quaternion.AngleAxis(visionAngle * 0.5f, transform.up) * transform.forward * searchRange;
        Vector3 fovLine2 = Quaternion.AngleAxis(-visionAngle * 0.5f, transform.up) * transform.forward * searchRange;
        
        Gizmos.DrawLine(transform.position, transform.position + fovLine1);
        Gizmos.DrawLine(transform.position, transform.position + fovLine2);

        }
    
}
