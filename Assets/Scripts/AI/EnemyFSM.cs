using System.Collections;
using System.Collections.Generic;
using Audio;
using Mirror;
using UnityEngine;

public abstract class EnemyFSM : NetworkBehaviour
{
    public float reactionTime = 1f;
    public LayerMask obstructionMask;

    [Header("Alarm")] public Light alarmLight;
    public Color alarmColor = Color.red;
    public Color searchColor = Color.yellow;

    protected FSM FSM;
    protected GameObject[] TargetObjects;
    protected FieldOfVIew FieldOfView;

    protected Transform AlarmTarget;

    protected IEnumerator Patrol()
    {
        while (true)
        {
            FSM.Update();
            yield return new WaitForSeconds(reactionTime);
        }
    }

    protected Transform ScanField()
    {
        Vector3 distance = Vector3.positiveInfinity;
        GameObject potentialTarget = null;

        foreach (GameObject go in TargetObjects)
        {
            Vector3 tempDistance = go.transform.position - transform.position;
            if (tempDistance.magnitude < distance.magnitude)
            {
                distance = tempDistance;
                potentialTarget = go;
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


    [ClientRpc]
    protected void PlayAlarmSound()
    {
        SoundManager.Instance?.OnSentinelAlarm(transform.position);
    }

    [ClientRpc]
    protected void PlayEnemyLostSound()
    {
        SoundManager.Instance?.OnSentinelEnemyLost(transform.position);
    }

    [ClientRpc]
    protected void PlaySearchingSound()
    {
        SoundManager.Instance?.OnSentinelSearching(transform.position);
    }
}