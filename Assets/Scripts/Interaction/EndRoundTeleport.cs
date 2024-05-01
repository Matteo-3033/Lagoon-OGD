using Interaction;
using Round;
using UnityEngine;

public class EndRoundTeleport : MonoBehaviour, IInteractable
{
    public string InteractionPrompt => "Exit door";

    [SerializeField] private GameObject enabledState;

    public void Start()
    {
        if (Player.LocalPlayer != null)
            RegisterPlayerCallback(Player.LocalPlayer);
        else Player.OnPlayerSpawned += RegisterPlayerCallback;
        
        enabledState.SetActive(false);
    }

    private void RegisterPlayerCallback(Player player)
    {
        if (player.isLocalPlayer)
            player.Inventory.OnKeyFragmentUpdated += OnKeyFragmentUpdated;
    }

    private void OnKeyFragmentUpdated(object sender, Inventory.OnKeyFragmentUpdatedArgs args)
    {
        enabledState.SetActive(args.NewValue == RoundController.Round.keyFragments);
    }

    public bool StartInteraction(Interactor interactor)
    {
        RoundController.Instance.CheckWinningCondition();
        return true;
    }

    private void OnDestroy()
    {
        Player.OnPlayerSpawned -= RegisterPlayerCallback;
    }
}
