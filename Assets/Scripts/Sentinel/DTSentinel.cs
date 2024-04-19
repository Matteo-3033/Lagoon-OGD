using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.AI;

public class DTSentinel : MonoBehaviour
{
    public float reactionTime = 1f;
    public string targetTag = "Player";

    [Header("Alarm")] public Light alarmLight;
    public Color alarmColor = Color.red;

    [Header("Patrol Positions")] public Transform[] patrolPositions = null;

    [SerializeField] public LayerMask obstructionMask;

    private DecisionTree _dt;
    private Color _baseColor;
    private NavMeshAgent _agent;
    private FieldOfVIew _fieldOfView;
    private Transform _alarmTarget;
    private Transform _target;
    private int _currentPatrolPositionIndex;

    private Color _rayColor;

    void Start()
    {
        _agent = GetComponent<NavMeshAgent>();
        _fieldOfView = GetComponentInChildren<FieldOfVIew>();

        if (patrolPositions.Length > 0)
        {
            _target = patrolPositions[_currentPatrolPositionIndex];
        }

        DTAction alarmAction = new DTAction(Alarm);
        DTAction notAlarmAction = new DTAction(NotAlarm);

        DTDecision scanFieldDecision = new DTDecision(ScanField);
        DTDecision visibleEnemyDecision = new DTDecision(VisibleEnemy);

        scanFieldDecision.AddLink(true, visibleEnemyDecision);
        scanFieldDecision.AddLink(false, notAlarmAction);
        visibleEnemyDecision.AddLink(true, alarmAction);
        visibleEnemyDecision.AddLink(false, notAlarmAction);

        _dt = new DecisionTree(scanFieldDecision);

        _baseColor = alarmLight.color;

        StartCoroutine(Patrol());
    }

    private IEnumerator Patrol()
    {
        while (true)
        {
            _dt.walk();
            yield return new WaitForSeconds(reactionTime);
        }
    }

    // ACTIONS

    public object Alarm(object o)
    {
        alarmLight.color = alarmColor;
        _agent.SetDestination(_alarmTarget.position);
        return null;
    }

    public object NotAlarm(object o)
    {
        alarmLight.color = _baseColor;
        if (patrolPositions == null || patrolPositions.Length == 0)
        {
            _target = null;
            _agent.isStopped = true;
            return null;
        }

        Vector3 targetPosition = _target.position;
        targetPosition.y = transform.position.y;
        if ((targetPosition - transform.position).magnitude < .2f)
        {
            _currentPatrolPositionIndex = (_currentPatrolPositionIndex + 1) % patrolPositions.Length;
            _target = patrolPositions[_currentPatrolPositionIndex];
        }

        _agent.SetDestination(_target.position);

        return null;
    }

    // DECISIONS

    public object ScanField(object o)
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

    public object VisibleEnemy(object o)
    {
        Vector3 toTarget = _alarmTarget.position - transform.position;

        return !Physics.Raycast(transform.position,
            toTarget.normalized,
            out RaycastHit _,
            toTarget.magnitude,
            obstructionMask);
    }
}