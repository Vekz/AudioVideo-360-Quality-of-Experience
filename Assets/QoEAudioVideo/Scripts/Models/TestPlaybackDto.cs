using System;
using UnityEngine;

[Serializable]
public class TestPlaybackDto : PlaybackDto
{
    public int ControlIndex;
    public bool UseDelay;
    [Range(20,2500), Tooltip("DelayTime in miliseconds (ms)")]
    public float RotationDelayTime = 20f;

    public void OnValidate()
    {
        RotationDelayTime = Mathf.Round(RotationDelayTime / 20f) * 20f;
    }
}
