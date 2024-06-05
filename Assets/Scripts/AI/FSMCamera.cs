using UnityEngine;
using Mirror;

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
        FieldOfView = GetComponentInChildren<FieldOfView>();
        SoundManager = GetComponent<SentinelSoundManager>();
    }

    public override void OnStartServer()
    {
        base.OnStartServer();

        if (patrolRotations.Length > 0)
        {
            _rotationTarget = patrolRotations[_currentPatrolRotationIndex];
        }

        Debug.Log("Camera OnStartServer");

        SetupFSM();
    }

    [Server]
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
        PlayFSM();
    }

    private void FixedUpdate()
    {
        if (!isServer) return;

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
        RpcSetAlarmColor(_baseColor);
        _rotationSpeed = patrolRotationSpeed;
        AlarmTarget = null;
    }

    private void AlarmStart()
    {
        RpcSetAlarmColor(alarmColor);
        _rotationSpeed = alarmRotationSpeed;
    }

    private void AlarmSearch()
    {
        RpcSetAlarmColor(searchColor);
        _rotationSpeed = searchRotationSpeed;
        _currentTimeInSearch = 0;
        AlarmTarget = null;
    }

    [ClientRpc]
    private void RpcSetAlarmColor(Color color)
    {
        alarmLight.color = color;
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