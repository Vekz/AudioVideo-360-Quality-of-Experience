using System.Collections.Generic;
using UnityEngine;

public class VideoSetup : MonoBehaviour
{
    public List<ControlPlaybackDto> ControlVideos = new();
    public List<TestPlaybackDto> TestVideos = new();
    
    private void OnValidate(){
        TestVideos.ForEach(x => x.OnValidate());
    }
}
