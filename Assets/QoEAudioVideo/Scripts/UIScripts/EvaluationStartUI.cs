using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class EvaluationStartUI : MonoBehaviour
{
    public Button StartButton;
    public UnityEvent StartButtonClicked = new();

    private void Start()
    {
        StartButton.onClick.AddListener(StartEvaluation);
    }

    private void OnDestroy()
    {
        StartButton.onClick.RemoveListener(StartEvaluation);
    }

    private void StartEvaluation()
    {
        StartButtonClicked.Invoke();
        gameObject.SetActive(false);
    }

    public void ShowCanvas()
    {
        gameObject.SetActive(true);
    }
}
