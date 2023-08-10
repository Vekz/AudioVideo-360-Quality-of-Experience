using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class EvaluationQualityUI : MonoBehaviour
{
    public Button ApproveButton;
    public UnityEvent ApprovedClicked = new();

    public Slider QualityQuestion1;

    private void Start()
    {
        ApproveButton.onClick.AddListener(ApproveAnswers);
    }

    private void OnDestroy()
    {
        ApproveButton.onClick.RemoveListener(ApproveAnswers);
    }

    private void ApproveAnswers()
    {
        ApprovedClicked.Invoke();
        gameObject.SetActive(false);
    }

    public void ShowCanvas()
    {
        QualityQuestion1.value = 0;
        gameObject.SetActive(true);
    }

    public EvaluationQualityAnswersData GetAnswers()
        => new EvaluationQualityAnswersData { QualityQuestionValue = QualityQuestion1.value };
}
