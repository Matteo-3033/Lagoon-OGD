using UnityEngine;
using UnityEngine.Serialization;

namespace Audio
{
    [CreateAssetMenu(menuName = "ScriptableObjects/AudioClips", fileName = "AudioClips")]
    public class AudioClips : ScriptableObject
    {
        public AudioClip chancellorAlarm;
        public AudioClip[] sentinelSteps;
        public AudioClip sentinelAlarm;
    }
}