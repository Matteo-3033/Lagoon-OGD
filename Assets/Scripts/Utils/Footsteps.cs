using System.Collections;
using Audio;
using Mirror;
using Round;
using UnityEngine;

namespace Utils
{
    public class Footsteps : NetworkBehaviour
    {
        private const float CHECK_DELAY = 0.1F;
        
        [SerializeField] private float delay = 0.3F;
        [SerializeField] private float volume = 0.1F;
        [SerializeField] private GameObject sourceObj;
        
        private Vector3 lastPosition = Vector3.zero;
        private Player player;  // only if sourceObj is player
        private float lastPlayedTime;

        [SyncVar] private bool isSilent;

        public override void OnStartClient()
        {
            base.OnStartClient();

            if (RoundSoundManager.Instance != null)
                StartCoroutine(FootstepsLoop());
        }

        private IEnumerator FootstepsLoop()
        {
            while (RoundController.State < RoundController.RoundState.Started)
                yield return new WaitForSeconds(CHECK_DELAY);
            
            lastPlayedTime = Time.time;

            while (RoundController.State == RoundController.RoundState.Started)
            {
                yield return new WaitForSeconds(CHECK_DELAY);

                if (!isSilent && SourceIsMoving() && Time.time - lastPlayedTime > delay)
                {
                    lastPlayedTime = Time.time;
                    RoundSoundManager.Instance.PlayFootstepsSound(sourceObj.transform.position, volume);
                }
                
                lastPosition = sourceObj.transform.position;
            }
            
            // ReSharper disable once IteratorNeverReturns
        }

        private bool SourceIsMoving()
        {
            return Vector3.Distance(lastPosition, sourceObj.transform.position) > 0.1F;
        }
        
        [Command(requiresAuthority = false)]
        public void SetSilent(bool silent)
        {
            isSilent = silent;
        }
    }
}