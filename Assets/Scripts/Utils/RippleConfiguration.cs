using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "ScriptableObjects/RippleEffectConfiguration", fileName = "RippleConfiguration")]
public class RippleConfiguration : ScriptableObject
{
    [Range(.1f, 20)] public float singleRippleDuration;
    [Range(.1f, 20)] public float interval;
    [Space] public float scale;
    public Color rippleColor = Color.white;

    private void OnValidate()
    {
        if (scale <= 0)
        {
            scale = 1;
        }
    }
}