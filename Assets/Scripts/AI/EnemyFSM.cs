using System.Collections;
using System.Collections.Generic;
using Audio;
using Mirror;
using Round;
using Unity.VisualScripting;
using UnityEngine;

public abstract class EnemyFSM : NetworkBehaviour
{
    public float reactionTime = 1f;
    public LayerMask obstructionMask;

    [Header("Alarm")] public Light alarmLight;
    public Color alarmColor = Color.red;
    public Color searchColor = Color.yellow;

    protected FSM FSM;
    protected FieldOfView FieldOfView;

    protected Transform AlarmTarget;
    protected SentinelSoundManager SoundManager;
    private bool _canUpdate;
    private Coroutine _fsmCoroutine;

    [Server]
    protected IEnumerator Patrol()
    {
        while (_canUpdate)
        {
            FSM.Update();
            yield return new WaitForSeconds(reactionTime);
        }
    }

    protected Transform ScanField()
    {
        Vector3 distance = Vector3.positiveInfinity;
        GameObject potentialTarget = null;

        var players = GameObject.FindGameObjectsWithTag("Player");
        if (players == null) return null;

        foreach (GameObject p in players)
        {
            Vector3 tempDistance = p.transform.position - transform.position;
            if (!p.GetComponent<Player>().IsDead && tempDistance.magnitude < distance.magnitude)
            {
                distance = tempDistance;
                potentialTarget = p.gameObject;
            }
        }

        if (potentialTarget &&
            Vector3.Angle(distance, transform.forward) < FieldOfView.GetViewAngle() / 2 &&
            distance.magnitude <= FieldOfView.GetViewDistance())
        {
            return potentialTarget.transform;
        }

        return null;
    }

    protected bool VisibleEnemy(Transform potentialTarget)
    {
        Vector3 toTarget = potentialTarget.position - transform.position;

        return !Physics.Raycast(transform.position,
            toTarget.normalized,
            toTarget.magnitude,
            obstructionMask);
    }

    protected void SignalOnTarget()
    {
        RippleController rippleController = AlarmTarget?.GetComponentInChildren<RippleController>();
        if (!rippleController) return;

        rippleController.ShowAlarmRipple();
    }

    protected void StopSignalOnTarget()
    {
        RippleController rippleController = AlarmTarget?.GetComponentInChildren<RippleController>();
        if (!rippleController) return;

        rippleController.StopAlarmRipple();
    }

    public virtual void StopFSM()
    {
        _canUpdate = false;
        if (_fsmCoroutine != null)
        {
            StopCoroutine(_fsmCoroutine);
        }
    }

    public virtual void PlayFSM()
    {
        if (_fsmCoroutine != null)
        {
            StopCoroutine(_fsmCoroutine);
        }

        _canUpdate = true;
        _fsmCoroutine = StartCoroutine(Patrol());
    }

    [ClientRpc]
    protected void PlayAlarmSound()
    {
        SoundManager?.OnSentinelAlarm();
    }

    [ClientRpc]
    protected void PlayEnemyLostSound()
    {
        SoundManager?.OnSentinelEnemyLost();
    }

    [ClientRpc]
    protected void PlaySearchingSound()
    {
        SoundManager?.OnSentinelSearching();
    }

    private void OnDestroy()
    {
        if (!isServer) return;

        if (AlarmTarget)
        {
            StopSignalOnTarget();
        }

        StopFSM();
    }
}