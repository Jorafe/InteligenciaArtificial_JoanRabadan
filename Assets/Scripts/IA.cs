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

    [SerializeField] Transform[] patrolpoints;

    // Update is called once per frame

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
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
            if(agent.remainingDistance < 0.2f)
            {
                SetRandomPatrolPoint();
            }
        }
        void Chase()
        {
            
        }
        void Searching()
        {

        }

        void SetRandomPatrolPoint()
        {
            agent.destination = patrolpoints[Random.Range(0, patrolpoints.Length)].position;
        }
    
}
