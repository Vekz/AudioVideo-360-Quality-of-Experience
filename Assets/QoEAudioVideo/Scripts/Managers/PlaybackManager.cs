using System.Collections.Generic;
using System.Linq;
using OscJack;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Video;

public class PlaybackManager : MonoBehaviour
{
    public EvaluationCoordinator Coordinator = null;
    public GameObject Camera = null;

    [Header("OSC")]
    public string IPAddress = "127.0.0.1";
    public int DAWControlPort = 9101;
    public int SceneRotatorPluginPort = 9100;

    private VideoManager _videoManager = null;
    private AudioManager _audioManager = null;

    private void Awake()
    {
        var videoPlayer = GetComponent<VideoPlayer>();

        _videoManager = new(videoPlayer);
        _audioManager = new(IPAddress, DAWControlPort, SceneRotatorPluginPort, Camera.transform);

        _videoManager.PlaybackStopped.AddListener(Coordinator.TransitionToNextState);
        _videoManager.PlaybackStopped.AddListener(_audioManager.ResetAudioControls);

        Coordinator.OnPlayBackStart.AddListener(_videoManager.Play);
        Coordinator.OnPlayBackStart.AddListener(_audioManager.PlayAudioWithVideo);
        Coordinator.OnPlayBackFinish.AddListener(_videoManager.Stop);
        Coordinator.OnPlayBackFinish.AddListener(_audioManager.ResetAudioControls);
    }

    private void OnDestroy()
    {
        Coordinator.OnPlayBackFinish.RemoveListener(_audioManager.ResetAudioControls);
        Coordinator.OnPlayBackFinish.RemoveListener(_videoManager.Stop);
        Coordinator.OnPlayBackStart.RemoveListener(_audioManager.PlayAudioWithVideo);
        Coordinator.OnPlayBackStart.RemoveListener(_videoManager.Play);

        _videoManager.PlaybackStopped.RemoveAllListeners();

        _audioManager.OnDestroy();
    }

    private void FixedUpdate()
    {
        _audioManager.UpdateRotation();
    }
}

public class VideoManager
{
    public UnityEvent PlaybackStopped = new UnityEvent();

    private VideoPlayer _videoPlayer = null;

    public VideoManager(VideoPlayer videoPlayer)
    {
        _videoPlayer = videoPlayer;

        _videoPlayer.loopPointReached += Stopped;
    }

    public void Play(PlaybackDto playback)
    {
        _videoPlayer.clip = playback.VideoClip;
        _videoPlayer.Play();
    }

    public void Stop()
    {
        _videoPlayer.Stop();
    }

    public void Stopped(VideoPlayer vp)
    {
        vp.Stop();
        PlaybackStopped.Invoke();
    }

}

public class AudioManager
{
    private OscClient _dawControlConnection;
    private OscClient _sceneRotatorPluginConnection;
    private Transform _cameraTransform;

    private List<Vector3> _rotationBuffer = new();

    public AudioManager(string ipAddress, int dawControlPort, int sceneRotatorPluginPort, Transform cameraTransform)
    {
        _dawControlConnection = new OscClient(ipAddress, dawControlPort);
        _sceneRotatorPluginConnection = new OscClient(ipAddress, sceneRotatorPluginPort);
        _cameraTransform = cameraTransform;
    }

    public void OnDestroy()
    {
        _sceneRotatorPluginConnection?.Dispose();
        _sceneRotatorPluginConnection = null;

        ResetAudioControls();

        _dawControlConnection?.Dispose();
        _dawControlConnection = null;
    }

    // AudioPlayback
    public void PlayAudioWithVideo(PlaybackDto playback)
    {
        _rotationBuffer.Clear();

        var testPlayback = playback as TestPlaybackDto;
        if (testPlayback?.UseDelay ?? false)
        {
            var numberOfBufferedEntries = Mathf.FloorToInt(testPlayback.RotationDelayTime / (Time.fixedDeltaTime * 1000));

            _rotationBuffer.AddRange(Enumerable.Repeat(_cameraTransform.rotation.eulerAngles, numberOfBufferedEntries));
        }

        ResetAudioControls();

        _dawControlConnection.Send($"/track/{playback.AudioTrackIndex}/solo/toggle");
        _dawControlConnection.Send("/play");
    }

    public void ResetAudioControls()
    {
        _dawControlConnection.Send("/stop");
        _dawControlConnection.Send("/soloreset");
    }

    // SceneRotator 
    public void UpdateRotation()
    {
        _rotationBuffer.Add(_cameraTransform.rotation.eulerAngles);
        var ea_transformRotation = _rotationBuffer.First();
        _rotationBuffer.RemoveAt(0);
        
        _sceneRotatorPluginConnection.Send("/SceneRotator/ypr", ParseAngleToHalfRotation(ea_transformRotation.y), ParseAngleToQuaterRotation(ea_transformRotation.x), ParseAngleToHalfRotation(ea_transformRotation.z));
    }

    private float ParseAngleToHalfRotation(float angle)
        => ParseAngleToRange(angle, 180, 360);

    private float ParseAngleToQuaterRotation(float angle)
        => ParseAngleToRange(angle, 90, 360);

    private float ParseAngleToRange(float angle, float breakPoint, float range)
    {
        angle = angle % range;
        if (angle > breakPoint)
        {
            var modulus = angle % breakPoint;
            return -breakPoint + modulus;
        }

        if (angle < -breakPoint)
        {
            var modulus = Mathf.Abs(angle) % breakPoint;
            return breakPoint - modulus;
        }

        return angle;
    }
}
