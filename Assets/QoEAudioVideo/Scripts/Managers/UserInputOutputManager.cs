using UnityEngine;

public class UserInputOutputManager : MonoBehaviour
{
    public EvaluationCoordinator Coordinator = null;

    public EvaluationStartUI EvaluationStartUI = null;
    public EvaluationFinishedUI EvaluationFinishedUI = null;
    public EvaluationQualityUI EvaluationQualityUI = null;
    public EvaluationQualityAndSicknessUI EvaluationQualityAndSicknessUI = null;

    private void Awake()
    {
        Coordinator.OnEvalStart.AddListener(ShowUI);
        EvaluationStartUI.StartButtonClicked.AddListener(Coordinator.TransitionToNextState);
        EvaluationQualityUI.ApprovedClicked.AddListener(Coordinator.TransitionToNextState);
        EvaluationQualityAndSicknessUI.ApprovedClicked.AddListener(Coordinator.TransitionToNextState);
        Coordinator.OnFinish.AddListener(ShowFinishedUI);
    }

    private void Update()
    {
        HandleXRInput();
#if DEBUG
        HandleKeyboardInput();
#endif
    }

    private void OnDestroy()
    {
        EvaluationQualityAndSicknessUI.ApprovedClicked.RemoveListener(Coordinator.TransitionToNextState);
        EvaluationQualityUI.ApprovedClicked.RemoveListener(Coordinator.TransitionToNextState);
        EvaluationStartUI.StartButtonClicked.RemoveListener(Coordinator.TransitionToNextState);
        Coordinator.OnEvalStart.RemoveListener(ShowUI);
    }

    private void HandleXRInput()
    {

    }

    private void HandleKeyboardInput()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            Coordinator.TransitionToNextState();
        }
    }

    private void ShowUI()
    {
        var settings = Coordinator.GetDataForStorage();

        if (settings.CurrentTrialNumber == 0)
        {
            DisplayStartUI();
        }
        else if (settings.CurrentTrialNumber != settings.NumberOfTrials)
        {
            DisplayQualityAssessmentQuestionnaire();
        }
        else if (settings.CurrentTrialNumber == settings.NumberOfTrials)
        {
            DisplayQualityAndSicknessQuestionnaire();
        }
    }

    private void DisplayStartUI()
    {
        EvaluationStartUI.ShowCanvas();
    }

    private void DisplayQualityAssessmentQuestionnaire()
    {
        EvaluationQualityUI.ShowCanvas();
    }

    private void DisplayQualityAndSicknessQuestionnaire()
    {
        EvaluationQualityAndSicknessUI.ShowCanvas();
    }

    private void ShowFinishedUI()
    {
        EvaluationFinishedUI.ShowCanvas();
    }
}
