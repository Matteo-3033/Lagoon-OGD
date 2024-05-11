using Round;
using UnityEngine;

namespace Interaction.Teleport
{
    [RequireComponent(typeof(AudioSource))]
    public class EndRoundTeleportEffect : MonoBehaviour
    {
        private void OnEnable()
        {
            if (RoundController.HasLoaded())
                RegisterRoundControllerCallbacks();
            else
                RoundController.OnRoundLoaded += RegisterRoundControllerCallbacks;
        }

        private void OnDisable()
        {
            RoundController.OnRoundLoaded -= RegisterRoundControllerCallbacks;
        }

        private void RegisterRoundControllerCallbacks()
        {
            RoundController.Instance.OnRoundEnded += OnRoundEnded;
        }

        private void OnRoundEnded(Player obj)
        {
            var audioSource = GetComponent<AudioSource>();
            audioSource.Pause();
        }
    }
}