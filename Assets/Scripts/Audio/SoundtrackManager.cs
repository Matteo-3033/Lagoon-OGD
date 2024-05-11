using Round;
using UnityEngine;

namespace Audio
{
    [RequireComponent(typeof(AudioSource))]
    public class SoundtrackManager : MonoBehaviour
    {
        private AudioSource source;

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