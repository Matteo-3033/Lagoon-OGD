using System;
using Mirror;
using Round;
using UnityEngine;

public class PlayerAnimationManager : MonoBehaviour
{
    public enum Animation
    {
        Idle, Stab, Death    
    }
    
    private NetworkAnimator networkAnimator;

    private static readonly int SpeedParam = Animator.StringToHash("speed");
    private static readonly int StabTrigger = Animator.StringToHash("Stab");
    private static readonly int DeathTrigger = Animator.StringToHash("Death");

    private Player player;
    private PlayerPositionController positionController;
    
    private void Start()
    {
        player = GetComponentInParent<Player>();
        networkAnimator = player.GetComponent<NetworkAnimator>();
        
        if (!player.isLocalPlayer)
        {
            Destroy(this);
            return;
        }
        
        positionController = player.PositionController;
        
        player.StabManager.OnStab += OnStab;
        KillController.OnPlayerKilled += OnPlayerKilled;
    }

    private void OnPlayerKilled(Player killedPlayer)
    {
        if (killedPlayer != player)
            return;
        
        networkAnimator.SetTrigger(DeathTrigger);
    }

    private void OnStab(object sender, EventArgs e)
    {
        Debug.Log("Stab");
        networkAnimator.SetTrigger(StabTrigger);
    }

    private void FixedUpdate()
    {
        if (networkAnimator != null && networkAnimator.animator != null)
            networkAnimator.animator.SetFloat(SpeedParam, positionController.SpeedValue);
    }
}
