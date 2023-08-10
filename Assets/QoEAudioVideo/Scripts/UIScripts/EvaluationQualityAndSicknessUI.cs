using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class EvaluationQualityAndSicknessUI : MonoBehaviour
{
    public Button ApproveButton;
    public UnityEvent ApprovedClicked = new();

    public Slider QualityQuestion1;

    [Header("VRSQ")]
    public Slider SicknessQuestion1;
    public Slider SicknessQuestion2;
    public Slider SicknessQuestion3;
    public Slider SicknessQuestion4;
    public Slider SicknessQuestion5;
    public Slider SicknessQuestion6;
    public Slider SicknessQuestion7;
    public Slider SicknessQuestion8;
    public Slider SicknessQuestion9;

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
        SicknessQuestion1.value = 0;
        SicknessQuestion2.value = 0;
        SicknessQuestion3.value = 0;
        SicknessQuestion4.value = 0;
        SicknessQuestion5.value = 0;
        SicknessQuestion6.value = 0;
        SicknessQuestion7.value = 0;
        SicknessQuestion8.value = 0;
        SicknessQuestion9.value = 0;
        gameObject.SetActive(true);
    }

    public EvaluationQualityAndSicknessAnswerData GetAnswers()
        => new EvaluationQualityAndSicknessAnswerData
        {
            QualityQuestionValue = QualityQuestion1.value,
            SicknessQuestionValues = new[]
            {
                SicknessQuestion1.value,
                SicknessQuestion2.value,
                SicknessQuestion3.value,
                SicknessQuestion4.value,
                SicknessQuestion5.value,
                SicknessQuestion6.value,
                SicknessQuestion7.value,
                SicknessQuestion8.value,
                SicknessQuestion9.value,
            }
        };
}
