using Round;
using UnityEngine;
using UnityEngine.Audio;

namespace Audio
{
    [RequireComponent(typeof(AudioSource))]
    public class SoundtrackManager : MonoBehaviour
    {
        [SerializeField] private string audioMixerGroupPitchParam = "PitchBend";
        
        private AudioSource source;
        private bool tempoIncreased;

        private void Awake()
        {
            source = GetComponent<AudioSource>();
        }

        private void Start()
        {
            if (RoundController.HasLoaded())
                OnRoundLoaded();
            else
                RoundController.OnRoundLoaded += OnRoundLoaded;
        }

        private void OnRoundLoaded()
        {
            RoundController.Instance.OnRoundStarted += OnRoundStarted;
            RoundController.Instance.OnRoundEnded += OnRoundEnded;
            RoundController.Instance.TimerUpdate += OnTimerUpdate;
        }

        private void OnTimerUpdate(int remainingTime)
        {
            if (tempoIncreased || remainingTime > 60) return;
            
            tempoIncreased = true;
            source.pitch = 2F;
            source.outputAudioMixerGroup.audioMixer.SetFloat(audioMixerGroupPitchParam, 1f / 2F);
        }

        private void OnRoundStarted()
        {
            source.Play();
        }
        
        private void OnRoundEnded(Player winner)
        {
            source.Stop();
        }

        private void OnDestroy()
        {
            RoundController.OnRoundLoaded -= OnRoundLoaded;
        }
    }
}