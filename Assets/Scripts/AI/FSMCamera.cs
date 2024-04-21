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
    public float patrolRotationSpeed = 10f;
    public float alarmRotationSpeed = 20f;
    public string targetTag = "Player";

    [Header("Alarm")] public Light alarmLight;
    public Color alarmColor = Color.red;

    [FormerlySerializedAs("patrolPositions")]
    public float[] patrolRotations = null;

    [SerializeField] public LayerMask obstructionMask;

    private FSM _fsm;
    private Color _baseColor;
    private FieldOfVIew _fieldOfView;
    private float _rotationSpeed;
    private float _rotationTarget;
    private Transform _alarmTarget;
    private int _currentPatrolRotationIndex;

    void Start()
    {
        _baseColor = alarmLight.color;
        _fieldOfView = GetComponentInChildren<FieldOfVIew>();

        if (patrolRotations.Length > 0)
        {
            _rotationTarget = patrolRotations[_currentPatrolRotationIndex];
        }

        FSMState patrolState = new FSMState();
        patrolState.EnterActions.Add(AlarmReset);

        FSMState alarmState = new FSMState();
        alarmState.EnterActions.Add(AlarmStart);

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
        float target;

        if (_alarmTarget)
        {
            Vector3 dir = (_alarmTarget.position - transform.position).normalized;
            dir.y = 0;
            Vector3 forward = transform.forward;
            forward.y = 0;

            target = transform.eulerAngles.y + Vector3.SignedAngle(forward, dir, Vector3.up);
        }
        else
        {
            target = _rotationTarget;
        }

        float step = _rotationSpeed * Time.fixedDeltaTime;
        transform.rotation =
            Quaternion.RotateTowards(transform.rotation, Quaternion.Euler(0, target, 0), step);
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

    private void AlarmReset()
    {
        alarmLight.color = _baseColor;
        _rotationSpeed = patrolRotationSpeed;
        _alarmTarget = null;
    }

    private void AlarmStart()
    {
        alarmLight.color = alarmColor;
        _rotationSpeed = alarmRotationSpeed;
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
        Transform potentialTarget = ScanField();
        bool isEnemyVisible = potentialTarget && VisibleEnemy(potentialTarget);
        if (isEnemyVisible)
        {
            _alarmTarget = potentialTarget;
        }

        return isEnemyVisible;
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