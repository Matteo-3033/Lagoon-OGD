using System.Collections.Generic;
using Round;
using TrapModifiers;
using UnityEngine;
using Random = UnityEngine.Random;


namespace Audio
{
    public class SoundManager : MonoBehaviour
    {
        private const string PLAYER_PREFS_SOUND_EFFECTS_VOLUME = "SoundEffectsVolume";

        public static SoundManager Instance { get; private set; }
        
        [SerializeField] private AudioClips audioClips;
        
        private float volume = 1f;


        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Debug.LogWarning("SoundManager already exists in the scene. Deleting duplicate.");
                Destroy(gameObject);
                return;
            }
            
            Instance = this;

            volume = PlayerPrefs.GetFloat(PLAYER_PREFS_SOUND_EFFECTS_VOLUME, 1f);
        }

        private void Start()
        {
            if (RoundController.HasLoaded())
                RegisterRoundControllerCallbacks();
            else
                RoundController.OnRoundLoaded += RegisterRoundControllerCallbacks;
            
            ChancellorEffectsController.OnEffectEnabled += OnEffectEnabled;
        }

        private void RegisterRoundControllerCallbacks()
        {
            
        }

        private void OnEffectEnabled(object sender, ChancellorEffectsController.OnEffectEnabledArgs args)
        {
            PlaySound(audioClips.chancellorAlarm, Player.LocalPlayer?.transform.position ?? Vector3.zero);
        }
        
        private void PlaySound(IReadOnlyList<AudioClip> audioClipArray, Vector3 position, float volumeMultiplier = 1f)
        {
            PlaySound(audioClipArray[Random.Range(0, audioClipArray.Count)], position, volumeMultiplier);
        }

        private void PlaySound(AudioClip audioClip, Vector3 position, float volumeMultiplier = 1f)
        {
            AudioSource.PlayClipAtPoint(audioClip, position, volumeMultiplier * volume);
        }

        public void ChangeVolume()
        {
            volume += .1f;
            if (volume > 1f)
                volume = 0f;

            PlayerPrefs.SetFloat(PLAYER_PREFS_SOUND_EFFECTS_VOLUME, volume);
            PlayerPrefs.Save();
        }

        public float GetVolume()
        {
            return volume;
        }

        private void OnDestroy()
        {
            RoundController.OnRoundLoaded -= RegisterRoundControllerCallbacks;
            ChancellorEffectsController.OnEffectEnabled -= OnEffectEnabled;
        }
    }
}