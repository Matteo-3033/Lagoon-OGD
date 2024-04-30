using System.Collections;
using Audio;
using Round;
using UnityEngine;

namespace Utils
{
    public class Footsteps : MonoBehaviour
    {
        [SerializeField] private float delay = 0.3F;
        [SerializeField] private float volume = 0.1F;
        [SerializeField] private GameObject sourceObj;
        
        private Vector3 lastPosition = Vector3.zero;

        private void Start()
        {
            if (sourceObj.TryGetComponent<Player>(out var player) && player.isLocalPlayer)
            {
                Debug.Log("Disabling footsteps for local player.");
                gameObject.SetActive(false);
                return;
            }
            
            if (SoundManager.Instance != null)
                StartCoroutine(FootstepsLoop());
        }

        private IEnumerator FootstepsLoop()
        {
            while (RoundController.State >= RoundController.RoundState.Started)
            {
                yield return new WaitForSeconds(delay);

                if (SourceIsMoving())
                    SoundManager.Instance.PlayFootstepsSound(sourceObj.transform.position, volume);
                
                lastPosition = sourceObj.transform.position;
            }
            
            // ReSharper disable once IteratorNeverReturns
        }

        private bool SourceIsMoving()
        {
            return Vector3.Distance(lastPosition, sourceObj.transform.position) > 0.1F;
        }
    }
}