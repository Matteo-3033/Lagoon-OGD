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
        OnStab?.Invoke(this, EventArgs.Empty);
        CmdStab();
    }

    [Command(requiresAuthority = false)]
    private void CmdStab(NetworkConnectionToClient sender = null)
    {
        TargetOnStab(sender.Opponent().connectionToClient);
        
        if (Physics.Raycast(transform.position, transform.forward, out var hit, KILL_DISTANCE))
        {
            if (hit.collider.TryGetComponent(out Player opponent))
                RoundController.Instance.KillPlayer(opponent, sender.Player());
        }
    }

    [TargetRpc]
    private void TargetOnStab(NetworkConnectionToClient opponent)
    {
        OnStab?.Invoke(this, EventArgs.Empty);
    }

    private bool CanStab()
    {
        return Time.time - lastStabTime >= delay;
    }
}
