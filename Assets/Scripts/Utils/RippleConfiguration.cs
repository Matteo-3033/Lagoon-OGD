using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "ScriptableObjects/RippleEffectConfiguration", fileName = "RippleConfiguration")]
public class RippleConfiguration : ScriptableObject
{
    public float totalDuration;
    public float singleRippleDuration;
    public int repetitions;
    [Space] public float scale;
    public Color rippleColor = Color.white;

    private float _prevTotalDuration;
    private float _prevSingleRippleDuration;
    private int _prevRepetitions;

    private void OnValidate()
    {
        if (_prevTotalDuration != totalDuration)
        {
            _prevSingleRippleDuration = totalDuration / repetitions;
            singleRippleDuration = _prevSingleRippleDuration;
        }
        else if (_prevSingleRippleDuration != singleRippleDuration || _prevRepetitions != repetitions)
        {
            _prevTotalDuration = singleRippleDuration * repetitions;
            totalDuration = _prevTotalDuration;
        }

        if (scale <= 0)
        {
            scale = 1;
        }
    }
}