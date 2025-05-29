// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json;
using Micronaire.Claims;
using Micronaire.Evaluation.Interfaces;
using Micronaire.Evaluation.Models;
using Micronaire.GroundTruth;
using Micronaire.LlmEvaluation;
using Micronaire.LlmEvaluation.Models;
using Micronaire.OverallClaimEvaluation;
using Micronaire.OverallClaimEvaluation.Models;
using Micronaire.RetrievalClaimEvaluation;
using Micronaire.RetrievalClaimEvaluation.Models;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;

namespace Micronaire.Evaluation;

/// <summary>
/// Evaluates RAG Pipeline responses with respect to a query.
/// </summary>
public class Evaluator(
    ILogger<Evaluator> logger,
    IClaimExtractor claimExtractor,
    ILlmEvaluator llmEvaluator,
    IOverallClaimEvaluator overallClaimEvaluator,
    IRetrievalClaimEvaluator retrievalClaimEvaluator)
    : IEvaluator
{
    /// <inheritdoc/>
    public async Task<EvaluationReport> EvaluateAsync(
        Kernel evaluator,
        IRagPipeline pipeline,
        string groundTruthPath,
        CancellationToken cancellationToken = default)
    {
        var questionReports = new List<QuestionReport>();
        logger.LogInformation("Loading ground truth from {path}", groundTruthPath);
        foreach (var (question, groundTruthAnswer) in GroundTruthLoader.LoadQaDataSet(groundTruthPath))
        {
            logger.LogInformation("Evaluating question: {question}", question);
            var (generatedAnswer, contexts) = await pipeline.GenerateAsync(
                question,
                cancellationToken);
            logger.LogInformation("Generated answer: {generatedAnswer}", generatedAnswer);

            logger.LogInformation("Extracting claims for question: {question}", question);
            var contextClaimsRaw = await Task.WhenAll(
                contexts
                    .Select(c => c.Context)
                    .Select(async context =>
                        (await claimExtractor.ExtractClaimsAsync(evaluator, context, cancellationToken))
                        .Where(c => !c.IsTriplet)));
            var contextClaims = contextClaimsRaw.SelectMany(c => c).ToList();
            var groundTruthAnswerClaimsRaw = await claimExtractor.ExtractClaimsAsync(
                evaluator,
                groundTruthAnswer,
                cancellationToken);
            var groundTruthClaims = groundTruthAnswerClaimsRaw.Where(c => !c.IsTriplet).ToList();
            var generatedClaimsRaw = await claimExtractor.ExtractClaimsAsync(
                evaluator,
                generatedAnswer,
                cancellationToken);
            var generatedClaims = generatedClaimsRaw.Where(c => !c.IsTriplet).ToList();

            logger.LogInformation("Generating reports for question {question}", question);
            var llmReport = await llmEvaluator.EvaluateAsync(
                evaluator,
                question,
                string.Join('\n', contexts.Select(c => c.Context)),
                generatedAnswer,
                groundTruthAnswer,
                cancellationToken);
            var overallClaimReport = await overallClaimEvaluator.EvaluateAsync(
                evaluator,
                generatedClaims,
                groundTruthClaims,
                cancellationToken);
            var retrievalClaimReport = await retrievalClaimEvaluator.EvaluateAsync(
                evaluator,
                groundTruthClaims,
                contextClaims,
                cancellationToken);
            questionReports.Add(
                new QuestionReport
                {
                    Question = question,
                    LlmReport = llmReport,
                    OverallClaimReport = overallClaimReport,
                    RetrievalClaimReport = retrievalClaimReport,
                });
        }

        var evaluationReport = SummarizeQuestionReports(questionReports);
        logger.LogInformation(
            "EvaluationReport: {evaluationReport}",
            JsonSerializer.Serialize(evaluationReport, JsonSerializerOptions.Default));
        return evaluationReport;
    }

    /// <inheritdoc/>
    public async Task<QuestionEvaluationReport> EvaluateQuestionAsync(
        Kernel evaluator,
        string question,
        string generatedAnswer,
        List<string> contexts,
        CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Generating report for question {question}", question);
        var llmReport = await llmEvaluator.EvaluateQuestionAsync(
            evaluator,
            question,
            string.Join('\n', contexts),
            generatedAnswer,
            cancellationToken);
        return new QuestionEvaluationReport { LlmReport = llmReport };
    }

    private static EvaluationReport SummarizeQuestionReports(List<QuestionReport> questionReports)
    {
        // average all the floats in the LLM reports
        var averageLlmReport = new LlmEvaluationReport
        {
            Groundedness = questionReports.Average(q => q.LlmReport.Groundedness),
            Relevance = questionReports.Average(q => q.LlmReport.Relevance),
            Coherence = questionReports.Average(q => q.LlmReport.Coherence),
            Fluency = questionReports.Average(q => q.LlmReport.Fluency),
            RetrievalScore = questionReports.Average(q => q.LlmReport.RetrievalScore),
            Similarity = questionReports.Average(q => q.LlmReport.Similarity),
        };
        var averageOverallClaimReport = new OverallClaimReport
        {
            Precision = questionReports.Average(q => q.OverallClaimReport.Precision),
            Recall = questionReports.Average(q => q.OverallClaimReport.Recall),
            F1Score = questionReports.Average(q => q.OverallClaimReport.F1Score),
        };
        var averageRetrievalClaimReport = new RetrievalClaimReport
        {
            ClaimRecall = questionReports.Average(q => q.RetrievalClaimReport.ClaimRecall),
            ContextPrecision = questionReports.Average(q => q.RetrievalClaimReport.ContextPrecision),
        };

        return new EvaluationReport
        {
            QuestionReports = questionReports,
            AverageLlmReport = averageLlmReport,
            AverageOverallClaimReport = averageOverallClaimReport,
            AverageRetrievalClaimReport = averageRetrievalClaimReport,
        };
    }
}