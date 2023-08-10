using System;
using System.Linq;
using UnityEngine;

public class TrackingManager : MonoBehaviour
{
    public EvaluationCoordinator Coordinator = null;
    public Transform CameraTransform = null;

    private TrackingsDataCollection Trackings = new();
    private TrackingsDataCollection PreviousTrackings = new();

    private bool _isTracking = false;

    public TrackingsDataCollection GetDataForStorage()
    {
        // Deep copies trackings for storage
        var serialized = JsonUtility.ToJson(PreviousTrackings);
        PreviousTrackings.Clear();
        return JsonUtility.FromJson<TrackingsDataCollection>(serialized);
    }

    private void Awake()
    {
        Coordinator.OnPlayBackStart.AddListener(ToggleTracking);
        Coordinator.OnPlayBackFinish.AddListener(ToggleTracking);
        Coordinator.OnPlayBackFinish.AddListener(PreserveTrackings);
#if DEBUG
        Coordinator.OnPlayBackFinish.AddListener(DebugLog);
#endif
    }

    private void Update()
    {
        if (_isTracking)
            GatherTrackingData();
    }

    private void OnDestroy()
    {
#if DEBUG
        Coordinator.OnPlayBackFinish.RemoveListener(DebugLog);
#endif
        Coordinator.OnPlayBackFinish.RemoveListener(PreserveTrackings);
        Coordinator.OnPlayBackFinish.RemoveListener(ToggleTracking);
        Coordinator.OnPlayBackStart.RemoveListener(ToggleTracking);
    }

    private void ToggleTracking(PlaybackDto _)
        => ToggleTracking();

    private void ToggleTracking()
        => _isTracking = !_isTracking;

    private void PreserveTrackings()
    {
        Trackings.CopyTo(PreviousTrackings);
        Trackings.Clear();
    }

    private void GatherTrackingData()
        => Trackings.Add(new TrackingData
        {
            TimeStamp = DateTime.UtcNow,
            W = CameraTransform.rotation.w,
            X = CameraTransform.rotation.x,
            Y = CameraTransform.rotation.y,
            Z = CameraTransform.rotation.z
        });

    private void DebugLog()
    {
        Debug.Log($"First logged tracking: \tTime \"{PreviousTrackings.First().TimeStamp}\" \t| W {PreviousTrackings.First().W.ToString("n2")} \t| X {PreviousTrackings.First().X.ToString("n2")} \t| Y {PreviousTrackings.First().Y.ToString("n2")} \t| Z {PreviousTrackings.First().Z.ToString("n2")}");
        Debug.Log($"Last logged tracking: \tTime \"{PreviousTrackings.Last().TimeStamp}\" \t| W {PreviousTrackings.Last().W.ToString("n2")} \t| X {PreviousTrackings.Last().X.ToString("n2")} \t| Y {PreviousTrackings.Last().Y.ToString("n2")}  \t| Z {PreviousTrackings.Last().Z.ToString("n2")}");
    }
}
