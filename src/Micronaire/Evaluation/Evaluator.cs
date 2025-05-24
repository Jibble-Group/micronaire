// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.
using Micronaire.Claims;
using Micronaire.GroundTruth;
using Micronaire.LLMEvaluation;
using Micronaire.OverallClaimEvaluation;
using Micronaire.RetrievalClaimEvaluation;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Newtonsoft.Json;

namespace Micronaire;

/// <summary>
/// Evaluates RAG Pipeline responses with respect to a query.
/// </summary>
public class Evaluator : IEvaluator
{
    private readonly ILogger<Evaluator> _logger;
    private readonly ILLMEvaluator _llmEvaluator;
    private readonly IClaimExtractor _claimExtractor;
    private readonly IOverallClaimEvaluator _overallClaimEvaluator;
    private readonly IRetrievalClaimEvaluator _retrievalClaimEvaluator;

    /// <summary>
    /// Initializes a new instance of the <see cref="Evaluator"/> class.
    /// </summary>
    public Evaluator(
        ILogger<Evaluator> logger,
        IClaimExtractor claimExtractor,
        ILLMEvaluator llmEvaluator,
        IOverallClaimEvaluator overallClaimEvaluator,
        IRetrievalClaimEvaluator retrievalClaimEvaluator
    )
    {
        _logger = logger;
        _claimExtractor = claimExtractor;
        _llmEvaluator = llmEvaluator;
        _overallClaimEvaluator = overallClaimEvaluator;
        _retrievalClaimEvaluator = retrievalClaimEvaluator;
    }

    /// <inheritdoc/>
    public async Task<EvaluationReport> EvaluateAsync(
        Kernel evaluator,
        IRagPipeline pipeline,
        string groundTruthPath,
        CancellationToken cancellationToken = default
    )
    {
        var questionReports = new List<QuestionReport>();
        _logger.LogInformation("Loading ground truth from {path}", groundTruthPath);
        foreach (
            var (question, groundTruthAnswer) in GroundTruthLoader.LoadQADataSet(groundTruthPath)
        )
        {
            _logger.LogInformation("Evaluating question: {question}", question);
            var (generatedAnswer, contexts) = await pipeline.GenerateAsync(
                question,
                cancellationToken
            );
            _logger.LogInformation("Generated answer: {generatedAnswer}", generatedAnswer);

            _logger.LogInformation("Extracting claims for question: {question}", question);
            var contextClaimsRaw = await Task.WhenAll(
                contexts
                    .Select(c => c.Context)
                    .Select(async c =>
                        (
                            await _claimExtractor.ExtractClaimsAsync(
                                evaluator,
                                c,
                                cancellationToken
                            )
                        ).Where(c => !c.IsTriplet)
                    )
            );
            var contextClaims = contextClaimsRaw.SelectMany(c => c);
            var groundTruthAnswerClaimsRaw = await _claimExtractor.ExtractClaimsAsync(
                evaluator,
                groundTruthAnswer,
                cancellationToken
            );
            var groundTruthClaims = groundTruthAnswerClaimsRaw.Where(c => !c.IsTriplet);
            var generatedClaimsRaw = await _claimExtractor.ExtractClaimsAsync(
                evaluator,
                generatedAnswer,
                cancellationToken
            );
            var generatedClaims = generatedClaimsRaw.Where(c => !c.IsTriplet);

            _logger.LogInformation("Generating reports for question {question}", question);
            var llmReport = await _llmEvaluator.EvaluateAsync(
                evaluator,
                question,
                string.Join('\n', contexts.Select(c => c.Context)),
                generatedAnswer,
                groundTruthAnswer,
                cancellationToken
            );
            var overallClaimReport = await _overallClaimEvaluator.EvaluateAsync(
                evaluator,
                generatedClaims,
                groundTruthClaims,
                cancellationToken
            );
            var retrievalClaimReport = await _retrievalClaimEvaluator.EvaluateAsync(
                evaluator,
                groundTruthClaims,
                contextClaims,
                cancellationToken
            );
            questionReports.Add(
                new QuestionReport()
                {
                    Question = question,
                    LLMReport = llmReport,
                    OverallClaimReport = overallClaimReport,
                    RetrievalClaimReport = retrievalClaimReport,
                }
            );
        }
        var evaluationReport = SummarizeQuestionReports(questionReports);
        _logger.LogInformation(
            "EvaluationReport: {evaluationReport}",
            JsonConvert.SerializeObject(evaluationReport, Formatting.Indented)
        );
        return evaluationReport;
    }

    private EvaluationReport SummarizeQuestionReports(IEnumerable<QuestionReport> questionReports)
    {
        // average all the floats in the LLM reports
        var averageLLMReport = new LLMEvaluationReport()
        {
            Groundedness = questionReports.Average(q => q.LLMReport.Groundedness),
            Relevance = questionReports.Average(q => q.LLMReport.Relevance),
            Coherence = questionReports.Average(q => q.LLMReport.Coherence),
            Fluency = questionReports.Average(q => q.LLMReport.Fluency),
            RetrievalScore = questionReports.Average(q => q.LLMReport.RetrievalScore),
            Similarity = questionReports.Average(q => q.LLMReport.Similarity),
        };
        var averageOverallClaimReport = new OverallClaimReport()
        {
            Precision = questionReports.Average(q => q.OverallClaimReport.Precision),
            Recall = questionReports.Average(q => q.OverallClaimReport.Recall),
            F1Score = questionReports.Average(q => q.OverallClaimReport.F1Score),
        };
        var averageRetrievalClaimReport = new RetrievalClaimReport()
        {
            ClaimRecall = questionReports.Average(q => q.RetrievalClaimReport.ClaimRecall),
            ContextPrecision = questionReports.Average(q => q.RetrievalClaimReport.ContextPrecision),
        };

        return new()
        {
            QuestionReports = questionReports,
            AverageLLMReport = averageLLMReport,
            AverageOverallClaimReport = averageOverallClaimReport,
            AverageRetrievalClaimReport = averageRetrievalClaimReport,
        };
    }
}
