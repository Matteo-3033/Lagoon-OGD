using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.AI;
using UnityEngine.Serialization;

public class FSMCamera : MonoBehaviour
{
    public float reactionTime = 1f;
    public float rotationSpeed = 150f;
    public string targetTag = "Player";

    [Header("Alarm")] public Light alarmLight;
    public Color alarmColor = Color.red;

    [FormerlySerializedAs("patrolPositions")]
    public float[] patrolRotations = null;

    [SerializeField] public LayerMask obstructionMask;

    private FSM _fsm;
    private Color _baseColor;
    private NavMeshAgent _agent;
    private FieldOfVIew _fieldOfView;
    private float _rotationTarget;
    private Transform _alarmTarget;
    private int _currentPatrolRotationIndex;

    void Start()
    {
        _baseColor = alarmLight.color;
        _agent = GetComponent<NavMeshAgent>();
        _fieldOfView = GetComponentInChildren<FieldOfVIew>();

        if (patrolRotations.Length > 0)
        {
            _rotationTarget = patrolRotations[_currentPatrolRotationIndex];
        }

        FSMState patrolState = new FSMState();

        FSMState alarmState = new FSMState();
        alarmState.EnterActions.Add(AlarmColor);
        alarmState.StayActions.Add(AlarmFollowTarget);
        alarmState.ExitActions.Add(AlarmColorReset);

        FSMTransition rotationReachedTransition =
            new FSMTransition(RotationReached, new FSMAction[] { NextRotation });
        FSMTransition enemyVisibleTransition =
            new FSMTransition(IsEnemyVisible);
        FSMTransition notEnemyVisibleTransition =
            new FSMTransition(NotIsEnemyVisible, new FSMAction[] { LastAngle });

        patrolState.AddTransition(enemyVisibleTransition, alarmState);
        patrolState.AddTransition(rotationReachedTransition, patrolState);
        alarmState.AddTransition(notEnemyVisibleTransition, patrolState);

        _fsm = new FSM(patrolState);

        StartCoroutine(Patrol());
    }

    private void Update()
    {
        float step = rotationSpeed * Time.fixedDeltaTime;
        transform.rotation =
            Quaternion.RotateTowards(transform.rotation, Quaternion.Euler(0, _rotationTarget, 0), step);
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
        Vector3 dir = (_alarmTarget.position - transform.position).normalized;
        dir.y = 0;

        _rotationTarget = Vector3.Angle(dir, Vector3.forward);
    }

    private bool RotationReached()
    {
        return Mathf.Abs(transform.eulerAngles.y - _rotationTarget) < .1f;
    }

    private void NextRotation()
    {
        _currentPatrolRotationIndex = (_currentPatrolRotationIndex + 1) % patrolRotations.Length;
        _rotationTarget = patrolRotations[_currentPatrolRotationIndex];
    }

    private void LastAngle()
    {
        _rotationTarget = patrolRotations[_currentPatrolRotationIndex];
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