using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class EnemyAI : MonoBehaviour
{
    public enum EnemyState
    {
        Patrolling,
        Chasing,
        Searching,
        Waiting,
        Attacking
    }

    public EnemyState currentState;

    private NavMeshAgent _AIAgent;
    private Transform _playerTransform;

    // Puntos de patrulla
    [SerializeField] Transform[] _patrolPoints;
    private int _currentPatrolIndex = 0;

    // Cosas de detección
    [SerializeField] float _visionRange = 20;
    [SerializeField] float _visionAngle = 120;
    private Vector3 _playerLastPosition;

    // Cosas de búsqueda
    float _searchTimer;
    float _searchWaitTime = 15;
    float _searchRadius = 10;

    // Cosas de espera
    float _waitTimer;
    float _waitTime = 5;

    void Awake()
    {
        _AIAgent = GetComponent<NavMeshAgent>();
        _playerTransform = GameObject.FindWithTag("Player").transform;
    }

    void Start()
    {
        currentState = EnemyState.Patrolling;
        SetNextPatrolPoint();
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
                Search();
                break;
            case EnemyState.Waiting:
                Wait();
                break;
            case EnemyState.Attacking:
                Attack();
                break;
        }
    }

    void Patrol()
    {
        if (OnRange())
        {
            currentState = EnemyState.Chasing;
            return;
        }

        if (_AIAgent.remainingDistance < 0.5f)
        {
            currentState = EnemyState.Waiting;
            _waitTimer = 0;
        }
    }

    void Wait()
    {
        _waitTimer += Time.deltaTime;

        if (_waitTimer >= _waitTime)
        {
            currentState = EnemyState.Patrolling;
            SetNextPatrolPoint();
        }
    }

    void Chase()
    {
        if (!OnRange())
        {
            currentState = EnemyState.Searching;
            return;
        }

        if (Vector3.Distance(transform.position, _playerTransform.position) < 2.0f)
        {
            currentState = EnemyState.Attacking;
        }
        else
        {
            _AIAgent.destination = _playerTransform.position;
        }
    }

    void Search()
    {
        if (OnRange())
        {
            currentState = EnemyState.Chasing;
            return;
        }

        _searchTimer += Time.deltaTime;

        if (_searchTimer < _searchWaitTime)
        {
            if (_AIAgent.remainingDistance < 0.5f)
            {
                Vector3 randomPoint;
                if (RandomSearchPoint(_playerLastPosition, _searchRadius, out randomPoint))
                {
                    _AIAgent.destination = randomPoint;
                }
            }
        }
        else
        {
            currentState = EnemyState.Patrolling;
            _searchTimer = 0;
        }
    }

    void Attack()
    {
        Debug.Log("Enemy is attacking!");
        currentState = EnemyState.Chasing;
    }

    bool RandomSearchPoint(Vector3 center, float radius, out Vector3 point)
    {
        Vector3 randomPoint = center + Random.insideUnitSphere * radius;

        NavMeshHit hit;
        if (NavMesh.SamplePosition(randomPoint, out hit, 4, NavMesh.AllAreas))
        {
            point = hit.position;
            return true;
        }

        point = Vector3.zero;
        return false;
    }

    bool OnRange()
    {
        Vector3 directionToPlayer = _playerTransform.position - transform.position;
        float angleToPlayer = Vector3.Angle(transform.forward, directionToPlayer);
        float distanceToPlayer = Vector3.Distance(transform.position, _playerTransform.position);

        if (_playerTransform.position == _playerLastPosition)
        {
            return true;
        }

        if (distanceToPlayer > _visionRange)
        {
            return false;
        }

        if (angleToPlayer > _visionAngle * 0.5f)
        {
            return false;
        }

        RaycastHit hit;
        if (Physics.Raycast(transform.position, directionToPlayer, out hit, distanceToPlayer))
        {
            if (hit.collider.CompareTag("Player"))
            {
                _playerLastPosition = _playerTransform.position;
                return true;
            }
            else
            {
                return false;
            }
        }

        return true;
    }

    void SetNextPatrolPoint()
    {
        if (_patrolPoints.Length == 0)
        {
            return;
        }

        _AIAgent.destination = _patrolPoints[_currentPatrolIndex].position;

        // Incrementar el índice y reiniciarlo si alcanza el final
        _currentPatrolIndex = (_currentPatrolIndex + 1) % _patrolPoints.Length;
    }
}

