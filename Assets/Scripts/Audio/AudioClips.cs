using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace Audio
{
    [CreateAssetMenu(menuName = "ScriptableObjects/AudioClips", fileName = "AudioClips")]
    public class AudioClips : ScriptableObject
    {
        public AudioClip chancellorAlarm;

        [Header("Sentinel")] public AudioClip[] sentinelSteps;
        public AudioClip sentinelAlarm;
        public AudioClip sentinelEnemyLost;
        public AudioClip sentinelSearchingSound;
    }
}