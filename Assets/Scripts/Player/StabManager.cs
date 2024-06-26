using System;
using Mirror;
using Network;
using Round;
using UnityEngine;
using UnityEngine.Serialization;

public class StabManager : NetworkBehaviour
{
    [SerializeField] private float delaySeconds;

    [SerializeField] private float killDistance = 1.7f;
    [SerializeField] private float sphereRadius = 0.3f;

    public event EventHandler<EventArgs> OnStab;

    private float _lastStabTime;

    [field: SyncVar] public bool CanStealTraps { get; private set; }

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

        _lastStabTime = Time.time;
        OnStab?.Invoke(gameObject, EventArgs.Empty);
        CmdStab();
    }

    [Command(requiresAuthority = false)]
    private void CmdStab(NetworkConnectionToClient sender = null)
    {
        if (!CanStab()) return;
        _lastStabTime = Time.time;

        Player player = sender.Player();

        Debug.Log("Stab from " + player);
        TargetOnStab(sender.Opponent().connectionToClient);

        float distanceWithCompensation = killDistance + GetCompensation(player);

        Ray ray = new Ray(transform.position, transform.forward);
        if (!Physics.SphereCast(ray, sphereRadius, out var hit, distanceWithCompensation)) return;


        if (hit.collider.TryGetComponent(out Player opponent))
        {
            Debug.Log("Trying to kill opponent");
            KillController.Instance.TryKillPlayer(opponent, player, CanStealTraps);
        }
        else if (hit.collider.TryGetComponent(out FSMSentinel sentinel))
        {
            Debug.Log("Trying to kill sentinel");
            KillController.Instance.TryKillSentinel(sentinel, player);
        }
    }

    private float GetCompensation(Player player)
    {
        Vector3 speed = player.PositionController.GetCurrentSpeed();

        if (Vector3.Dot(speed, player.transform.forward) <= 0) return 0;

        float distanceWithCompensation =
            Vector3.Project(speed, player.transform.forward).magnitude * 2 * Time.fixedDeltaTime;
        Debug.Log("Distance with compensation: " + distanceWithCompensation);
        return distanceWithCompensation;
    }

    private void OnDrawGizmosSelected()
    {
        float distanceWithCompensation = killDistance + GetCompensation(Player.LocalPlayer);

        Gizmos.color = Color.white;
        Gizmos.DrawRay(transform.position, transform.forward * distanceWithCompensation);
        Gizmos.DrawWireSphere(transform.position + transform.forward * distanceWithCompensation, sphereRadius);
    }

    [TargetRpc]
    private void TargetOnStab(NetworkConnectionToClient opponent)
    {
        OnStab?.Invoke(gameObject, EventArgs.Empty);
    }

    private bool CanStab()
    {
        return Time.time - _lastStabTime >= delaySeconds;
    }

    [Command(requiresAuthority = false)]
    public void CmdSetCanStealTraps(bool canSteal)
    {
        CanStealTraps = canSteal;
    }
}