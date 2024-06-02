using System;
using Mirror;
using Network;
using Round;
using UnityEngine;

public class StabManager : NetworkBehaviour
{
    [SerializeField] private float delay;
    
    private const float KILL_DISTANCE = 1.5f;
    
    public event EventHandler<EventArgs> OnStab;
    
    private float lastStabTime;
    
    [SyncVar] private bool canStealTraps = false;

    public override void OnStartClient()
    {
        var player = GetComponent<Player>();
        if (!player.isLocalPlayer)
            return;

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
        TargetOnStab(sender.Opponent().connectionToClient);
        
        if (Physics.Raycast(transform.position, transform.forward, out var hit, KILL_DISTANCE))
        {
            if (hit.collider.TryGetComponent(out Player opponent))
                RoundController.Instance.TryKillPlayer(opponent, sender.Player(), canStealTraps);
        }
    }

    [TargetRpc]
    private void TargetOnStab(NetworkConnectionToClient opponent)
    {
        OnStab?.Invoke(gameObject, EventArgs.Empty);
    }

    private bool CanStab()
    {
        return Time.time - lastStabTime >= delay;
    }
    
    [Command(requiresAuthority = false)]
    public void CmdSetCanStealTraps(bool canSteal)
    {
        canStealTraps = canSteal;
    }
}
