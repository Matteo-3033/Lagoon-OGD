using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;


namespace Audio
{
    public class SoundManager : MonoBehaviour
    {
        private const string PLAYER_PREFS_SOUND_EFFECTS_VOLUME = "SoundEffectsVolume";
        
        [SerializeField] protected float maxDistance = 40F;

        private float baseVolume = 1f;

        protected virtual void Awake()
        {
            baseVolume = PlayerPrefs.GetFloat(PLAYER_PREFS_SOUND_EFFECTS_VOLUME, 1f);
        }

        protected void PlayClipAtPoint(IReadOnlyList<AudioClip> audioClipArray, Vector3 position, float volumeMultiplier = 1f, bool threeD = false)
        {
            if (audioClipArray.Count > 0)
                PlayClipAtPoint(audioClipArray[Random.Range(0, audioClipArray.Count)], position, volumeMultiplier, threeD);
        }

        protected void PlayClipAtPoint(AudioClip audioClip, Vector3 position, float volumeMultiplier = 1F, bool threeD = false)
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