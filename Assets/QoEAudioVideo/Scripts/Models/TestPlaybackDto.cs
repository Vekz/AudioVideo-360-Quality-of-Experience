using System;
using UnityEngine;

[Serializable]
public class TestPlaybackDto : PlaybackDto
{
    public int ControlIndex;
    public bool UseDelay;
    [Range(20,2500), Tooltip("DelayTime in miliseconds (ms)")]
    public float DelayTime = 20f;

    public void OnValidate()
    {
        DelayTime = Mathf.Round(DelayTime / 20f) * 20f;
    }
}
