using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Audio;
using Cinemachine.Utility;
using Mirror;
using Round;
using UnityEditor;
using UnityEngine.AI;

public class FSMSentinel : NetworkBehaviour
{
    public float reactionTime = 1f;

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

    private Transform _alarmTarget;
    private Vector3 _alarmTargetLastPosition = Vector3.zero;
    private int _currentPatrolPositionIndex;
    private GameObject[] _targetObjects;
    private Vector3 _previousPosition;
    private Animator _animator;
    private static readonly int SpeedParam = Animator.StringToHash("speed");

    private void Awake()
    {
        _baseColor = alarmLight.color;
        _agent = GetComponent<NavMeshAgent>();
        _animator = GetComponentInChildren<Animator>();
        _fieldOfView = GetComponentInChildren<FieldOfVIew>();

        if (patrolPositions.Length > 0)
        {
            _positionTarget = patrolPositions[_currentPatrolPositionIndex];
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
        //RoundController.Instance.OnRoundStarted += OnRoundStarted;
    }

    public override void OnStartServer()
    {
        base.OnStartServer();
        
        Debug.Log("Sentinel OnStartServer");
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

        SetUpFSM();
    }

    [Server]
    private void SetUpFSM()
    {
        //if (!authority) return;

        FSMState patrolState = new FSMState();
        patrolState.EnterActions.Add(PositionDestination);

        FSMState alarmState = new FSMState();
        alarmState.EnterActions.Add(AlarmColor);
        alarmState.EnterActions.Add(SignalOnTarget);
        alarmState.StayActions.Add(AlarmFollowTarget);
        alarmState.ExitActions.Add(PredictNewPosition);
        alarmState.ExitActions.Add(StopSignalOnTarget);

        FSMState searchState = new FSMState();
        searchState.EnterActions.Add(SearchColor);
        searchState.ExitActions.Add(AlarmColorReset);

        FSMTransition positionReachedTransition =
            new FSMTransition(PositionReached, new FSMAction[] { NextPosition });
        FSMTransition enemyVisibleTransition =
            new FSMTransition(IsEnemyVisible, new FSMAction[] { PlayAlarmSound });
        FSMTransition notEnemyVisibleTransition =
            new FSMTransition(EnemyNoMoreVisible, new FSMAction[] { PlaySearchingSound, NearestPosition });
        FSMTransition enemyLostTransition =
            new FSMTransition(EnemyLost, new FSMAction[] { PlayEnemyLostSound, NearestPosition });

        patrolState.AddTransition(enemyVisibleTransition, alarmState);
        patrolState.AddTransition(positionReachedTransition, patrolState);
        alarmState.AddTransition(notEnemyVisibleTransition, searchState);
        searchState.AddTransition(enemyLostTransition, patrolState);
        searchState.AddTransition(enemyVisibleTransition, alarmState);

        _fsm = new FSM(patrolState);
        StartPatrolling();

        //RoundController.Instance.OnRoundStarted -= OnRoundStarted;
    }

    private void Update()
    {
        _animator.SetFloat(SpeedParam, _agent.velocity.magnitude);
    }

    [Server]
    private void StartPatrolling()
    {
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

    [ClientRpc]
    private void AlarmColorReset()
    {
        alarmLight.color = _baseColor;
    }

    [ClientRpc]
    private void AlarmColor()
    {
        alarmLight.color = alarmColor;
    }

    [ClientRpc]
    private void SearchColor()
    {
        alarmLight.color = searchColor;
    }

    private void AlarmFollowTarget()
    {
        _agent.SetDestination(_alarmTarget.position);
    }

    private void PredictNewPosition()
    {
        Vector3 newPosition =
            _alarmTarget.position + (_alarmTarget.position - _alarmTargetLastPosition) * reactionTime * 2;
        _agent.SetDestination(newPosition);
    }

    private void PositionDestination()
    {
        _agent.SetDestination(_positionTarget.position);
    }

    private bool PositionReached()
    {
        bool reached = _agent.hasPath &&
                       ((_previousPosition - transform.position).magnitude < 0.1f ||
                        _agent.remainingDistance <= _agent.stoppingDistance);
        _previousPosition = reached ? Vector3.zero : transform.position;
        return reached;
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

    // TRANSITIONS

    private bool IsEnemyVisible()
    {
        Transform potentialTarget = ScanField();
        bool isEnemyVisible = potentialTarget && VisibleEnemy(potentialTarget);

        if (!potentialTarget) return isEnemyVisible;

        _alarmTargetLastPosition = potentialTarget.position;
        _alarmTarget = potentialTarget;

        return isEnemyVisible;
    }

    private bool EnemyLost()
    {
        return PositionReached() &&
               !IsEnemyVisible();
    }

    private bool EnemyNoMoreVisible()
    {
        Transform potentialTarget = ScanField();
        var isEnemyVisible = VisibleEnemy(!potentialTarget ? _alarmTarget : potentialTarget);

        if (potentialTarget) _alarmTarget = potentialTarget;

        return !isEnemyVisible;
    }

    private Transform ScanField()
    {
        Vector3 distance = Vector3.positiveInfinity;
        Transform potentialTarget = null;

        foreach (GameObject p in _targetObjects)
        {
            Vector3 tempDistance = p.transform.position - transform.position;
            if (tempDistance.magnitude < distance.magnitude)
            {
                distance = tempDistance;
                potentialTarget = p.transform;
            }
        }

        if (potentialTarget &&
            Vector3.Angle(distance, transform.forward) < _fieldOfView.GetViewAngle() / 2 &&
            distance.magnitude <= _fieldOfView.GetViewDistance())
        {
            return potentialTarget;
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

    [ClientRpc]
    private void PlayAlarmSound()
    {
        SoundManager.Instance?.OnSentinelAlarm(transform.position);
    }

    [ClientRpc]
    private void PlayEnemyLostSound()
    {
        SoundManager.Instance?.OnSentinelEnemyLost(transform.position);
    }

    [ClientRpc]
    private void PlaySearchingSound()
    {
        SoundManager.Instance?.OnSentinelSearching(transform.position);
    }
}