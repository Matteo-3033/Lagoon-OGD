using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.AI;
using UnityEngine.Serialization;

public class FSMCamera : EnemyFSM
{
    [Space] public float searchStateTime = 3;

    [Space] public float patrolRotationSpeed = 10f;
    public float alarmRotationSpeed = 120f;
    public float searchRotationSpeed = 90f;

    [Space] public float[] patrolRotations;

    private Color _baseColor;
    private int _currentPatrolRotationIndex;
    private float _rotationSpeed;
    private float _rotationTarget;
    private float _currentTimeInSearch = 0;

    void Start()
    {
        _baseColor = alarmLight.color;
        FieldOfView = GetComponentInChildren<FieldOfVIew>();

        if (patrolRotations.Length > 0)
        {
            _rotationTarget = patrolRotations[_currentPatrolRotationIndex];
        }

        GameObject localPlayer = Player.LocalPlayer?.gameObject;
        if (!localPlayer)
        {
            TargetObjects = new[] { GameObject.FindGameObjectWithTag("Player") };
        }
        else
        {
            GameObject opponent = Player.Opponent?.gameObject;
            TargetObjects = new[] { localPlayer, opponent };
        }

        SetupFSM();

        StartCoroutine(Patrol());
    }

    private void SetupFSM()
    {
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
            new FSMTransition(IsEnemyVisible, new FSMAction[] { PlayAlarmSound });
        FSMTransition notEnemyVisibleTransition =
            new FSMTransition(NotIsEnemyVisible, new FSMAction[] { PlaySearchingSound });
        FSMTransition enemyLostTransition =
            new FSMTransition(EnemyLost, new FSMAction[] { PlayEnemyLostSound, LastAngle });

        patrolState.AddTransition(enemyVisibleTransition, alarmState);
        patrolState.AddTransition(rotationReachedTransition, patrolState);
        alarmState.AddTransition(notEnemyVisibleTransition, searchState);
        searchState.AddTransition(enemyVisibleTransition, alarmState);
        searchState.AddTransition(enemyLostTransition, patrolState);
        searchState.AddTransition(rotationReachedTransition, searchState);

        FSM = new FSM(patrolState);
    }

    private void FixedUpdate()
    {
        float target;

        if (AlarmTarget)
        {
            Vector3 dir = (AlarmTarget.position - transform.position).normalized;
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

    #region ACTIONS

    private void AlarmReset()
    {
        alarmLight.color = _baseColor;
        _rotationSpeed = patrolRotationSpeed;
        AlarmTarget = null;
    }

    private void AlarmStart()
    {
        alarmLight.color = alarmColor;
        _rotationSpeed = alarmRotationSpeed;
    }

    private void AlarmSearch()
    {
        alarmLight.color = searchColor;
        _rotationSpeed = searchRotationSpeed;
        _currentTimeInSearch = 0;
        AlarmTarget = null;
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

    #endregion

    #region TRANSITIONS

    private bool IsEnemyVisible()
    {
        Transform potentialTarget = ScanField();
        bool isEnemyVisible = potentialTarget && VisibleEnemy(potentialTarget);
        if (isEnemyVisible)
        {
            AlarmTarget = potentialTarget;
        }

        return isEnemyVisible;
    }

    private bool NotIsEnemyVisible()
    {
        return !IsEnemyVisible();
    }

    private bool EnemyLost()
    {
        _currentTimeInSearch += reactionTime;
        return _currentTimeInSearch >= searchStateTime && NotIsEnemyVisible();
    }

    #endregion
}