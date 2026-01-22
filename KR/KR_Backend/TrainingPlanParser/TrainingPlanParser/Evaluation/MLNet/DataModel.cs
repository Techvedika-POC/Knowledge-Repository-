using Microsoft.ML.Data;

namespace TrainingPlanParser.Services.Evaluation.MLNet
{
    public class ModelInput
    {

        [LoadColumn(0)]
        public float CriticalErrors { get; set; }

        [LoadColumn(1)]
        public float StructuralErrors { get; set; }

        [LoadColumn(2)]
        public float MissingFields { get; set; }

        [LoadColumn(3)]
        public float EmptyContent { get; set; }

        [LoadColumn(4)]
        public float Hallucinations { get; set; }

        [LoadColumn(5)]
        public float UncertaintySignals { get; set; }

        [LoadColumn(6)]
        public float Warnings { get; set; }

        [LoadColumn(7)]
        public float RuleScore { get; set; }

        [LoadColumn(8), ColumnName("Label")]
        public bool Label { get; set; }
    }

    public class ModelOutput
    {
        [ColumnName("PredictedLabel")]
        public bool Prediction { get; set; }

        // Raw score (log-odds)
        public float Score { get; set; }

        // Probability (0–1)
        public float Probability { get; set; }
    }
}
