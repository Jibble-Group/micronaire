// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Micronaire.LlmEvaluation.Models;
using Micronaire.OverallClaimEvaluation.Models;
using Micronaire.RetrievalClaimEvaluation.Models;

namespace Micronaire.Evaluation.Models;

/// <summary>
/// Report for a question in the evaluation.
/// </summary>
public class QuestionReport
{
    /// <summary>
    /// The question being evaluated.
    /// </summary>
    public required string Question { get; set; }

    /// <summary>
    /// Gets the report from the LLM evaluator.
    /// </summary>
    public required LlmEvaluationReport LlmReport { get; init; }

    /// <summary>
    /// Gets the report from the overall claim evaluator.
    /// </summary>
    public required OverallClaimReport OverallClaimReport { get; init; }

    /// <summary>
    /// Gets the report from the retrieval claim evaluator.
    /// </summary>
    public required RetrievalClaimReport RetrievalClaimReport { get; init; }

    // TODO: Re-add when token efficient method is found. Currently, each question-answer pair uses ~1 million tokens.
    // public required GenerationClaimReport GenerationClaimReport { get; set; }
}