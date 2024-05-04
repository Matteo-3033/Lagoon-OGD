using System.Collections;
using System.Collections.Generic;
using Audio;
using UnityEngine;

public class SentinelSoundManager : MonoBehaviour
{
    public void PlaySentinelStepSound()
    {
        SoundManager.Instance?.OnSentinelStep(transform.position);
    }
}