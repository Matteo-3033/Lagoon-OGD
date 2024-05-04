using System;
using System.Collections.Generic;
using Interaction;
using Interaction.Trap;
using Round;
using Round.Obstacles.TrapPressurePlate;
using TrapModifiers;
using UnityEngine;
using Random = UnityEngine.Random;


namespace Audio
{
    public class SoundManager : MonoBehaviour
    {
        private const string PLAYER_PREFS_SOUND_EFFECTS_VOLUME = "SoundEffectsVolume";

        public static SoundManager Instance { get; private set; }

        [SerializeField] private float maxDistance = 40F;
        [SerializeField] private AudioClips audioClips;
        
        private float baseVolume = 1f;
        
        private Vector3 Target => Player.LocalPlayer?.transform.position ?? Camera.main.transform.position;


        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Debug.LogWarning("SoundManager already exists in the scene. Deleting duplicate.");
                Destroy(gameObject);
                return;
            }
            
            Instance = this;

            baseVolume = PlayerPrefs.GetFloat(PLAYER_PREFS_SOUND_EFFECTS_VOLUME, 1f);
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
            TrapVendingMachineInteractable.OnVendingMachineUsed += OnVendingMachineUsed;
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
        }

        private void OnKeyFragmentUpdated(object sender, Inventory.OnKeyFragmentUpdatedArgs args)
        {
            if (args.NewValue > args.OldValue)
                PlayClipAtPoint(audioClips.keyFragmentAcquisition, Target);
        }

        private void OnStatsUpdated(object sender, Inventory.OnStatsUpdatedArgs args)
        {
            if (args.Enabled)
                PlayClipAtPoint(
                    args.Modifier.isBuff ? audioClips.buffActivation : audioClips.debuffActivation,
                    Target
                );
        }

        private void OnTrapsUpdated(object sender, Inventory.OnTrapsUpdatedArgs args)
        {
            if (args.Op == Inventory.TrapOp.Placed)
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
        
        private void PlayClipAtPoint(IReadOnlyList<AudioClip> audioClipArray, Vector3 position, float volumeMultiplier = 1f, bool threeD = false)
        {
            if (audioClipArray.Count > 0)
                PlayClipAtPoint(audioClipArray[Random.Range(0, audioClipArray.Count)], position, volumeMultiplier, threeD);
        }

        private void PlayClipAtPoint(AudioClip audioClip, Vector3 position, float volumeMultiplier = 1F, bool threeD = false)
        {
            if (audioClip == null)
                return;
            
            var obj = new GameObject("One shot audio");
            obj.transform.position = position;
            
            var audioSource = (AudioSource) obj.AddComponent(typeof (AudioSource));
            audioSource.clip = audioClip;
            audioSource.spatialBlend = threeD ? 1F : 0F;
            audioSource.volume = volumeMultiplier * baseVolume;
            audioSource.maxDistance = maxDistance;
            audioSource.rolloffMode = AudioRolloffMode.Linear;
            audioSource.spatialize = threeD;
            audioSource.spread = threeD ? 360F : 0F;
            
            audioSource.Play();
            
            Destroy(obj, audioClip.length * (Time.timeScale < 0.009999999776482582 ? 0.01f : Time.timeScale));
        }

        private void OnDestroy()
        {
            RoundController.OnRoundLoaded -= RegisterRoundControllerCallbacks;
            ChancellorEffectsController.OnEffectEnabled -= OnChancellorEffectEnabled;
            Player.OnPlayerSpawned -= RegisterPlayerCallbacks;
            TrapPressurePlate.OnStateChanged -= OnTrapPressurePlateStateChanged;
            DoorInteractable.OnStateChanged -= OnDoorStateChanged;
            TrapVendingMachineInteractable.OnVendingMachineUsed -= OnVendingMachineUsed;
        }
        
        public void PlayFootstepsSound(Vector3 source, float footstepsVolume = 1F)
        {
            PlayClipAtPoint(audioClips.footsteps, source, footstepsVolume, true);
        }

        public void ChangeVolume()
        {
            baseVolume += .1f;
            if (baseVolume > 1f)
                baseVolume = 0f;

            PlayerPrefs.SetFloat(PLAYER_PREFS_SOUND_EFFECTS_VOLUME, baseVolume);
            PlayerPrefs.Save();
        }

        public float GetVolume()
        {
            return baseVolume;
        }
    }
}