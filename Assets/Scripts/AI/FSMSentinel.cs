using System;
using UnityEngine;
using Mirror;
using Round;
using Unity.VisualScripting;
using UnityEngine.AI;

public class FSMSentinel : EnemyFSM
{
    public ParticleSystem deathEffect = null;
    [Space] public Transform[] patrolPositions;

    private Color _baseColor;
    private NavMeshAgent _agent;

    private bool _killed = false;

    private Transform _positionTarget;

    private Vector3 _alarmTargetLastPosition = Vector3.zero;
    private int _currentPatrolPositionIndex;
    private Vector3 _previousPosition;
    private Animator _animator;
    [SyncVar] private float _agentSpeed;
    
    private static readonly int SpeedParam = Animator.StringToHash("speed");
    private static readonly int DeathTrigger = Animator.StringToHash("death");
    
    private const float KILL_DISTANCE = 1.5f;

    private void Awake()
    {
        _baseColor = alarmLight.color;
        _agent = GetComponent<NavMeshAgent>();
        _animator = GetComponentInChildren<Animator>();
        FieldOfView = GetComponentInChildren<FieldOfView>();
        SoundManager = GetComponent<SentinelSoundManager>();
        
        KillController.OnSentinelKilled += OnSentinelKilled;
        
        if (patrolPositions.Length > 0)
        {
            _positionTarget = patrolPositions[_currentPatrolPositionIndex];
        }
    }

    public override void OnStartServer()
    {
        base.OnStartServer();

        Debug.Log("Sentinel OnStartServer");
        
        SentinelDeath.OnDeathAnimationEnded += SentinelOnDeathAnimationEnded;

        if (RoundController.HasLoaded())
        {
            SetupFSM();
        }
        else
        {
            RoundController.OnRoundLoaded += SetupFSM;
        }
    }

    [Server]
    private void SetupFSM()
    {
        RoundController.OnRoundLoaded -= SetupFSM;

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
        FSMTransition enemyKilledTransition =
            new FSMTransition(EnemyKilled, new FSMAction[] { AlarmColorReset, NearestPosition });
        FSMTransition enemyLostTransition =
            new FSMTransition(EnemyLost, new FSMAction[] { PlayEnemyLostSound, NearestPosition });

        patrolState.AddTransition(enemyVisibleTransition, alarmState);
        patrolState.AddTransition(positionReachedTransition, patrolState);
        alarmState.AddTransition(notEnemyVisibleTransition, searchState);
        alarmState.AddTransition(enemyKilledTransition, patrolState);
        searchState.AddTransition(enemyLostTransition, patrolState);
        searchState.AddTransition(enemyVisibleTransition, alarmState);

        FSM = new FSM(patrolState);
        PlayFSM();
    }

    private void Update()
    {
        if (isServer)
        {
            _agentSpeed = _agent.velocity.magnitude;
        }
        else
        {
            _animator.SetFloat(SpeedParam, _agentSpeed);
        }
    }

    #region ACTIONS

    private void AlarmColorReset()
    {
        RpcSetAlarmColor(_baseColor);
    }

    private void AlarmColor()
    {
        RpcSetAlarmColor(alarmColor);
    }

    private void SearchColor()
    {
        RpcSetAlarmColor(searchColor);
    }

    private void AlarmFollowTarget()
    {
        _agent.SetDestination(AlarmTarget.position);
    }

    private void PredictNewPosition()
    {
        Vector3 newPosition =
            AlarmTarget.position + (AlarmTarget.position - _alarmTargetLastPosition) * (reactionTime * 2);
        _agent.SetDestination(newPosition);
    }

    private void PositionDestination()
    {
        _agent.SetDestination(_positionTarget.position);
    }

    private bool PositionReached()
    {
        bool reached = !_agent.hasPath || (_agent.hasPath &&
                                           ((_previousPosition - transform.position).magnitude < 0.1f ||
                                            _agent.remainingDistance <= _agent.stoppingDistance));
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

    #endregion

    #region TRANSITIONS

    private bool IsEnemyVisible()
    {
        Transform potentialTarget = ScanField();
        bool isEnemyVisible = potentialTarget && VisibleEnemy(potentialTarget);

        if (!potentialTarget) return isEnemyVisible;

        _alarmTargetLastPosition = potentialTarget.position;
        AlarmTarget = potentialTarget;

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
        var isEnemyVisible = VisibleEnemy(!potentialTarget ? AlarmTarget : potentialTarget);

        if (potentialTarget) AlarmTarget = potentialTarget;

        return !isEnemyVisible;
    }

    private bool EnemyKilled()
    {
        if (_agent.remainingDistance > 2.5f) return false;
        if (!Physics.Raycast(transform.position, transform.forward, out var hit, KILL_DISTANCE)) return false;

        if (!hit.collider.TryGetComponent(out Player enemy)) return false;

        KillController.Instance.TryKillPlayer(enemy, this);
        return true;
    }

    #endregion
    
    private void OnSentinelKilled(FSMSentinel sentinel)
    {
        if (_killed || sentinel != this) return;

        KillController.OnSentinelKilled -= OnSentinelKilled;
        
        _killed = true;
        _animator.SetTrigger(DeathTrigger);
        deathEffect?.Play();
    }

    private void SentinelOnDeathAnimationEnded(object sender, EventArgs e)
    {
        Debug.Log("SentinelOnDeathAnimationEnded");
        if (sender as GameObject != gameObject) return;
        Debug.Log("Destroy");
        
        NetworkServer.Destroy(gameObject);
    }

    [Server]
    public override void StopFSM()
    {
        _agent.isStopped = true;
        base.StopFSM();
    }

    [Server]
    public override void PlayFSM()
    {
        _agent.isStopped = false;

        base.PlayFSM();
    }
}