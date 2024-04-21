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

    [Header("Patrol Positions")] public Transform[] patrolPositions = null;

    [SerializeField] public LayerMask obstructionMask;

    private FSM _fsm;
    private Color _baseColor;
    private NavMeshAgent _agent;
    private FieldOfVIew _fieldOfView;
    private Transform _positionTarget;
    private Transform _alarmTarget;
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
        alarmState.ExitActions.Add(AlarmColorReset);

        FSMTransition positionReachedTransition =
            new FSMTransition(PositionReached, new FSMAction[] { NextPosition });
        FSMTransition enemyVisibleTransition =
            new FSMTransition(IsEnemyVisible);
        FSMTransition notEnemyVisibleTransition =
            new FSMTransition(NotIsEnemyVisible, new FSMAction[] { NearerPosition });

        patrolState.AddTransition(enemyVisibleTransition, alarmState);
        patrolState.AddTransition(positionReachedTransition, patrolState);
        alarmState.AddTransition(notEnemyVisibleTransition, patrolState);

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

    private void AlarmFollowTarget()
    {
        _agent.SetDestination(_alarmTarget.position);
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

    private void NearerPosition()
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
        return ScanField() && VisibleEnemy();
    }

    private bool NotIsEnemyVisible()
    {
        return !IsEnemyVisible();
    }

    private bool ScanField()
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
            _alarmTarget = potentialTarget.transform;
            return true;
        }

        return false;
    }

    private bool VisibleEnemy()
    {
        Vector3 toTarget = _alarmTarget.position - transform.position;

        return !Physics.Raycast(transform.position,
            toTarget.normalized,
            out RaycastHit _,
            toTarget.magnitude,
            obstructionMask);
    }
}