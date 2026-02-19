using Knowledge_Repository.Application.Dtos;
using Knowledge_Repository.Application.Interfaces.Repositories;
using Knowledge_Repository.Application.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using System.Text.Json;
using TrainingPlanParser.Services.Enrichment;
using TrainingPlanParser.Services.Evaluation.Core;
using TrainingPlanParser.Services.Evaluation.RuleBased;
using TrainingPlanParser.Services.Pipeline;
using TrainingPlanParser.Services.Pipeline.Models;

namespace KR_Backend.Controllers
{
    [ApiController]
    [Route("api/training-plans")]
    public class TrainingPlansController : ControllerBase
    {
        private readonly TrainingPlanProcessingPipeline _pipeline;
        private readonly TrainingPlanEnrichmentProcessor _enrichmentProcessor;
        private readonly ITrainingPlanIngestionService _ingestionService;
        private readonly ITrainingPlanRepository _trainingPlanRepository;

        public TrainingPlansController(
            TrainingPlanProcessingPipeline pipeline,
            TrainingPlanEnrichmentProcessor enrichmentProcessor,
            ITrainingPlanIngestionService ingestionService,
            ITrainingPlanRepository trainingPlanRepository)
        {
            _pipeline = pipeline;
            _enrichmentProcessor = enrichmentProcessor;
            _ingestionService = ingestionService;
            _trainingPlanRepository = trainingPlanRepository;
        }

        [HttpGet("{planId}")]
        public async Task<IActionResult> GetTrainingPlan(Guid planId)
        {
            var plan = await _trainingPlanRepository.GetByIdAsync(planId);
            return plan == null ? NotFound() : Ok(plan);
        }

        // ==================================================
        // PHASE 1 — UPLOAD & EVALUATE (NO ENRICHMENT, NO SAVE)
        // ==================================================
        //[HttpPost("upload-docx")]
        //public async Task<IActionResult> UploadTrainingPlanDocx(IFormFile file)
        //{
        //    if (file == null || file.Length == 0)
        //    {
        //        return BadRequest(new
        //        {
        //            Success = false,
        //            ErrorCode = "NO_FILE",
        //            Message = "No document uploaded.",
        //            Details = "Please upload a valid DOCX file containing a learning plan."
        //        });
        //    }

        //    if (!file.FileName.EndsWith(".docx"))
        //    {
        //        return BadRequest(new
        //        {
        //            Success = false,
        //            ErrorCode = "INVALID_FILE_TYPE",
        //            Message = "Unsupported file format.",
        //            Details = "Only DOCX files are supported."
        //        });
        //    }

        //    string tempPath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid()}.docx");

        //    try
        //    {
        //        await using (var stream = System.IO.File.Create(tempPath))
        //            await file.CopyToAsync(stream);

        //        TrainingPlanPipelineResult result =
        //            await _pipeline.ExecuteAsync(tempPath);

        //        return Ok(new
        //        {
        //            Phase = "Evaluation",

        //            Summary = new
        //            {
        //                IsValid = result.IsValid,
        //                Score = result.FinalScore
        //            },

        //            // 🔥 RAW LLM STRUCTURE (DEBUG / TRANSPARENCY)
        //            LlmStructuredOutput = result.LlmJson,

        //            RuleBased = result.RuleBased,
        //            ML = result.ML,
        //            Hybrid = result.Hybrid,

        //            Payload = new
        //            {
        //                result.RawText,
        //                result.StructuredText,
        //                result.LlmJson
        //            }
        //        });

        //    }
        //    catch (JsonException)
        //    {
        //        return BadRequest(new
        //        {
        //            Success = false,
        //            ErrorCode = "INVALID_JSON",
        //            Message = "Generated content could not be validated.",
        //            Details = "The learning plan structure is incomplete or malformed."
        //        });
        //    }
        //    catch (InvalidOperationException ex)
        //    {
        //        return BadRequest(new
        //        {
        //            Success = false,
        //            ErrorCode = "INVALID_CONTENT",
        //            Message = "The document content is not a valid learning plan.",
        //            Details = ex.Message
        //        });
        //    }
        //    catch (Exception)
        //    {
        //        return StatusCode(500, new
        //        {
        //            Success = false,
        //            ErrorCode = "PIPELINE_FAILURE",
        //            Message = "We couldn't process your learning plan.",
        //            Details = "Please try again or contact support if the issue persists."
        //        });
        //    }
        //    finally
        //    {
        //        if (System.IO.File.Exists(tempPath))
        //            System.IO.File.Delete(tempPath);
        //    }
        //}


        // ==================================================
        // PHASE 2 — USER-TRIGGERED ENRICHMENT
        // ==================================================

        [HttpPost("upload-docx")]
        public async Task<IActionResult> UploadTrainingPlanDocx(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest(new { Error = "NO_FILE" });

            if (!file.FileName.EndsWith(".docx", StringComparison.OrdinalIgnoreCase))
                return BadRequest(new { Error = "INVALID_FILE_TYPE" });

            string tempPath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid()}.docx");

            try
            {
                await using (var stream = new FileStream(
                    tempPath,
                    FileMode.Create,
                    FileAccess.Write,
                    FileShare.None))
                {
                    await file.CopyToAsync(stream);
                }
                var result = await _pipeline.ExecuteAsync(tempPath);
                var response = new EvaluationReportDto
                {
                    Phase = "Evaluation",

                    Summary = new
                    {
                        IsValid = result.IsValid,
                        FinalScore = result.FinalScore
                    },

                    RuleBased = new
                    {
                        Engine = "Rule-Based Validation",
                        IsValid = result.RuleBased.IsValid,
                        Score = result.RuleBased.Score,

                        Errors = result.RuleBased.Errors,
                        Warnings = result.RuleBased.Warnings,
                        MissingFields = result.RuleBased.MissingFields,

                        ExpectedStructure = new
                        {
                            Weeks = result.RuleBased.ExpectedData.Weeks,
                            ModulesPerWeek = result.RuleBased.ExpectedData.ModulesPerWeek
                        }
                    },

                    ML = new
                    {
                        Engine = "ML.NET Classifier",
                        IsValid = result.ML.IsValid,
                        Score = result.ML.Score,
                        Metrics = result.ML.Metrics
                            .ToDictionary(k => k.Key, v => Convert.ToDouble(v.Value))
                    },

                    Hybrid = new
                    {
                        Engine = "Hybrid Evaluator",
                        IsValid = result.Hybrid.IsValid,
                        FinalScore = result.Hybrid.Score,
                        Metrics = result.Hybrid.Metrics
                            .ToDictionary(k => k.Key, v => Convert.ToDouble(v.Value))
                    },

                    LlmStructuredOutput = result.LlmJson,

                    Payload = new
                    {
                        result.RawText,
                        result.StructuredText,
                        result.LlmJson
                    }
                };

                Console.WriteLine("=========== PIPELINE RESULT===========");
                Console.WriteLine(JsonSerializer.Serialize(
                    result,
                    new JsonSerializerOptions { WriteIndented = true }));

                Console.WriteLine("=========== API RESPONSE  =========");
                Console.WriteLine(JsonSerializer.Serialize(
                    response,
                    new JsonSerializerOptions { WriteIndented = true }));

                return Ok(response);
            }
            finally
            {
                if (System.IO.File.Exists(tempPath))
                    System.IO.File.Delete(tempPath);
            }
        }


        [HttpPost("enrich")]
        public async Task<IActionResult> EnrichTrainingPlan(
    [FromBody] EnrichmentRequest request)
        {
            try
            {
                var context = new EvaluationContext
                {
                    RawText = request.RawText,
                    StructuredText = request.StructuredText,
                    LlmJson = request.LlmJson
                };

                var result =
           await _enrichmentProcessor.EnrichAndValidateAsync(context);
                return Ok(new
                {
                    Phase = "Enrichment",
                    UpdatedJson = result.EnrichedJson,
                    Evaluation = result.Evaluation,
                    EnrichedFields = result.EnrichedFields
                });



            }
            catch (Exception ex)
            {
                Console.WriteLine(ex); 
                return StatusCode(500, new
                {
                    Success = false,
                    ErrorCode = "ENRICHMENT_FAILED",
                    Message = "Failed to enrich the learning plan.",
                    Details = ex.Message
                });
            }
        }


        private static void NormalizeLessonArrays(JObject root)
        {
            var lessons = root
                .SelectTokens("$.weeks[*].modules[*].lessons[*]")
                .OfType<JObject>();

            foreach (var lesson in lessons)
            {
                NormalizeArrayProperty(lesson, "resources");
                NormalizeArrayProperty(lesson, "assessments");
            }
        }

        private static void NormalizeArrayProperty(JObject obj, string propertyName)
        {
            if (!obj.TryGetValue(propertyName, out var token) || token == null)
            {
                obj[propertyName] = new JArray();
                return;
            }

            if (token.Type != JTokenType.Array)
            {
                obj[propertyName] = new JArray();
                return;
            }
            obj[propertyName] = new JArray(token.OfType<JObject>());
        }



        [HttpPost("persist")]
        public async Task<IActionResult> PersistTrainingPlan(
            [FromBody] JsonElement body)
        {
            string finalLlmJson =
                body.ValueKind == JsonValueKind.String
                    ? body.GetString()!
                    : body.GetRawText();

            if (string.IsNullOrWhiteSpace(finalLlmJson))
            {
                return BadRequest(new
                {
                    Success = false,
                    ErrorCode = "EMPTY_PLAN"
                });
            }

            JObject root;
            try
            {
                root = JObject.Parse(finalLlmJson);
                NormalizeLessonArrays(root);
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    Success = false,
                    ErrorCode = "INVALID_JSON_STRUCTURE",
                    Details = ex.Message
                });
            }

            var safeJson = root.ToString();

            var dto = JsonSerializer.Deserialize<TrainingPlanIngestionDto>(
                safeJson,
                new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

            if (dto == null)
            {
                return BadRequest(new
                {
                    Success = false,
                    ErrorCode = "DESERIALIZATION_FAILED"
                });
            }

            Guid userId = Guid.NewGuid();
            Guid planId = await _ingestionService.IngestTrainingPlanAsync(dto, userId);

            return Ok(new { Success = true, PlanId = planId });
        }



    }
}
