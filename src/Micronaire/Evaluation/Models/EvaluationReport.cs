// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Micronaire.LlmEvaluation.Models;
using Micronaire.OverallClaimEvaluation.Models;
using Micronaire.RetrievalClaimEvaluation.Models;

namespace Micronaire.Evaluation.Models;

/// <summary>
/// Full evaluation report for the RAG pipeline.
/// </summary>
public class EvaluationReport
{
    /// <summary>
    /// Gets or sets the set of reports for each question in the evaluation.
    /// </summary>
    public required IEnumerable<QuestionReport> QuestionReports { get; set; }

    /// <summary>
    /// Gets or sets the report from the LLM evaluator.
    /// </summary>
    public required LlmEvaluationReport AverageLlmReport { get; set; }

    /// <summary>
    /// Gets or sets the report from the overall claim evaluator.
    /// </summary>
    public required OverallClaimReport AverageOverallClaimReport { get; set; }

    /// <summary>
    /// Gets or sets the report from the retrieval claim evaluator.
    /// </summary>
    public required RetrievalClaimReport AverageRetrievalClaimReport { get; set; }

    // TODO: Re-add when token efficient method is found. Currently, each question-answer pair uses ~1 million tokens.
    // public required GenerationClaimReport AverageGenerationClaimReport { get; set; }
}