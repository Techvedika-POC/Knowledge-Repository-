using Microsoft.ML;

namespace TrainingPlanParser.Services.Evaluation.MLNet
{
    public static class MLNetModelBuilder
    {
        private static readonly string BaseDir =
            AppDomain.CurrentDomain.BaseDirectory;

        private static readonly string DataDir =
            Path.Combine(BaseDir, "Data");

        private static readonly string CsvPath =
            Path.Combine(DataDir, "training_quality_data.csv");

        private static readonly string ModelPath =
            Path.Combine(BaseDir, "mlnet_model.zip");

        public static void Train()
        {
            Console.WriteLine("🔄 Starting ML.NET training...");
            Console.Out.Flush();

            if (!File.Exists(CsvPath))
                throw new FileNotFoundException($"Training CSV not found: {CsvPath}");

            var mlContext = new MLContext(seed: 42);

            Console.WriteLine("📥 Loading training data...");
            Console.Out.Flush();

            var data = mlContext.Data.LoadFromTextFile<ModelInput>(
                CsvPath, separatorChar: ',', hasHeader: true);

            Console.WriteLine("✂ Splitting train/test data...");
            Console.Out.Flush();

            var split = mlContext.Data.TrainTestSplit(data, testFraction: 0.2);

            Console.WriteLine("🧠 Building ML pipeline...");
            Console.Out.Flush();

            var pipeline =
             mlContext.Transforms.Concatenate(
                 "Features",
                 nameof(ModelInput.CriticalErrors),
                 nameof(ModelInput.StructuralErrors),
                 nameof(ModelInput.MissingFields),
                 nameof(ModelInput.EmptyContent),
                 nameof(ModelInput.Hallucinations),
                 nameof(ModelInput.UncertaintySignals),
                 nameof(ModelInput.Warnings),
                 nameof(ModelInput.RuleScore))
             .Append(
                 mlContext.BinaryClassification.Trainers
                     .SdcaLogisticRegression(
                         labelColumnName: nameof(ModelInput.Label),
                         featureColumnName: "Features",
                         maximumNumberOfIterations: 50));



            Console.WriteLine("🚀 Training model...");
            Console.Out.Flush();

            var model = pipeline.Fit(split.TrainSet);

            Console.WriteLine("📊 Evaluating model...");
            Console.Out.Flush();

            var predictions = model.Transform(split.TestSet);
            var metrics = mlContext.BinaryClassification.Evaluate(
                predictions,
                labelColumnName: nameof(ModelInput.Label));

            Console.WriteLine("=== ML.NET TRAINING METRICS ===");
            Console.WriteLine($"Accuracy : {metrics.Accuracy:P2}");
            Console.WriteLine($"AUC      : {metrics.AreaUnderRocCurve:P2}");
            Console.WriteLine($"F1 Score : {metrics.F1Score:P2}");
            Console.Out.Flush();

            mlContext.Model.Save(model, data.Schema, ModelPath);

            Console.WriteLine($"✔ ML.NET model saved → {ModelPath}");
            Console.Out.Flush();
        }

        public static ITransformer LoadModel(out MLContext mlContext)
        {
            mlContext = new MLContext();

            if (!File.Exists(ModelPath))
            {
                Console.WriteLine("⚙ ML.NET model not found.");
                Console.WriteLine("➡ Training model automatically...");
                Train();
            }

            return mlContext.Model.Load(ModelPath, out _);
        }
    }
}
