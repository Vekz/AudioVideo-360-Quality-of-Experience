using System.Collections.Generic;

public class EvaluationSettingsData
{
    public string ParticipantNumber { get; set; }
    public int CurrentTrialNumber { get; set; }
    public int NumberOfTrials { get; set; }
    public List<ControlPlaybackDto> ControlPlaybackSettings { get; set; }
    public List<TestPlaybackDto> TestPlaybackSettings { get; set; }
    public int CurrentTestRecordingIndex { get; set; }
}
