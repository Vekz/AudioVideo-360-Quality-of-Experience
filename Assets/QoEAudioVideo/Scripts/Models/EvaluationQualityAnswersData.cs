public class EvaluationQualityAnswersData
{
    public float QualityQuestionValue { get; set; }
}

public class EvaluationQualityAndSicknessAnswerData : EvaluationQualityAnswersData
{
    public float[] SicknessQuestionValues { get; set; }
}
