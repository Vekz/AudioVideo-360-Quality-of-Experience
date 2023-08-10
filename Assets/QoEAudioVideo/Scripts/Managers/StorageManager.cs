using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;

public class StorageManager : MonoBehaviour
{
    public string BaseSavePath = null;

    public EvaluationCoordinator Coordinator = null;
    public TrackingManager TrackingManager = null;
    public EvaluationQualityUI EvaluationQualityUI = null;
    public EvaluationQualityAndSicknessUI EvaluationQualityAndSicknessUI = null;

    private string BaseFolderPath => @$"{_evaluationTrialSettingsData.ParticipantNumber}_{DateTime.UtcNow.Date.ToString(@"dd-MM-yyyy")}";
    private string BaseFileName => @$"{BaseFolderPath}\{_evaluationTrialSettingsData.CurrentTestRecordingIndex}_{_evaluationTrialSettingsData.CurrentTrialNumber}";

    #region Evaluation
    private EvaluationSettingsData _evaluationTrialSettingsData;
    private bool _hasSavedSettings = false;
    #endregion

    #region Tracking
    private bool _isControlPlayback = true;
    private List<TrackingStorage> TrackingDataForStorage = new();
    #endregion

    #region Questionnaires
    private List<QuestionnaireStorage> QuestionnaireDataForStorage = new();
    #endregion

    private void Awake()
    {
        Coordinator.OnPairRandomizerFinish.AddListener(StoreEvaluationSettings);
        Coordinator.OnPlayBackPostFinish.AddListener(StoreHeadtrackings);
        EvaluationQualityUI.ApprovedClicked.AddListener(KeepQuestionnaireAnswers);
        EvaluationQualityAndSicknessUI.ApprovedClicked.AddListener(KeepQuestionnaireAndVRSQAnswers);
        Coordinator.OnFinish.AddListener(SaveQuestionnaireAnswers);
    }

    private void OnDestroy()
    {
        Coordinator.OnFinish.RemoveListener(SaveQuestionnaireAnswers);
        EvaluationQualityAndSicknessUI.ApprovedClicked.RemoveListener(KeepQuestionnaireAndVRSQAnswers);
        EvaluationQualityUI.ApprovedClicked.RemoveListener(KeepQuestionnaireAnswers);
        Coordinator.OnPlayBackPostFinish.RemoveListener(StoreHeadtrackings);
        Coordinator.OnPairRandomizerFinish.RemoveListener(StoreEvaluationSettings);
    }

    #region Storing Tracking
    private void StoreHeadtrackings()
    {
        TrackingDataForStorage.AddRange(
            TrackingManager.GetDataForStorage().TrackingData
                .Select(x => new TrackingStorage
                {
                    TimeStamp = x.TimeStamp,
                    W = x.W,
                    X = x.X,
                    Y = x.Y,
                    Z = x.Z,
                    IsReference = _isControlPlayback
                })
        );

        if (!_isControlPlayback)
            SaveTrackingsToFile();

        _isControlPlayback = !_isControlPlayback;
    }

    private void SaveTrackingsToFile()
    {
        var fileName = @$"{BaseFileName}_Headtracking.csv";
        var savePath = Path.Combine(BaseSavePath, fileName);
        var contentBuilder = new StringBuilder();

        contentBuilder.AppendLine("Timestamp\tW\tX\tY\tZ\tIsReference");
        foreach (var tracking in TrackingDataForStorage)
            contentBuilder.AppendLine($"{tracking.TimeStamp}\t{tracking.W}\t{tracking.X}\t{tracking.Y}\t{tracking.Z}\t{(tracking.IsReference ? 1 : 0)}");

        (new FileInfo(savePath)).Directory.Create();
        File.WriteAllText(savePath, contentBuilder.ToString());

        TrackingDataForStorage.Clear();
    }
    #endregion

    #region Storing Evaluation Settings
    private void StoreEvaluationSettings()
    {
        _evaluationTrialSettingsData = Coordinator.GetDataForStorage();

        if (!_hasSavedSettings)
            SaveEvaluationSettings();
    }

    private void SaveEvaluationSettings()
    {
        var fileName = @$"{BaseFolderPath}\TrialSetup.txt";
        var savePath = Path.Combine(BaseSavePath, fileName);
        var contentBuilder = new StringBuilder();

        contentBuilder.AppendLine($"#{DateTime.UtcNow.Date.ToString(@"dd-MM-yyyy")}\t{_evaluationTrialSettingsData.ParticipantNumber}");
        contentBuilder.AppendLine($"Number of trials: {_evaluationTrialSettingsData.NumberOfTrials}");

        contentBuilder.AppendLine("Control Videos:");
        contentBuilder.AppendLine("Index\tVideo clip name");
        for (int i = 0; i < _evaluationTrialSettingsData.ControlPlaybackSettings.Count; i++)
            contentBuilder.AppendLine($"{i}\t{_evaluationTrialSettingsData.ControlPlaybackSettings[i].VideoClip.name}");

        contentBuilder.AppendLine("Test Videos:");
        contentBuilder.AppendLine("Index\tVideo clip name\tControl index");
        for (int i = 0; i < _evaluationTrialSettingsData.TestPlaybackSettings.Count; i++)
            contentBuilder.AppendLine($"{i}\t{_evaluationTrialSettingsData.TestPlaybackSettings[i].VideoClip.name}\t{_evaluationTrialSettingsData.TestPlaybackSettings[i].ControlIndex}");

        (new FileInfo(savePath)).Directory.Create();
        File.WriteAllText(savePath, contentBuilder.ToString());

        _hasSavedSettings = !_hasSavedSettings;
    }
    #endregion

    #region Storing Questionnaires

    private void KeepQuestionnaireAnswers()
    {
        var answers = EvaluationQualityUI.GetAnswers();
        QuestionnaireDataForStorage.Add(new QuestionnaireStorage
        {
            CurrentTrialNumber = _evaluationTrialSettingsData.CurrentTrialNumber,
            CurrentTestRecordingIndex = _evaluationTrialSettingsData.CurrentTestRecordingIndex,
            QualityQuestionValue = answers.QualityQuestionValue
        });
    }

    private void KeepQuestionnaireAndVRSQAnswers()
    {
        var answers = EvaluationQualityAndSicknessUI.GetAnswers();
        QuestionnaireDataForStorage.Add(new QuestionnaireStorage
        {
            CurrentTrialNumber = _evaluationTrialSettingsData.CurrentTrialNumber,
            CurrentTestRecordingIndex = _evaluationTrialSettingsData.CurrentTestRecordingIndex,
            QualityQuestionValue = answers.QualityQuestionValue,
            SicknessQuestionValues = answers.SicknessQuestionValues
        });
    }

    private void SaveQuestionnaireAnswers()
    {
        var fileName = @$"{BaseFolderPath}\Answers.csv";
        var savePath = Path.Combine(BaseSavePath, fileName);
        var contentBuilder = new StringBuilder();

        contentBuilder.AppendLine("TrialNumber\tTestRecordingIndex\tQ1\tVRSQ1\tVRSQ2\tVRSQ3\tVRSQ4\tVRSQ5\tVRSQ6\tVRSQ7\tVRSQ8\tVRSQ9");
        foreach (var answers in QuestionnaireDataForStorage)
        {
            var commonLine = $"{answers.CurrentTrialNumber}\t{answers.CurrentTestRecordingIndex}\t{answers.QualityQuestionValue}";
            if (answers.SicknessQuestionValues is null)
                contentBuilder.AppendLine($"{commonLine}\t{0}\t{0}\t{0}\t{0}\t{0}\t{0}\t{0}\t{0}\t{0}");
            else
                contentBuilder.AppendLine($"{commonLine}\t{answers.SicknessQuestionValues[0]}\t{answers.SicknessQuestionValues[1]}\t{answers.SicknessQuestionValues[2]}\t{answers.SicknessQuestionValues[3]}\t{answers.SicknessQuestionValues[4]}\t{answers.SicknessQuestionValues[5]}\t{answers.SicknessQuestionValues[6]}\t{answers.SicknessQuestionValues[7]}\t{answers.SicknessQuestionValues[8]}");
        }

        (new FileInfo(savePath)).Directory.Create();
        File.WriteAllText(savePath, contentBuilder.ToString());
    }

    #endregion

    #region StorageModels

    private class TrackingStorage : TrackingData
    {
        public bool IsReference { get; set; }
    }

    private class QuestionnaireStorage : EvaluationQualityAndSicknessAnswerData
    {
        public int CurrentTrialNumber { get; set; }
        public int CurrentTestRecordingIndex { get; set; }
    }

    #endregion
}
