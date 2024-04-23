using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine.AI;

public class FSMSentinel : MonoBehaviour
{
    public float reactionTime = 1f;
    public string targetTag = "Player";

    [Header("Alarm")] public Light alarmLight;
    public Color alarmColor = Color.red;
    public Color searchColor = Color.yellow;

    [Header("Patrol Positions")] public Transform[] patrolPositions = null;

    [SerializeField] public LayerMask obstructionMask;

    private FSM _fsm;
    private Color _baseColor;
    private NavMeshAgent _agent;
    private FieldOfVIew _fieldOfView;
    private Transform _positionTarget;
    private Vector3 _alarmTargetPosition;
    private Vector3 _alarmTargetPreviousPosition = Vector3.zero;
    private int _currentPatrolPositionIndex;

    void Start()
    {
        _baseColor = alarmLight.color;
        _agent = GetComponent<NavMeshAgent>();
        _fieldOfView = GetComponentInChildren<FieldOfVIew>();

        if (patrolPositions.Length > 0)
        {
            _positionTarget = patrolPositions[_currentPatrolPositionIndex];
        }

        FSMState patrolState = new FSMState();
        patrolState.EnterActions.Add(PositionDestination);

        FSMState alarmState = new FSMState();
        alarmState.EnterActions.Add(AlarmColor);
        alarmState.StayActions.Add(AlarmFollowTarget);
        //alarmState.ExitActions.Add(AlarmColorReset);
        alarmState.ExitActions.Add(FigureOutNewPosition);

        FSMState searchState = new FSMState();
        searchState.EnterActions.Add(SearchColor);
        searchState.ExitActions.Add(AlarmColorReset);

        FSMTransition positionReachedTransition =
            new FSMTransition(PositionReached, new FSMAction[] { NextPosition });
        FSMTransition enemyVisibleTransition =
            new FSMTransition(IsEnemyVisible);
        FSMTransition notEnemyVisibleTransition =
            new FSMTransition(NotIsEnemyVisible, new FSMAction[] { NearestPosition });
        FSMTransition enemyLostTransition =
            new FSMTransition(EnemyLost, new FSMAction[] { NearestPosition });

        patrolState.AddTransition(enemyVisibleTransition, alarmState);
        patrolState.AddTransition(positionReachedTransition, patrolState);
        alarmState.AddTransition(notEnemyVisibleTransition, searchState);
        searchState.AddTransition(enemyLostTransition, patrolState);
        searchState.AddTransition(enemyVisibleTransition, alarmState);

        _fsm = new FSM(patrolState);

        StartCoroutine(Patrol());
    }

    private IEnumerator Patrol()
    {
        while (true)
        {
            _fsm.Update();
            yield return new WaitForSeconds(reactionTime);
        }
    }

    // ACTIONS

    private void AlarmColorReset()
    {
        alarmLight.color = _baseColor;
    }

    private void AlarmColor()
    {
        alarmLight.color = alarmColor;
    }

    private void SearchColor()
    {
        alarmLight.color = searchColor;
    }

    private void AlarmFollowTarget()
    {
        _agent.SetDestination(_alarmTargetPosition);
    }

    private void FigureOutNewPosition()
    {
        Vector3 newPosition = _alarmTargetPosition + (_alarmTargetPosition - _alarmTargetPreviousPosition) * reactionTime;
        _alarmTargetPosition = newPosition;
    }

    private void PositionDestination()
    {
        _agent.SetDestination(_positionTarget.position);
    }

    private bool PositionReached()
    {
        Vector3 targetPosition = _positionTarget.position;
        targetPosition.y = transform.position.y;

        return (targetPosition - transform.position).magnitude < .2f;
    }

    private void NextPosition()
    {
        _currentPatrolPositionIndex = (_currentPatrolPositionIndex + 1) % patrolPositions.Length;
        _positionTarget = patrolPositions[_currentPatrolPositionIndex];
    }

    private void NearestPosition()
    {
        float distance = float.MaxValue;

        for (int i = 1; i < patrolPositions.Length; i++)
        {
            Transform position = patrolPositions[i];
            float positionDistance = (position.position - transform.position).magnitude;
            if (positionDistance >= distance) continue;

            _positionTarget = position;
            _currentPatrolPositionIndex = i;
            distance = positionDistance;
        }
    }

    // TRANSITIONS

    private bool IsEnemyVisible()
    {
        Transform potentialTarget = ScanField();
        bool isEnemyVisible = potentialTarget && VisibleEnemy(potentialTarget);
        if (isEnemyVisible)
        {
            _alarmTargetPreviousPosition = _alarmTargetPosition;
            _alarmTargetPosition = potentialTarget.position;
        }

        return isEnemyVisible;
    }

    private bool EnemyLost()
    {
        Vector3 guessPosition = _alarmTargetPosition;
        guessPosition.y = transform.position.y;

        return (guessPosition - transform.position).magnitude < .2f && NotIsEnemyVisible();
    }

    private bool NotIsEnemyVisible()
    {
        return !IsEnemyVisible();
    }

    private Transform ScanField()
    {
        Vector3 distance = Vector3.positiveInfinity;
        GameObject potentialTarget = null;
        GameObject[] targetObjects = GameObject.FindGameObjectsWithTag(targetTag);

        foreach (GameObject go in targetObjects)
        {
            Vector3 tempDistance = go.transform.position - transform.position;
            if (tempDistance.magnitude < distance.magnitude)
            {
                distance = tempDistance;
                potentialTarget = go;
            }
        }

        if (potentialTarget &&
            Vector3.Angle(distance, transform.forward) < _fieldOfView.GetViewAngle() / 2 &&
            distance.magnitude <= _fieldOfView.GetViewDistance())
        {
            return potentialTarget.transform;
        }

        return null;
    }

    private bool VisibleEnemy(Transform potentialTarget)
    {
        Vector3 toTarget = potentialTarget.position - transform.position;

        return !Physics.Raycast(transform.position,
            toTarget.normalized,
            out RaycastHit _,
            toTarget.magnitude,
            obstructionMask);
    }
}