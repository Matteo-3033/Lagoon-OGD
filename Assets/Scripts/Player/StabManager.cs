using System;
using Mirror;
using Network;
using Round;
using UnityEngine;

public class StabManager : NetworkBehaviour
{
    [SerializeField] private float delaySeconds;

    private const float KILL_DISTANCE = 1.5f;

    public event EventHandler<EventArgs> OnStab;

    private float lastStabTime;

    [field: SyncVar] public bool CanStealTraps { get; private set; }

    public override void OnStartClient()
    {
        var player = GetComponent<Player>();

#if !UNITY_EDITOR
        if (!player.isLocalPlayer)
            return;
#endif

        player.InputHandler.OnStab += OnStabInteraction;
    }

    private void OnStabInteraction(object sender, EventArgs args)
    {
        if (!CanStab()) return;

        lastStabTime = Time.time;
        OnStab?.Invoke(gameObject, EventArgs.Empty);
        CmdStab();
    }

    [Command(requiresAuthority = false)]
    private void CmdStab(NetworkConnectionToClient sender = null)
    {
        if (!CanStab()) return;
        lastStabTime = Time.time;
        
        Debug.Log("Stab from " + sender.Player());
        TargetOnStab(sender.Opponent().connectionToClient);
        if (!Physics.Raycast(transform.position, transform.forward, out var hit, KILL_DISTANCE)) return;

        if (hit.collider.TryGetComponent(out Player opponent))
        {
            Debug.Log("Trying to kill opponent");
            KillController.Instance.TryKillPlayer(opponent, sender.Player(), CanStealTraps);
        }
        else if (hit.collider.TryGetComponent(out FSMSentinel sentinel))
        {
            Debug.Log("Trying to kill sentinel");
            float halfFieldOfViewAngle = sentinel.GetComponentInChildren<FieldOfView>().GetAngle() / 2;
            float dotProduct = Vector3.Dot(transform.forward, sentinel.transform.forward);

            float stabAngle = Mathf.Acos(dotProduct) * Mathf.Rad2Deg;
            if ((dotProduct > 0 && stabAngle < halfFieldOfViewAngle) ||
                (dotProduct < 0 && stabAngle < 180 - halfFieldOfViewAngle))
            {
                Debug.Log("Sentinel Killed");
                NetworkServer.Destroy(sentinel.gameObject);
            }
        }
    }

    [TargetRpc]
    private void TargetOnStab(NetworkConnectionToClient opponent)
    {
        OnStab?.Invoke(gameObject, EventArgs.Empty);
    }

    private bool CanStab()
    {
        return Time.time - lastStabTime >= delaySeconds;
    }

    [Command(requiresAuthority = false)]
    public void CmdSetCanStealTraps(bool canSteal)
    {
        CanStealTraps = canSteal;
    }
}