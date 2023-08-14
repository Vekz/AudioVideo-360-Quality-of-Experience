using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public enum EvaluationState : int
{
    EVAL_ENTRY,
    PAIR_RANDOMIZER,
    CONTROL_PLAYBACK,
    TEST_PLAYBACK,
}

public class EvaluationCoordinator : MonoBehaviour
{
    [Header("Participant info")]
    public string ParticipantIdentifier = string.Empty;

    [Header("Recordings Setup")]
    public VideoSetup VideoSetup;

    #region EventSourcing
    [HideInInspector] public UnityEvent OnEvalStart = new();
    [HideInInspector] public RandomizeEvent OnPairRandomizerStart = new();
    [HideInInspector] public UnityEvent OnPairRandomizerFinish = new();
    [HideInInspector] public PlaybackEvent OnPlayBackStart = new();
    [HideInInspector] public UnityEvent OnPlayBackFinish = new();
    [HideInInspector] public UnityEvent OnPlayBackPostFinish = new();
    [HideInInspector] public UnityEvent OnEvalFinish = new();
    [HideInInspector] public UnityEvent OnFinish = new();
    #endregion

    #region StateMachine
    private bool _isEntry = true;
    private bool _stateSetInternally = false;
    private int _maximumTestCases => VideoSetup.TestVideos.Count;
    private int _currentTrialNumber = 0;
    private EvaluationState _previousState = EvaluationState.TEST_PLAYBACK;
    private EvaluationState _currentState = EvaluationState.EVAL_ENTRY;
    #endregion

    #region Randomizer
    private System.Random _random = new();
    private int _maximumRecording => VideoSetup.TestVideos.Count;
    private int _currentTestPlaybackIndex = 0;
    private HashSet<int> _alreadySeenRecordings = new();
    #endregion

    public EvaluationSettingsData GetDataForStorage()
        => new EvaluationSettingsData
        {
            ParticipantNumber = ParticipantIdentifier,
            CurrentTrialNumber = _currentTrialNumber,
            NumberOfTrials = _maximumTestCases,
            ControlPlaybackSettings = VideoSetup.ControlVideos,
            TestPlaybackSettings = VideoSetup.TestVideos,
            CurrentTestRecordingIndex = _currentTestPlaybackIndex
        };

    public void TransitionToNextState()
    {
        _previousState = _currentState;

        var newStateValue = ((int)_currentState + 1) % 4;

        _currentState = (EvaluationState)newStateValue;
    }

    private void Awake()
    {
        OnPairRandomizerStart.AddListener(ChooseRandomTestRecording);
    }

    private void Update()
    {
        if (_previousState == _currentState)
            return;

        switch (_currentState)
        {
            case EvaluationState.EVAL_ENTRY:
                if (!_isEntry)
                {
                    OnPlayBackFinish.Invoke();
                    OnPlayBackPostFinish.Invoke();
                    OnEvalFinish.Invoke();
                }
                OnEvalStart.Invoke();
                break;
            case EvaluationState.PAIR_RANDOMIZER:
                OnPairRandomizerStart.Invoke(_maximumRecording);
                break;
            case EvaluationState.CONTROL_PLAYBACK:
                OnPairRandomizerFinish.Invoke();
                OnPlayBackStart.Invoke(VideoSetup.ControlVideos[VideoSetup.TestVideos[_currentTestPlaybackIndex].ControlIndex]);
                break;
            case EvaluationState.TEST_PLAYBACK:
                OnPlayBackFinish.Invoke();
                OnPlayBackPostFinish.Invoke();
                OnPlayBackStart.Invoke(VideoSetup.TestVideos[_currentTestPlaybackIndex]);
                break;
            default:
                throw new UnityException("Unknown evaluation state!");
        };

#if DEBUG
        Debug.Log($"Changed Evaluation state from {_previousState} to {_currentState}");
#endif

        if (!_stateSetInternally)
            _previousState = _currentState;
        else
            _stateSetInternally = false;

        if (_isEntry)
            _isEntry = false;
    }

    private void OnDestroy()
    {
        OnPairRandomizerStart.RemoveListener(ChooseRandomTestRecording);
    }

    private void ChooseRandomTestRecording(int maximumIndex)
    {
        if (_currentTrialNumber >= _maximumTestCases)
        {

#if DEBUG
            Debug.Log($"Seen maximum recordings. State changed to: {EvaluationState.EVAL_ENTRY}");
#endif
            FinishEvaluation();
            return;
        }

        int predictedIndex;
        do
        {
            predictedIndex = _random.Next(0, maximumIndex);
        }
        while (!_alreadySeenRecordings.Add(predictedIndex));

#if DEBUG
        Debug.Log($"Test recording index: {_currentTestPlaybackIndex} was changed to {predictedIndex}");
#endif

        _currentTestPlaybackIndex = predictedIndex;
        _currentTrialNumber++;
        TransitionToNextState();
        _stateSetInternally = true;
    }

    private void FinishEvaluation()
    {
        _currentState = EvaluationState.EVAL_ENTRY;
        _previousState = EvaluationState.EVAL_ENTRY;
        OnFinish.Invoke();
    }

}
