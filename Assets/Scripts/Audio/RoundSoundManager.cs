using System;
using Interaction;
using Interaction.Trap;
using Round;
using Round.Obstacles.TrapPressurePlate;
using UnityEngine;


namespace Audio
{
    public class RoundSoundManager : SoundManager
    {
        public static RoundSoundManager Instance { get; private set; }

        [SerializeField] private AudioClips audioClips;
        
        private Vector3 Target => Player.LocalPlayer?.transform.position ?? Camera.main.transform.position;


        protected override void Awake()
        {
            base.Awake();
            
            if (Instance != null && Instance != this)
            {
                Debug.LogWarning("RoundSoundManager already exists in the scene. Deleting duplicate.");
                Destroy(gameObject);
                return;
            }
            
            Instance = this;
        }

        private void Start()
        {
            if (RoundController.HasLoaded())
                RegisterRoundControllerCallbacks();
            else
                RoundController.OnRoundLoaded += RegisterRoundControllerCallbacks;
            
            if (Player.LocalPlayer != null)
                RegisterPlayerCallbacks(Player.LocalPlayer);
            else 
                Player.OnPlayerSpawned += RegisterPlayerCallbacks;
            
            ChancellorEffectsController.OnEffectEnabled += OnChancellorEffectEnabled;
            TrapPressurePlate.OnStateChanged += OnTrapPressurePlateStateChanged;
            DoorInteractable.OnStateChanged += OnDoorStateChanged;
            TrapDispenserInteractable.OnVendingMachineUsed += OnVendingMachineUsed;
            KillController.OnPlayerKilled += OnPlayerKilled;
        }

        private void RegisterRoundControllerCallbacks()
        {
            RoundController.Instance.OnCountdown += OnCountdown;
            RoundController.Instance.OnRoundStarted += OnRoundStart;
            RoundController.Instance.OnNoWinningCondition += OnError;
        }

        private void RegisterPlayerCallbacks(Player player)
        {
            if (!player.isLocalPlayer)
                return;

            player.Inventory.OnTrapsUpdated += OnTrapsUpdated;
            player.Inventory.OnStatsUpdate += OnStatsUpdated;
            player.Inventory.OnKeyFragmentUpdated += OnKeyFragmentUpdated;
            
            player.StabManager.OnStab += OnStab;
        }

        private void OnKeyFragmentUpdated(object sender, Inventory.OnKeyFragmentUpdatedArgs args)
        {
            if (args.NewValue > args.OldValue)
                PlayClipAtPoint(audioClips.keyFragmentAcquisition, Target);
        }
        
        private void OnStab(object sender, EventArgs e)
        {
            PlayClipAtPoint(audioClips.stab, ((GameObject)sender).transform.position, 1F, true);
        }

        private void OnStatsUpdated(object sender, Inventory.OnStatsUpdatedArgs args)
        {
            if (args.Op != Inventory.InventoryOp.Acquired) return;
            
            if (args.Modifier.isBuff && !args.Modifier.canBeFoundInGame)
            {
                PlayClipAtPoint(
                    args.Player.isLocalPlayer
                        ? audioClips.superBuffActivation
                        : audioClips.superBuffActivationOnOpponent,
                    Target
                );
            } 
            else if (args.Player.isLocalPlayer)
                PlayClipAtPoint(
                    args.Modifier.isBuff ? audioClips.buffActivation : audioClips.debuffActivation,
                    Target
                );
        }

        private void OnTrapsUpdated(object sender, Inventory.OnTrapsUpdatedArgs args)
        {
            if (args.Op == Inventory.InventoryOp.Removed)
                PlayClipAtPoint(audioClips.trapPlacement, Target);
        }

        private void OnCountdown(int obj)
        {
            PlayClipAtPoint(audioClips.countdown, Target);
        }
        
        private void OnRoundStart()
        {
            PlayClipAtPoint(audioClips.roundStart, Target);
        }
        
        private void OnPlayerKilled(Player player)
        {
            PlayClipAtPoint(audioClips.kill, Target);
        }
        
        private void OnChancellorEffectEnabled(object sender, ChancellorEffectsController.OnEffectEnabledArgs args)
        {
            PlayClipAtPoint(audioClips.chancellorAlarm, Target);
        }
        
        private void OnTrapPressurePlateStateChanged(object sender, bool pressed)
        {
            if (!pressed) return;
            
            PlayClipAtPoint(
                audioClips.trapActivation,
                ((MonoBehaviour) sender).transform.position,
                1F,
                true
            );
        }
        
        private void OnDoorStateChanged(object sender, bool open)
        {
            PlayClipAtPoint(
                open ? audioClips.doorOpen : audioClips.doorClose,
                ((MonoBehaviour) sender).transform.position,
                1F,
                true
            );
        }
        
        private void OnVendingMachineUsed(object sender, EventArgs args)
        {
            PlayClipAtPoint(
                audioClips.trapVendingMachine,
                ((MonoBehaviour) sender).transform.position,
                1F,
                true
            );
        }
        
        private void OnError()
        {
            PlayClipAtPoint(audioClips.error, Target);
        }
        
        public void PlayFootstepsSound(Vector3 source, float footstepsVolume = 1F)
        {
            PlayClipAtPoint(audioClips.footsteps, source, footstepsVolume, true);
        }

        private void OnDestroy()
        {
            RoundController.OnRoundLoaded -= RegisterRoundControllerCallbacks;
            ChancellorEffectsController.OnEffectEnabled -= OnChancellorEffectEnabled;
            Player.OnPlayerSpawned -= RegisterPlayerCallbacks;
            TrapPressurePlate.OnStateChanged -= OnTrapPressurePlateStateChanged;
            DoorInteractable.OnStateChanged -= OnDoorStateChanged;
            TrapDispenserInteractable.OnVendingMachineUsed -= OnVendingMachineUsed;
            KillController.OnPlayerKilled -= OnPlayerKilled;
        }
    }
}