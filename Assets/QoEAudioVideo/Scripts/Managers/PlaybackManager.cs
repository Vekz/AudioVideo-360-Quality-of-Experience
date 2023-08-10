using OscJack;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Video;

public class PlaybackManager : MonoBehaviour
{
    public EvaluationCoordinator Coordinator = null;

    [Header("Audio OSC")]
    public string IPAddress = "127.0.0.1";
    public int Port = 9101;

    private VideoManager _videoManager = null;
    private AudioManager _audioManager = null;

    private void Awake()
    {
        var videoPlayer = GetComponent<VideoPlayer>();

        _videoManager = new(videoPlayer);
        _audioManager = new(IPAddress, Port);

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
    private OscClient _audioConnection;

    public AudioManager(string ipAddress, int port)
    {
        _audioConnection = new OscClient(ipAddress, port);
    }

    public void OnDestroy()
    {
        ResetAudioControls();

        _audioConnection?.Dispose();
        _audioConnection = null;
    }

    public void PlayAudioWithVideo(PlaybackDto playback)
    {
        ResetAudioControls();
        _audioConnection.Send($"/track/{playback.AudioTrackIndex}/solo/toggle");
        _audioConnection.Send("/play");
    }

    public void ResetAudioControls()
    {
        _audioConnection.Send("/stop");
        _audioConnection.Send("/soloreset");
    }
}
