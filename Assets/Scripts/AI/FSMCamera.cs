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

    [Header("Alarm")] public Light alarmLight;
    public Color alarmColor = Color.red;
    public Color searchColor = Color.yellow;

    public float[] patrolRotations = null;
    public float searchStateTime = 2;
    public LayerMask obstructionMask;

    private FSM _fsm;
    private Color _baseColor;
    private int _currentPatrolRotationIndex;
    private GameObject[] _targetObjects;
    private FieldOfVIew _fieldOfView;
    private float _rotationSpeed;
    private float _rotationTarget;
    private Transform _alarmTarget;
    private float _currentTimeInSearch = 0;

    void Start()
    {
        _baseColor = alarmLight.color;
        _fieldOfView = GetComponentInChildren<FieldOfVIew>();

        if (patrolRotations.Length > 0)
        {
            _rotationTarget = patrolRotations[_currentPatrolRotationIndex];
        }

        GameObject localPlayer = Player.LocalPlayer?.gameObject;
        if (!localPlayer)
        {
            _targetObjects = new[] { GameObject.FindGameObjectWithTag("Player") };
        }
        else
        {
            GameObject opponent = Player.Opponent?.gameObject;
            _targetObjects = new[] { localPlayer, opponent };
        }

        FSMState patrolState = new FSMState();
        patrolState.EnterActions.Add(AlarmReset);

        FSMState alarmState = new FSMState();
        alarmState.EnterActions.Add(AlarmStart);
        alarmState.EnterActions.Add(SignalOnTarget);
        alarmState.ExitActions.Add(AlarmSearch);
        alarmState.ExitActions.Add(StopSignalOnTarget);

        FSMState searchState = new FSMState();

        FSMTransition rotationReachedTransition =
            new FSMTransition(RotationReached, new FSMAction[] { NextRotation });
        FSMTransition enemyVisibleTransition =
            new FSMTransition(IsEnemyVisible);
        FSMTransition notEnemyVisibleTransition =
            new FSMTransition(NotIsEnemyVisible);
        FSMTransition enemyLostTransition =
            new FSMTransition(EnemyLost, new FSMAction[] { LastAngle });

        patrolState.AddTransition(enemyVisibleTransition, alarmState);
        patrolState.AddTransition(rotationReachedTransition, patrolState);
        alarmState.AddTransition(notEnemyVisibleTransition, searchState);
        searchState.AddTransition(enemyVisibleTransition, alarmState);
        searchState.AddTransition(enemyLostTransition, patrolState);
        searchState.AddTransition(rotationReachedTransition, searchState);

        _fsm = new FSM(patrolState);

        StartCoroutine(Patrol());
    }

    private void FixedUpdate()
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

    private void AlarmSearch()
    {
        alarmLight.color = searchColor;
        _rotationSpeed = alarmRotationSpeed;
        _currentTimeInSearch = 0;
        _alarmTarget = null;
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

    private void SignalOnTarget()
    {
        RippleController rippleController = _alarmTarget.GetComponentInChildren<RippleController>();
        if (!rippleController) return;

        rippleController.ShowAlarmRipple();
    }

    private void StopSignalOnTarget()
    {
        RippleController rippleController = _alarmTarget.GetComponentInChildren<RippleController>();
        if (!rippleController) return;

        rippleController.StopAlarmRipple();
    }

    private bool NotIsEnemyVisible()
    {
        return !IsEnemyVisible();
    }

    private Transform ScanField()
    {
        Vector3 distance = Vector3.positiveInfinity;
        GameObject potentialTarget = null;

        foreach (GameObject go in _targetObjects)
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
            toTarget.magnitude,
            obstructionMask);
    }

    private bool EnemyLost()
    {
        _currentTimeInSearch += reactionTime;
        return _currentTimeInSearch >= searchStateTime && NotIsEnemyVisible();
    }
}