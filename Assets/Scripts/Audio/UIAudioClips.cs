using UnityEngine;

namespace Audio
{
    [CreateAssetMenu(menuName = "ScriptableObjects/AudioClips/UI", fileName = "UI")]
    public class UIAudioClips: ScriptableObject
    {
        public AudioClip beforeButtonClick;
        public AudioClip afterButtonClick;
    }
}